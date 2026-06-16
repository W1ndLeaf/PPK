"""Step 3 (Ishod 2 desired + Ishod 3): classify each uploaded recording.

For every recording not yet classified:
  1. read the audio back from MinIO,
  2. POST it to the classify API (multipart field 'file'),
  3. store the full request/response log as JSON in MinIO (LOG_BUCKET),
  4. store normalized results in MongoDB 'classifications', linking each detected
     bird to the matching 'species' document (by GBIF key or scientific name).

The classifier returns {"results": [...]}. We keep the RAW response no matter its
shape, and also extract a best-effort (name, key, confidence) per result so the
report step can aggregate without us hard-coding the exact field names."""
import io
import json
from datetime import datetime, timezone
import requests
import config
from db import get_db, get_minio, ensure_bucket

def _extract(item):
    """Pull a species name + confidence out of one result item.
    The classifier is BirdNET-style and uses snake_case keys:
    scientific_name, common_name, confidence, start_time, end_time, label."""
    if not isinstance(item, dict):
        return {"scientificName": None, "commonName": None, "key": None, "confidence": None, "raw": item}
    name = (item.get("scientific_name") or item.get("scientificName") or item.get("species")
            or item.get("canonicalName") or item.get("name"))
    common = item.get("common_name") or item.get("commonName")
    key = (item.get("key") or item.get("speciesKey")
           or item.get("gbifKey") or item.get("taxonKey") or item.get("usageKey"))
    conf = item.get("confidence", item.get("score", item.get("probability")))
    return {"scientificName": name, "commonName": common, "key": key,
            "confidence": conf, "raw": item}

def _link_species(species, e):
    """Attach the matching species _id/name from our taxonomy collection, if we can find it."""
    sp = None
    if e.get("key") is not None:
        sp = species.find_one({"_id": e["key"]})
    if sp is None and e.get("scientificName"):
        sp = species.find_one({"$or": [
            {"scientificName": e["scientificName"]},
            {"canonicalName": e["scientificName"]},
        ]})
    if sp:
        e["species_key"] = sp["_id"]
        e["matched_name"] = sp.get("scientificName")
    return e

def run():
    db = get_db()
    recordings, classifications, species = db.recordings, db.classifications, db.species
    minio = get_minio()
    ensure_bucket(minio, config.LOG_BUCKET)

    todo = list(recordings.find({"classified": {"$ne": True}}))
    if not todo:
        print("[step3] nothing to classify (all recordings already done).")
        return

    for rec in todo:
        # 1. read audio back from MinIO
        obj = minio.get_object(rec["bucket"], rec["object_name"])
        try:
            audio_bytes = obj.read()
        finally:
            obj.close()
            obj.release_conn()

        # 2. POST to classifier
        resp = requests.post(config.CLASSIFY_URL,
                             files={"file": (rec["filename"], audio_bytes)},
                             timeout=120)
        ctype = resp.headers.get("content-type", "")
        body = resp.json() if ctype.startswith("application/json") else {"raw_text": resp.text}
        results = body.get("results", []) if isinstance(body, dict) else []

        # 3. store the request/response log in MinIO
        log = {
            "recording_id": rec["_id"],
            "filename": rec["filename"],
            "status_code": resp.status_code,
            "requested_at": datetime.now(timezone.utc).isoformat(),
            "response": body,
        }
        log_bytes = json.dumps(log, default=str, ensure_ascii=False, indent=2).encode("utf-8")
        log_object = f"{rec['object_name']}.json"
        minio.put_object(config.LOG_BUCKET, log_object, io.BytesIO(log_bytes),
                         length=len(log_bytes), content_type="application/json")

        # 4. normalize + link to species, save to MongoDB
        norm = [_link_species(species, _extract(item)) for item in results]
        classifications.update_one(
            {"_id": rec["_id"]},
            {"$set": {
                "recording_id": rec["_id"],
                "filename": rec["filename"],
                "location": rec.get("location"),
                "requested_at": datetime.now(timezone.utc),
                "status_code": resp.status_code,
                "raw_response": body,
                "results": norm,
                "num_results": len(norm),
                "log_object": f"{config.LOG_BUCKET}/{log_object}",
            }},
            upsert=True,
        )
        recordings.update_one({"_id": rec["_id"]}, {"$set": {"classified": True}})
        print(f"[step3] {rec['filename']}: {len(norm)} result(s) "
              f"(log -> minio/{config.LOG_BUCKET}/{log_object})")

if __name__ == "__main__":
    run()

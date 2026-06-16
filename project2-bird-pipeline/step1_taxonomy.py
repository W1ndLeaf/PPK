"""Step 1 (Ishod 3, minimal): fetch bird taxonomy -> MongoDB 'species', avoiding duplicates.

The species 'key' (GBIF taxon key) is unique, so we use it as the MongoDB _id and upsert.
Running again won't create duplicates. If the collection is already populated we skip the
download entirely (the assignment says to skip this step if the data already exists)."""
import requests
from pymongo import UpdateOne
import config
from db import get_db

def run(force=False):
    db = get_db()
    species = db.species

    if not force and species.estimated_document_count() > 0:
        print(f"[step1] 'species' already has {species.count_documents({})} docs - skipping download. "
              f"(use --force to refresh)")
        return

    print(f"[step1] downloading taxonomy from {config.AVES_DATA_URL} ...")
    data = requests.get(config.AVES_DATA_URL, timeout=120).json()
    print(f"[step1] got {len(data)} records, upserting (dedup on key)...")

    ops = []
    for rec in data:
        key = rec.get("key")
        if key is None:
            continue
        rec["_id"] = key                       # use GBIF key as the Mongo _id -> no duplicates
        ops.append(UpdateOne({"_id": key}, {"$set": rec}, upsert=True))

    if ops:
        res = species.bulk_write(ops, ordered=False)
        print(f"[step1] upserted={res.upserted_count} modified={res.modified_count} "
              f"total={species.count_documents({})}")

    # indexes that step3 uses to link classification results back to species by name
    species.create_index("scientificName")
    species.create_index("canonicalName")

if __name__ == "__main__":
    run()

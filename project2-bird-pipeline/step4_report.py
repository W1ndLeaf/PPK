"""Write a CSV of every species that got at least one detection.

Skips unmatched results, adds family/order from the taxonomy when available,
averages the confidence and de-duplicates recordings/locations. The optional
--species filter narrows the report by name."""
import csv
import os
import config
from db import get_db

FIELDS = ["species_key", "scientificName", "common_name", "family", "order",
          "num_classifications", "avg_confidence", "recordings", "locations"]

def run(output_dir=None, species_filter=None):
    output_dir = output_dir or config.OUTPUT_DIR
    os.makedirs(output_dir, exist_ok=True)
    db = get_db()
    classifications, species = db.classifications, db.species

    # aggregate by species across all classifications
    stats = {}
    for c in classifications.find():
        for res in c.get("results", []):
            key = res.get("species_key") or res.get("key")
            name = res.get("matched_name") or res.get("scientificName")
            if key is None and not name:
                continue                       # cleaning: drop unmatched detections
            ident = key if key is not None else name
            s = stats.setdefault(ident, {"key": key, "name": name,
                                         "common": res.get("commonName"),
                                         "count": 0, "conf": [], "recs": set(), "locs": set()})
            s["count"] += 1
            if not s["common"] and res.get("commonName"):
                s["common"] = res.get("commonName")
            if res.get("confidence") is not None:
                try:
                    s["conf"].append(float(res["confidence"]))
                except (TypeError, ValueError):
                    pass
            if c.get("filename"):
                s["recs"].add(c["filename"])
            loc = c.get("location") or {}
            if loc.get("lat") is not None:
                s["locs"].add(f"{loc.get('lat')},{loc.get('lon')}")

    rows = []
    for s in stats.values():
        name, family, order = s["name"], None, None
        if s["key"] is not None:
            sp = species.find_one({"_id": s["key"]})
            if sp:
                name = sp.get("scientificName", name)
                family, order = sp.get("family"), sp.get("order")
        if species_filter and species_filter.lower() not in (name or "").lower():
            continue
        conf = s["conf"]
        rows.append({
            "species_key": s["key"],
            "scientificName": name,
            "common_name": s.get("common"),
            "family": family,
            "order": order,
            "num_classifications": s["count"],
            "avg_confidence": round(sum(conf) / len(conf), 4) if conf else "",
            "recordings": ";".join(sorted(s["recs"])),
            "locations": ";".join(sorted(s["locs"])),
        })
    rows.sort(key=lambda r: r["num_classifications"], reverse=True)

    out = os.path.join(output_dir, "bird_report.csv")
    with open(out, "w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=FIELDS)
        w.writeheader()
        w.writerows(rows)
    print(f"[step4] wrote {len(rows)} species row(s) -> {out}")
    if not rows:
        print("[step4] (no positive classifications yet - add real bird audio and run step2+step3)")

if __name__ == "__main__":
    run()

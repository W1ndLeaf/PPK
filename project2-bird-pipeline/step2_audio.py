"""Step 2 (Ishod 2, minimal): upload audio files from a directory -> MinIO,
and store each file's metadata (location, filename, object name) in MongoDB 'recordings'.

Each file is uniquely identifiable in MinIO via an object name = "<uuid>_<filename>".
Re-running won't re-upload a file that's already there (dedup on filename + size)."""
import os
import uuid
import mimetypes
from datetime import datetime, timezone
import config
from db import get_db, get_minio, ensure_bucket

AUDIO_EXTS = {".wav", ".mp3", ".ogg", ".flac", ".m4a", ".aac"}

def run(audio_dir=None, lat=None, lon=None):
    audio_dir = audio_dir or config.AUDIO_DIR
    lat = config.DEFAULT_LAT if lat is None else lat
    lon = config.DEFAULT_LON if lon is None else lon

    db = get_db()
    recordings = db.recordings
    minio = get_minio()
    ensure_bucket(minio, config.AUDIO_BUCKET)

    if not os.path.isdir(audio_dir):
        print(f"[step2] audio dir '{audio_dir}' not found - nothing to upload.")
        return
    files = [f for f in os.listdir(audio_dir)
             if os.path.splitext(f)[1].lower() in AUDIO_EXTS]
    if not files:
        print(f"[step2] no audio files in '{audio_dir}'. Drop .wav/.mp3/.ogg/... there and rerun.")
        return

    for fname in files:
        path = os.path.join(audio_dir, fname)
        size = os.path.getsize(path)

        if recordings.find_one({"filename": fname, "size": size}):
            print(f"[step2] skip (already uploaded): {fname}")
            continue

        object_name = f"{uuid.uuid4().hex}_{fname}"
        ctype = mimetypes.guess_type(fname)[0] or "application/octet-stream"
        minio.fput_object(config.AUDIO_BUCKET, object_name, path, content_type=ctype)

        recordings.insert_one({
            "_id": object_name,
            "filename": fname,
            "object_name": object_name,
            "bucket": config.AUDIO_BUCKET,
            "size": size,
            "content_type": ctype,
            "location": {"lat": lat, "lon": lon},
            "uploaded_at": datetime.now(timezone.utc),
            "classified": False,
        })
        print(f"[step2] uploaded {fname} -> minio/{config.AUDIO_BUCKET}/{object_name}")

if __name__ == "__main__":
    run()

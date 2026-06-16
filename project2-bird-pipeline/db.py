"""Shared clients for MongoDB and MinIO."""
from pymongo import MongoClient
from minio import Minio
import config

def get_db():
    """Return the MongoDB database handle (collections: species, recordings, classifications)."""
    client = MongoClient(config.MONGO_URI)
    return client[config.MONGO_DB]

def get_minio():
    """Return a MinIO (S3) client."""
    return Minio(
        config.MINIO_ENDPOINT,
        access_key=config.MINIO_ACCESS_KEY,
        secret_key=config.MINIO_SECRET_KEY,
        secure=config.MINIO_SECURE,
    )

def ensure_bucket(minio, name):
    """Create the bucket if it does not exist yet (idempotent)."""
    if not minio.bucket_exists(name):
        minio.make_bucket(name)
        print(f"  (created MinIO bucket '{name}')")

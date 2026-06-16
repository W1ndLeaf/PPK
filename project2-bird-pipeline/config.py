"""Central configuration. Values come from environment variables (see .env.example),
falling back to the defaults that match docker-compose.yml so it works out of the box."""
import os

# --- MongoDB (NoSQL store) ---
# The root user is created by docker-compose, so we authenticate against the 'admin' db.
MONGO_URI = os.getenv("MONGO_URI", "mongodb://bird:bird_dev_pw@localhost:27017/?authSource=admin")
MONGO_DB = os.getenv("MONGO_DB", "birds")

# --- MinIO (S3-compatible object storage) ---
MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "localhost:9000")
MINIO_ACCESS_KEY = os.getenv("MINIO_ACCESS_KEY", "birdminio")
MINIO_SECRET_KEY = os.getenv("MINIO_SECRET_KEY", "birdminio_dev_pw")
MINIO_SECURE = os.getenv("MINIO_SECURE", "false").lower() == "true"
AUDIO_BUCKET = os.getenv("AUDIO_BUCKET", "bird-audio")      # uploaded audio files
LOG_BUCKET = os.getenv("LOG_BUCKET", "classify-logs")       # classify request/response logs

# --- Bird data source (mock GBIF site) ---
AVES_BASE = os.getenv("AVES_BASE", "https://aves.regoch.net")
AVES_DATA_URL = f"{AVES_BASE}/aves.json"          # full taxonomy as a JSON array
CLASSIFY_URL = f"{AVES_BASE}/api/classify"        # POST multipart 'file' -> {"results": [...]}

# --- Local paths ---
AUDIO_DIR = os.getenv("AUDIO_DIR", "audio")       # drop your .wav/.mp3 here
OUTPUT_DIR = os.getenv("OUTPUT_DIR", "output")    # CSV report lands here

# --- Geolocation for a batch of audio files ---
# The assignment lets us assume every file in a folder shares one location.
DEFAULT_LAT = float(os.getenv("DEFAULT_LAT", "45.8150"))   # Zagreb
DEFAULT_LON = float(os.getenv("DEFAULT_LON", "15.9819"))

# Pipeline za obradu podataka o pticama

Python + MongoDB + MinIO.

## Koraci
1. step1_taxonomy.py – preuzima vrste ptica u MongoDB
2. step2_audio.py – prenosi audio datoteke u MinIO, metapodatke sprema u MongoDB
3. step3_classify.py – šalje audio klasifikatoru, rezultate sprema u MongoDB, log u MinIO
4. step4_report.py – generira CSV izvještaj

## Pokretanje
Baze:

    docker compose up -d

Python okruženje:

    python -m venv .venv
    .venv\Scripts\python.exe -m pip install -r requirements.txt

Cijeli pipeline:

    .venv\Scripts\python.exe run.py

Pojedini korak:

    .venv\Scripts\python.exe run.py step1

MongoDB: localhost:27017. MinIO konzola: http://localhost:9001. mongo-express: http://localhost:8081.
Audio datoteke se stavljaju u mapu audio/, izvještaj se zapisuje u output/.

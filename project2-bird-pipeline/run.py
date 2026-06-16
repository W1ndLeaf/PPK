"""Pipeline entrypoint. One command, optional step selection and parameters.

Examples:
  python run.py                 # run the whole pipeline (step1 -> step4)
  python run.py step1           # only fetch taxonomy
  python run.py upload --audio-dir audio --lat 45.81 --lon 15.98
  python run.py report --species "Turdus"
"""
import argparse
import step1_taxonomy
import step2_audio
import step3_classify
import step4_report

STEPS = {"step1": "taxonomy", "step2": "upload", "step3": "classify", "step4": "report"}

def main():
    p = argparse.ArgumentParser(description="Bird observation & taxonomy pipeline")
    p.add_argument("step", nargs="?", default="all",
                   choices=["all", "step1", "step2", "step3", "step4",
                            "taxonomy", "upload", "classify", "report"],
                   help="which step to run (default: all)")
    p.add_argument("--audio-dir", help="folder with audio files (default: ./audio)")
    p.add_argument("--lat", type=float, help="latitude for this batch of audio")
    p.add_argument("--lon", type=float, help="longitude for this batch of audio")
    p.add_argument("--species", help="case-insensitive species-name filter for the report")
    p.add_argument("--force", action="store_true", help="re-download taxonomy even if present")
    a = p.parse_args()
    s = a.step

    if s in ("all", "step1", "taxonomy"):
        step1_taxonomy.run(force=a.force)
    if s in ("all", "step2", "upload"):
        step2_audio.run(audio_dir=a.audio_dir, lat=a.lat, lon=a.lon)
    if s in ("all", "step3", "classify"):
        step3_classify.run()
    if s in ("all", "step4", "report"):
        step4_report.run(species_filter=a.species)

if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Re-voices the Thai word sounds with a macOS (Siri) voice.

Usage: python3 Tools/ThaiVoice/revoice.py [--voice Male|Female] [--voice-id ID] [--only KEY_SUBSTRING]
Run from the project root. Texts are the LearnWord values of Assets/Project/Configs/MediaPrompts.asset.
"""
import argparse, os, re, subprocess, sys, tempfile

import yaml

CONFIG = "Assets/Project/Configs/MediaPrompts.asset"
SOUNDS = "Assets/Project/Resources_Bundled/SoundWords"
VOICES = {
    "Male": "com.apple.ttsbundle.gryphon-neural_th-TH-A_th-TH_premium",    # Siri Voice 1
    "Female": "com.apple.ttsbundle.gryphon-neural_th-TH-B_th-TH_premium",  # Siri Voice 2
}
# word sounds must have no silence around, sentences are assembled from them (same as MediaGenerator)
TRIM = "silenceremove=start_periods=1:start_threshold=-40dB:start_silence=0.02:detection=rms:window=0.02,areverse"


def read_words():
    # Unity YAML: drop the !u! tags so that PyYAML can read it
    text = re.sub(r"^--- !u!.*$", "---", open(CONFIG, encoding="utf-8").read(), flags=re.M)
    text = re.sub(r"^%TAG.*$", "", text, flags=re.M)
    sections = yaml.safe_load(text)["MonoBehaviour"]["Sections"]
    items = [item for section in sections for item in section.get("Items") or []]
    return [(item["SoundKey"], str(item["LearnWord"])) for item in items if item.get("SoundKey") and item.get("LearnWord")]


def speech_text(text):
    # "ไป...มา", "ร้าน + ...", "(แล้ว)เจอกันใหม่", "ตรงไป / ตรงมา": ellipses, pluses and brackets are not read aloud, a slash is a pause
    text = re.sub(r"\.{2,}|…|\+|[()\[\]]", " ", text)
    text = re.sub(r"\s*/\s*", ", ", text)
    return re.sub(r"\s+", " ", text).strip(" ,")


def sound_path(sound_key, voice):
    # Thai/Vocabulary/<Section>/<Key> -> SoundWords/Thai/<Voice>/<Section>/<Key>.mp3
    language, _, rest = sound_key.split("/", 2)
    return os.path.join(SOUNDS, language, voice, rest + ".mp3")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--voice", default="Male", choices=VOICES)
    parser.add_argument("--voice-id")
    parser.add_argument("--only", help="re-voice only the keys containing this text")
    args = parser.parse_args()

    words = [(k, t) for k, t in read_words() if k.startswith("Thai/") and (not args.only or args.only in k)]
    tmp = tempfile.mkdtemp(prefix="chang_voice_")
    jobs = [(speech_text(t), os.path.join(tmp, f"{i}.caf"), sound_path(k, args.voice)) for i, (k, t) in enumerate(words)]
    with open(os.path.join(tmp, "jobs.tsv"), "w", encoding="utf-8") as f:
        f.writelines(f"{text}\t{caf}\n" for text, caf, _ in jobs)

    swift = os.path.join(os.path.dirname(os.path.abspath(__file__)), "tts.swift")
    subprocess.run(["swift", swift, args.voice_id or VOICES[args.voice], os.path.join(tmp, "jobs.tsv")], check=True)

    for text, caf, mp3 in jobs:
        os.makedirs(os.path.dirname(mp3), exist_ok=True)
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", caf, "-af", f"{TRIM},{TRIM}",
                        "-ar", "24000", "-ac", "1", "-b:a", "32k", mp3], check=True)
        print(f"{mp3} [{text}]")
    print(f"Sounds created: {len(jobs)}", file=sys.stderr)


if __name__ == "__main__":
    main()

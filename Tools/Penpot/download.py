#!/usr/bin/env python3
"""Downloads the Chang Penpot file as JSON for Chang/Design System/Import (Design/chang-penpot.json).

Usage (from the project root): python3 Tools/Penpot/download.py [--file-id ID] [--out PATH]

The Penpot personal access token is read from the PENPOT_ACCESS_TOKEN environment variable or from
Design/penpot-token.txt (git-ignored). Create it in Penpot: Your account -> Settings -> Access tokens.
The same token file is used by the Unity menu Chang/Design System/Download from Penpot.
"""
import argparse, json, os, subprocess, sys, tempfile

BASE_URL = "https://design.penpot.app"
FILE_ID = "71b39894-c9c5-81cd-8008-ba09a7fc113c"
TOKEN_FILE = "Design/penpot-token.txt"
OUT = "Design/chang-penpot.json"


def read_token():
    token = os.environ.get("PENPOT_ACCESS_TOKEN", "").strip()
    if not token and os.path.exists(TOKEN_FILE):
        token = open(TOKEN_FILE, encoding="utf-8").read().strip()
    if not token:
        sys.exit(f"No Penpot access token: set PENPOT_ACCESS_TOKEN or put it into {TOKEN_FILE} "
                 "(Penpot: Your account -> Settings -> Access tokens)")
    return token


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--file-id", default=FILE_ID)
    parser.add_argument("--out", default=OUT)
    args = parser.parse_args()

    # curl uses the system certificates (the python.org build of Python may have none)
    result = subprocess.run(
        ["curl", "-sS", "--max-time", "300", "-w", "\n%{http_code}",
         "-H", "Accept: application/json", "-H", f"Authorization: Token {read_token()}",
         f"{BASE_URL}/api/rpc/command/get-file?id={args.file_id}"],
        capture_output=True)
    if result.returncode != 0:
        sys.exit(f"curl failed: {result.stderr.decode(errors='replace')}")
    body, _, code = result.stdout.rpartition(b"\n")
    if code != b"200":
        hint = " (the token is wrong or expired)" if code in (b"401", b"403") else ""
        sys.exit(f"Penpot answered {code.decode()}{hint}: {body[:300]!r}")

    # A valid export has the pages; anything else (an error object) must not replace the last good file.
    try:
        data = json.loads(body)
    except ValueError:
        sys.exit(f"Not JSON: {body[:300]!r}")
    pages = data.get("data", {}).get("pagesIndex")
    if not pages:
        sys.exit(f"Not a Penpot file export: {body[:300]!r}")

    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    fd, tmp = tempfile.mkstemp(dir=os.path.dirname(args.out), suffix=".json")
    with os.fdopen(fd, "wb") as f:
        f.write(body)
    os.replace(tmp, args.out)
    print(f"{args.out}: '{data.get('name')}' revision {data.get('revn')}, {len(pages)} pages, {len(body) / 1e6:.1f} MB")


if __name__ == "__main__":
    main()

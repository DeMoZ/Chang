"""Render svg/<Category>/*.svg -> png/<Category>/*.png (1024x1024) and a contact sheet.

An argument is a category (all its pictures) or Category/Key (one picture, no contact sheet)."""
import ctypes.util
import os
import sys

# macOS drops DYLD_FALLBACK_LIBRARY_PATH for processes started by a shell (SIP),
# so cairocffi gets the full path of Homebrew's cairo from find_library
CAIRO = next((lib for lib in ("/opt/homebrew/lib/libcairo.2.dylib", "/usr/local/lib/libcairo.2.dylib") if os.path.exists(lib)), None)
if CAIRO:
    _find_library = ctypes.util.find_library
    ctypes.util.find_library = lambda name: CAIRO if "cairo" in name else _find_library(name)

import cairosvg
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.abspath(__file__))


def render(target):
    category, _, key = target.partition("/")
    src = os.path.join(ROOT, "svg", category)
    dst = os.path.join(ROOT, "png", category)
    os.makedirs(dst, exist_ok=True)
    names = [key] if key else sorted(f[:-4] for f in os.listdir(src) if f.endswith(".svg"))
    failed = 0
    thumbs = []
    for n in names:
        out = os.path.join(dst, n + ".png")
        try:
            cairosvg.svg2png(url=os.path.join(src, n + ".svg"), write_to=out, output_width=1024, output_height=1024)
            im = Image.open(out).convert("RGB")
            if im.size != (1024, 1024):
                raise ValueError(f"bad size {im.size}")
            im.save(out, optimize=True)
            thumbs.append((n, im.resize((200, 200))))
        except Exception as e:
            failed += 1
            print(f"FAIL {category}/{n}: {e}")
    if thumbs and not key:
        cols = 6
        rows = (len(thumbs) + cols - 1) // cols
        sheet = Image.new("RGB", (cols * 210, rows * 230), "white")
        d = ImageDraw.Draw(sheet)
        for i, (n, t) in enumerate(thumbs):
            x, y = (i % cols) * 210 + 5, (i // cols) * 230 + 5
            sheet.paste(t, (x, y))
            d.text((x, y + 203), n[:32], fill="black")
        sheet.save(os.path.join(dst, "_contact.png"))
    print(f"{target}: rendered {len(thumbs)}/{len(names)}")
    return failed


if __name__ == "__main__":
    sys.exit(1 if sum(render(c) for c in sys.argv[1:]) else 0)

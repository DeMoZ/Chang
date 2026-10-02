"""Render svg/<Category>/*.svg -> png/<Category>/*.png (1024x1024) and a contact sheet."""
import os
import sys
import cairosvg
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.abspath(__file__))


def render(category):
    src = os.path.join(ROOT, "svg", category)
    dst = os.path.join(ROOT, "png", category)
    os.makedirs(dst, exist_ok=True)
    names = sorted(f[:-4] for f in os.listdir(src) if f.endswith(".svg"))
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
            print(f"FAIL {category}/{n}: {e}")
    if thumbs:
        cols = 6
        rows = (len(thumbs) + cols - 1) // cols
        sheet = Image.new("RGB", (cols * 210, rows * 230), "white")
        d = ImageDraw.Draw(sheet)
        for i, (n, t) in enumerate(thumbs):
            x, y = (i % cols) * 210 + 5, (i // cols) * 230 + 5
            sheet.paste(t, (x, y))
            d.text((x, y + 203), n[:32], fill="black")
        sheet.save(os.path.join(dst, "_contact.png"))
    print(f"{category}: rendered {len(thumbs)}/{len(names)}")


if __name__ == "__main__":
    for c in sys.argv[1:]:
        render(c)

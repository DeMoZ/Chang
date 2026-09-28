"""Generator for Shopping and Mix word illustrations."""
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
D = "#2e211b"
BG = {
    "beige": ("#f3e6d6", "#c9ab93"),
    "rose": ("#f1e0dc", "#bf9a98"),
    "sage": ("#e7ecdc", "#a9b595"),
    "sky": ("#e3ecf1", "#9fb3c1"),
    "sand": ("#f5ead0", "#cfb27e"),
}
RED, TERRA, MUST, CREAM, OLIVE, LEAF, TEAL, BLUE, PLUM, WOOD, DBROWN = (
    "#c8574b", "#d9825b", "#e0b04f", "#f6ecd8", "#7fa05a", "#5f8f4e", "#4f9a9a",
    "#5d82a8", "#8a5a86", "#9a6a45", "#5c3d2e")
SKIN, SKIN2, SKIN3, HAIR = "#f1c9a5", "#e0a882", "#b57d58", "#3b2a22"
ST = f'stroke="{D}" stroke-linejoin="round" stroke-linecap="round"'


def save(cat, key, bg, body):
    l, d = BG[bg]
    s = (f'<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">\n'
         f'<defs><radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{l}"/>'
         f'<stop offset="100%" stop-color="{d}"/></radialGradient></defs>\n'
         f'<rect width="1024" height="1024" fill="url(#bg)"/>\n{body}\n</svg>\n')
    p = os.path.join(ROOT, "svg", cat)
    os.makedirs(p, exist_ok=True)
    with open(os.path.join(p, key + ".svg"), "w") as f:
        f.write(s)


def G(content, tx=0, ty=0, s=1, rot=0, op=1):
    t = f"translate({tx} {ty})"
    if rot:
        t += f" rotate({rot})"
    if s != 1:
        t += f" scale({s})"
    o = f' opacity="{op}"' if op != 1 else ""
    return f'<g transform="{t}"{o}>{content}</g>'


def shadow(cx, cy, rx, ry=None):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry or rx * 0.16}" fill="{D}" opacity="0.15"/>'


def baht(cx, cy, h, fill, sw):
    """drawn baht sign centered at (cx, cy), cap height h"""
    k = h / 2
    d = (f"M{cx - 0.34 * h} {cy - k} L{cx - 0.34 * h} {cy + k} "
         f"M{cx - 0.34 * h} {cy - k} L{cx + 0.05 * h} {cy - k} Q{cx + 0.3 * h} {cy - k} {cx + 0.3 * h} {cy - 0.26 * h} "
         f"Q{cx + 0.3 * h} {cy} {cx + 0.05 * h} {cy} L{cx - 0.34 * h} {cy} "
         f"M{cx + 0.05 * h} {cy} L{cx + 0.1 * h} {cy} Q{cx + 0.38 * h} {cy} {cx + 0.38 * h} {cy + 0.25 * h} "
         f"Q{cx + 0.38 * h} {cy + k} {cx + 0.1 * h} {cy + k} L{cx - 0.34 * h} {cy + k} "
         f"M{cx - 0.04 * h} {cy - 0.68 * h} L{cx - 0.04 * h} {cy + 0.68 * h}")
    lw = 0.15 * h
    out = ""
    if sw:
        out += f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{lw + 2 * sw}" stroke-linecap="round" stroke-linejoin="round"/>'
    out += f'<path d="{d}" fill="none" stroke="{fill}" stroke-width="{lw}" stroke-linecap="round" stroke-linejoin="round"/>'
    return out


def txt(x, y, size, t, fill=CREAM, sw=None, anchor="middle"):
    if t and set(t) == {"฿"}:
        sw = sw if sw is not None else max(6, size * 0.09)
        h = size * 0.72
        step = size * 0.66
        x0 = x - step * (len(t) - 1) / 2
        return "".join(baht(x0 + i * step, y - h / 2, h, fill, sw * 0.6) for i in range(len(t)))
    sw = sw if sw is not None else max(6, size * 0.09)
    t = t.replace("&", "&amp;")
    return (f'<text x="{x}" y="{y}" font-family="Arial, Helvetica, sans-serif" font-weight="bold" '
            f'font-size="{size}" text-anchor="{anchor}" fill="{fill}" stroke="{D}" stroke-width="{sw}" '
            f'stroke-linejoin="round" paint-order="stroke">{t}</text>')


def limb(d, color, w=60, cap="round"):
    return (f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{w}" stroke-linecap="{cap}" stroke-linejoin="round"/>'
            f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w - 26}" stroke-linecap="{cap}" stroke-linejoin="round"/>')


def hand(x, y, r=34, skin=SKIN):
    return f'<circle cx="{x}" cy="{y}" r="{r}" fill="{skin}" {ST} stroke-width="11"/>'


# ---------------------------------------------------------------- components (local coords)
def shirt(color=RED, s=1, tx=0, ty=0, op=1, rot=0):
    W, w = 14 / s, 9 / s
    c = (f'<path d="M-90 -170 Q0 -130 90 -170 L200 -110 L160 -20 L110 -45 L110 190 L-110 190 L-110 -45 L-160 -20 L-200 -110 Z" '
         f'fill="{color}" {ST} stroke-width="{W}"/>'
         f'<path d="M62 -40 L110 -45 L110 190 L62 190 Z" fill="{D}" opacity="0.13"/>'
         f'<path d="M-82 160 L-82 -20" stroke="#fff" stroke-width="{18 / s}" opacity="0.3" stroke-linecap="round"/>'
         f'<path d="M-90 -170 Q0 -95 90 -170" fill="none" {ST} stroke-width="{w}"/>')
    return G(c, tx, ty, s, rot, op)


def person(tx, ty, s=1, female=True, top=RED, mouth="smile", eyes="dot", skin=SKIN, hair=HAIR, body=True,
           extra_front=""):
    W, w = 14 / s, 10 / s
    c = ""
    if body:
        c += (f'<path d="M-212 480 Q-212 220 0 200 Q212 220 212 480 Z" fill="{top}" {ST} stroke-width="{W}"/>'
              f'<path d="M-72 210 L0 300 L72 210" fill="none" {ST} stroke-width="{w}"/>'
              f'<path d="M-182 460 Q-182 280 -92 240" fill="none" stroke="#fff" stroke-width="{16 / s}" opacity="0.3" stroke-linecap="round"/>')
    c += f'<rect x="-40" y="140" width="80" height="80" fill="{skin}" {ST} stroke-width="{12 / s}"/>'
    if female:
        c += (f'<path d="M-212 20 Q-222 -190 0 -200 Q222 -190 212 20 Q228 200 128 220 L-128 220 Q-228 200 -212 20 Z" '
              f'fill="{hair}" {ST} stroke-width="{W}"/>')
    c += f'<circle cx="0" cy="0" r="170" fill="{skin}" {ST} stroke-width="{W}"/>'
    if female:
        c += f'<path d="M-167 -30 Q-132 -170 0 -170 Q138 -170 168 -30 Q88 -90 0 -100 Q-92 -100 -167 -30 Z" fill="{hair}"/>'
    else:
        c += (f'<path d="M-172 -5 Q-185 -205 0 -198 Q185 -205 172 -5 Q150 -95 70 -105 Q0 -80 -70 -105 Q-150 -95 -172 -5 Z" '
              f'fill="{hair}" {ST} stroke-width="{W}"/>')
    if eyes == "dot":
        c += f'<circle cx="-62" cy="10" r="14" fill="{D}"/><circle cx="62" cy="10" r="14" fill="{D}"/>'
    elif eyes == "up":
        c += f'<circle cx="-56" cy="-4" r="14" fill="{D}"/><circle cx="68" cy="-4" r="14" fill="{D}"/>'
    elif eyes == "happy":
        c += (f'<path d="M-84 16 Q-62 -8 -40 16" fill="none" {ST} stroke-width="{w}"/>'
              f'<path d="M40 16 Q62 -8 84 16" fill="none" {ST} stroke-width="{w}"/>')
    c += f'<circle cx="-97" cy="60" r="26" fill="#e8907f" opacity="0.5"/><circle cx="97" cy="60" r="26" fill="#e8907f" opacity="0.5"/>'
    if mouth == "smile":
        c += f'<path d="M-42 80 Q0 120 42 80" fill="none" {ST} stroke-width="{w}"/>'
    elif mouth == "open":
        c += f'<path d="M-38 78 Q0 140 38 78 Z" fill="#a8453c" {ST} stroke-width="{w}"/>'
    elif mouth == "o":
        c += f'<ellipse cx="0" cy="96" rx="20" ry="24" fill="#a8453c" {ST} stroke-width="{w}"/>'
    elif mouth == "flat":
        c += f'<path d="M-30 92 L30 92" fill="none" {ST} stroke-width="{w}"/>'
    if female:
        c += (f'<circle cx="-172" cy="50" r="12" fill="{MUST}" {ST} stroke-width="{6 / s}"/>'
              f'<circle cx="172" cy="50" r="12" fill="{MUST}" {ST} stroke-width="{6 / s}"/>')
    c += extra_front
    return G(c, tx, ty, s)


def coin(cx, cy, r=70):
    return (f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{MUST}" {ST} stroke-width="12"/>'
            f'<circle cx="{cx}" cy="{cy}" r="{r * 0.72}" fill="none" stroke="{D}" stroke-width="7" opacity="0.5"/>'
            f'<path d="M{cx - r * 0.55} {cy - r * 0.35} Q{cx - r * 0.4} {cy - r * 0.62} {cx - r * 0.1} {cy - r * 0.66}" fill="none" stroke="#fff" stroke-width="9" opacity="0.45" stroke-linecap="round"/>'
            + txt(cx, cy + r * 0.3, r * 0.85, "฿", fill="#f6ecd8", sw=r * 0.12))


def coin_stack(cx, base, n, rx=100, ry=30, h=30, op=1, fill=MUST):
    c = ""
    for i in range(n):
        y = base - i * h
        c += (f'<path d="M{cx - rx} {y - h} L{cx - rx} {y} A{rx} {ry} 0 0 0 {cx + rx} {y} L{cx + rx} {y - h} Z" '
              f'fill="#c8953c" {ST} stroke-width="10"/>')
    top = base - n * h
    c += (f'<ellipse cx="{cx}" cy="{top}" rx="{rx}" ry="{ry}" fill="{fill}" {ST} stroke-width="10"/>'
          f'<ellipse cx="{cx}" cy="{top}" rx="{rx * 0.62}" ry="{ry * 0.6}" fill="none" stroke="{D}" stroke-width="6" opacity="0.4"/>')
    return G(c, op=op)


def banknote(tx, ty, s=1, rot=0, op=1):
    c = (f'<rect x="-170" y="-90" width="340" height="180" rx="14" fill="{OLIVE}" {ST} stroke-width="{13 / s}"/>'
         f'<rect x="-140" y="-62" width="280" height="124" rx="8" fill="none" stroke="{D}" stroke-width="{6 / s}" opacity="0.45"/>'
         f'<circle cx="0" cy="0" r="52" fill="#a9c47f" {ST} stroke-width="{8 / s}"/>'
         + txt(0, 30, 80, "฿", fill=CREAM, sw=8) +
         f'<path d="M-150 -70 L-100 -70" stroke="#fff" stroke-width="{12 / s}" opacity="0.35" stroke-linecap="round"/>')
    return G(c, tx, ty, s, rot, op)


def tag(tx, ty, s=1, rot=0, color=MUST, label="฿", size=110, op=1, lfill=CREAM, string=True):
    c = ""
    if string:
        c += f'<path d="M-95 0 Q-170 -60 -150 -150" fill="none" {ST} stroke-width="{8 / s}"/>'
    c += (f'<path d="M-160 0 L-90 -95 L170 -95 Q185 -95 185 -80 L185 80 Q185 95 170 95 L-90 95 Z" fill="{color}" {ST} stroke-width="{14 / s}"/>'
          f'<path d="M100 -95 L170 -95 Q185 -95 185 -80 L185 80 Q185 95 170 95 L100 95 Z" fill="{D}" opacity="0.12"/>'
          f'<circle cx="-95" cy="0" r="20" fill="{CREAM}" {ST} stroke-width="{9 / s}"/>')
    if label:
        c += txt(45, size * 0.36, size, label, fill=lfill, sw=size * 0.1)
    return G(c, tx, ty, s, rot, op)


def badge(cx, cy, r=80, ok=True):
    col = OLIVE if ok else RED
    sym = (f'<path d="M{cx - r * 0.45} {cy} L{cx - r * 0.1} {cy + r * 0.35} L{cx + r * 0.5} {cy - r * 0.35}" fill="none" stroke="{CREAM}" stroke-width="{r * 0.22}" stroke-linecap="round" stroke-linejoin="round"/>'
           if ok else
           f'<path d="M{cx - r * 0.38} {cy - r * 0.38} L{cx + r * 0.38} {cy + r * 0.38} M{cx + r * 0.38} {cy - r * 0.38} L{cx - r * 0.38} {cy + r * 0.38}" stroke="{CREAM}" stroke-width="{r * 0.22}" stroke-linecap="round"/>')
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{col}" {ST} stroke-width="13"/>' + sym


def bubble(cx, cy, rx, ry, tail, fill=CREAM):
    """speech bubble; tail = (x,y) tip point"""
    tx, ty = tail
    return (f'<path d="M{cx - rx * 0.25} {cy + ry * 0.8} L{tx} {ty} L{cx + rx * 0.2} {cy + ry * 0.9} Z" fill="{fill}" {ST} stroke-width="13"/>'
            f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {ST} stroke-width="13"/>'
            f'<path d="M{cx - rx * 0.24} {cy + ry * 0.72} L{tx} {ty} L{cx + rx * 0.14} {cy + ry * 0.8}" fill="{fill}"/>'
            f'<path d="M{tx} {ty} L{cx - rx * 0.25} {cy + ry * 0.8} M{tx} {ty} L{cx + rx * 0.2} {cy + ry * 0.9}" {ST} stroke-width="13" fill="none"/>')


def think(cx, cy, rx, ry, tail, fill=CREAM):
    tx, ty = tail
    c = ""
    for k, r in ((0.0, 14), (0.4, 22)):
        x = tx + (cx - tx) * (0.25 + k * 0.4)
        y = ty + (cy - ty) * (0.25 + k * 0.4)
        c += f'<circle cx="{x}" cy="{y}" r="{r}" fill="{fill}" {ST} stroke-width="10"/>'
    # cloud
    pts = []
    import math
    n = 9
    for i in range(n):
        a = 2 * math.pi * i / n
        pts.append((cx + rx * math.cos(a), cy + ry * math.sin(a)))
    cl = ""
    for (x, y) in pts:
        cl += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{min(rx, ry) * 0.42:.0f}" fill="{fill}" {ST} stroke-width="13"/>'
    cl += f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}"/>'
    return c + cl


def sparkle(cx, cy, r=40, fill=MUST):
    k = r * 0.28
    return (f'<path d="M{cx} {cy - r} Q{cx + k * 0.4} {cy - k * 0.4} {cx + r} {cy} Q{cx + k * 0.4} {cy + k * 0.4} {cx} {cy + r} '
            f'Q{cx - k * 0.4} {cy + k * 0.4} {cx - r} {cy} Q{cx - k * 0.4} {cy - k * 0.4} {cx} {cy - r} Z" fill="{fill}" {ST} stroke-width="8"/>')


def arrow(d, color=OLIVE, w=34, head=None):
    """thick outlined arrow along path d, head = polygon points string"""
    c = (f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{w + 24}" stroke-linecap="round" stroke-linejoin="round"/>')
    if head:
        c += f'<polygon points="{head}" fill="{color}" {ST} stroke-width="12"/>'
    c += f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"/>'
    if head:
        c += f'<polygon points="{head}" fill="{color}"/>'
    return c


def thumb(tx, ty, s=1, rot=0, sleeve=BLUE, skin=SKIN):
    W = 13 / s
    c = (f'<rect x="-120" y="150" width="210" height="90" rx="14" fill="{sleeve}" {ST} stroke-width="{W}"/>'
         f'<rect x="-110" y="-40" width="190" height="200" rx="50" fill="{skin}" {ST} stroke-width="{W}"/>'
         f'<rect x="-110" y="-230" width="80" height="220" rx="40" fill="{skin}" {ST} stroke-width="{W}"/>'
         f'<path d="M-30 -40 Q-30 20 -110 20" fill="none" {ST} stroke-width="{W}" />'
         f'<path d="M-20 25 L60 25 M-20 75 L65 75 M-20 122 L60 122" {ST} stroke-width="{9 / s}"/>'
         f'<path d="M50 -20 Q72 0 72 40" fill="none" stroke="#fff" stroke-width="{12 / s}" opacity="0.35" stroke-linecap="round"/>')
    return G(c, tx, ty, s, rot)


def palm(tx, ty, s=1, rot=0, skin=SKIN, sleeve=None):
    W = 13 / s
    c = ""
    if sleeve:
        c += f'<rect x="-130" y="150" width="260" height="110" rx="14" fill="{sleeve}" {ST} stroke-width="{W}"/>'
    for x, top in ((-108, -190), (-52, -230), (4, -225), (60, -185)):
        c += f'<rect x="{x}" y="{top}" width="52" height="{top * -1 + 40}" rx="26" fill="{skin}" {ST} stroke-width="{W}"/>'
    c += (f'<path d="M-110 -30 L-110 100 Q-110 170 -30 170 L40 170 Q115 170 115 100 L115 -30 Z" fill="{skin}" {ST} stroke-width="{W}"/>'
          f'<path d="M-106 60 Q-160 20 -190 -40 Q-200 -80 -165 -85 Q-130 -80 -106 -30" fill="{skin}" {ST} stroke-width="{W}"/>'
          f'<rect x="-100" y="-40" width="210" height="40" fill="{skin}"/>'
          f'<path d="M-60 -30 L-60 -5 M-4 -30 L-4 -5 M52 -30 L52 -5" {ST} stroke-width="{7 / s}" opacity="0.5"/>')
    return G(c, tx, ty, s, rot)


def bag(tx, ty, s=1, color=TERRA, op=1, rot=0, logo=True):
    W = 14 / s
    c = (f'<path d="M-80 -140 Q-80 -260 0 -260 Q80 -260 80 -140" fill="none" stroke="{D}" stroke-width="{36 / s}" stroke-linecap="round"/>'
         f'<path d="M-80 -140 Q-80 -260 0 -260 Q80 -260 80 -140" fill="none" stroke="{WOOD}" stroke-width="{16 / s}" stroke-linecap="round"/>'
         f'<path d="M-170 -160 L170 -160 L195 190 L-195 190 Z" fill="{color}" {ST} stroke-width="{W}"/>'
         f'<path d="M110 -160 L170 -160 L195 190 L125 190 Z" fill="{D}" opacity="0.14"/>'
         f'<path d="M-140 -130 L-155 150" stroke="#fff" stroke-width="{16 / s}" opacity="0.3" stroke-linecap="round"/>'
         f'<circle cx="-80" cy="-120" r="14" fill="{D}"/><circle cx="80" cy="-120" r="14" fill="{D}"/>')
    if logo:
        c += f'<path d="M-40 20 C-80 -20 -40 -60 0 -20 C40 -60 80 -20 40 20 L0 60 Z" fill="{CREAM}" {ST} stroke-width="{9 / s}"/>'
    return G(c, tx, ty, s, rot, op)


def star(cx, cy, r, fill=MUST, sw=10):
    import math
    pts = []
    for i in range(10):
        a = -math.pi / 2 + i * math.pi / 5
        rr = r if i % 2 == 0 else r * 0.45
        pts.append(f"{cx + rr * math.cos(a):.1f},{cy + rr * math.sin(a):.1f}")
    return f'<polygon points="{" ".join(pts)}" fill="{fill}" {ST} stroke-width="{sw}"/>'


def hanger(x, y, s=1):
    return (f'<path d="M{x} {y} Q{x} {y - 40 * s} {x + 22 * s} {y - 40 * s} Q{x + 40 * s} {y - 40 * s} {x + 36 * s} {y - 60 * s}" fill="none" {ST} stroke-width="9"/>'
            f'<path d="M{x - 120 * s} {y + 55 * s} L{x} {y} L{x + 120 * s} {y + 55 * s}" fill="none" {ST} stroke-width="11"/>')


def kid(tx, ty, s=1, top=TEAL, pants=DBROWN, female=False, walking=True):
    """full body figure; origin at feet center"""
    c = ""
    if walking:
        c += limb("M-10 -230 L-70 -110 L-90 -10", pants, 64)
        c += limb("M10 -230 L60 -120 L110 -30", pants, 64)
        c += f'<path d="M-125 -12 L-60 -12" stroke="{D}" stroke-width="44" stroke-linecap="round"/>'
        c += f'<path d="M95 -30 L150 -12" stroke="{D}" stroke-width="44" stroke-linecap="round"/>'
        c += limb("M50 -430 L120 -350 L150 -270", SKIN, 50)
    c += (f'<path d="M-85 -470 Q0 -500 85 -470 L95 -220 L-95 -220 Z" fill="{top}" {ST} stroke-width="14"/>'
          f'<path d="M-60 -440 L-65 -250" stroke="#fff" stroke-width="16" opacity="0.3" stroke-linecap="round"/>')
    if walking:
        c += limb("M-50 -430 L-120 -350 L-150 -280", top, 56)
        c += hand(-150, -270, 26)
    c += person(0, -610, 0.62, female=female, body=False)
    return G(c, tx, ty, s)


# ======================================================================== SHOPPING
def shopping():
    C = "Shopping"
    # A classifier for clothes: rack with 3 shirts
    b = (f'<rect x="160" y="200" width="704" height="26" rx="13" fill="{WOOD}" {ST} stroke-width="12"/>'
         f'<path d="M190 226 L190 860 M834 226 L834 860" {ST} stroke-width="22"/>'
         f'<path d="M190 226 L190 860 M834 226 L834 860" stroke="{WOOD}" stroke-width="8"/>'
         + shadow(512, 880, 360, 30))
    for x, col in ((330, RED), (512, TEAL), (694, MUST)):
        b += hanger(x, 280) + shirt(col, 0.44, x, 400)
    for i, x in enumerate((330, 512, 694)):
        b += f'<circle cx="{x}" cy="{620}" r="30" fill="{CREAM}" {ST} stroke-width="10"/>' + txt(x, 636, 44, str(i + 1), fill=D, sw=0)
    save(C, "A classifier for clothes", "sand", b)

    # Bag
    b = shadow(512, 830, 260, 40) + bag(512, 620, 1.15, TERRA) + \
        G(shirt(TEAL, 0.5, 0, 0, rot=-12), 450, 410) + bag(512, 620, 1.15, TERRA)
    b = shadow(512, 840, 270, 40) + G(shirt(TEAL, 0.45), 470, 370, rot=-14) + bag(512, 620, 1.15, TERRA)
    save(C, "Bag", "beige", b)

    # Beautiful: woman with flower, sparkles, hearts
    flower = ""
    for i in range(5):
        import math
        a = i * 2 * math.pi / 5
        flower += f'<circle cx="{150 + 38 * math.cos(a):.0f}" cy="{-150 + 38 * math.sin(a):.0f}" r="30" fill="#e8907f" {ST} stroke-width="9"/>'
    flower += f'<circle cx="150" cy="-150" r="20" fill="{MUST}" {ST} stroke-width="8"/>'
    b = (person(512, 420, 0.95, True, PLUM, "smile", "happy", extra_front=flower)
         + sparkle(230, 260, 50) + sparkle(800, 330, 38) + sparkle(250, 560, 30) + sparkle(790, 610, 46)
         + sparkle(700, 170, 28))
    save(C, "Beautiful", "rose", b)

    # Big / Small
    arr = lambda d, h: arrow(d, TERRA, 22, h)
    b = (shadow(420, 820, 250) + shadow(800, 820, 90)
         + shirt(TEAL, 1.2, 410, 560) + shirt(TEAL, 0.42, 800, 720, op=0.35)
         + arrow("M140 260 L200 320", TERRA, 20, "120,240 180,250 130,300")
         + arrow("M680 260 L620 320", TERRA, 20, "700,240 640,250 690,300"))
    save(C, "Big", "sky", b)
    b = (shadow(360, 820, 250, 36) + shadow(740, 820, 110)
         + shirt(TEAL, 1.1, 350, 560, op=0.35) + shirt(TEAL, 0.5, 740, 700)
         + arrow("M740 440 L740 520", TERRA, 20, "705,500 775,500 740,545")
         )
    save(C, "Small", "sky", b)

    # Brand: clothing label with star on string
    b = (f'<path d="M512 180 Q430 220 470 290" fill="none" {ST} stroke-width="10"/>'
         f'<path d="M340 300 L512 250 L684 300 L684 800 Q684 830 654 830 L370 830 Q340 830 340 800 Z" fill="{CREAM}" {ST} stroke-width="15"/>'
         f'<path d="M600 280 L684 300 L684 800 Q684 830 654 830 L600 830 Z" fill="{D}" opacity="0.1"/>'
         f'<circle cx="512" cy="320" r="24" fill="{MUST}" {ST} stroke-width="10"/>'
         f'<circle cx="512" cy="560" r="130" fill="{RED}" {ST} stroke-width="13"/>'
         + star(512, 565, 100, MUST, 11) +
         f'<path d="M400 740 L624 740 M430 780 L594 780" {ST} stroke-width="12" opacity="0.5"/>'
         + sparkle(760, 380, 40) + sparkle(270, 660, 32))
    save(C, "Brand", "sand", b)

    # Can / Can_ / Cannot
    b = thumb(470, 560, 1.25, sleeve=BLUE) + badge(730, 300, 105, True) + sparkle(260, 300, 36)
    save(C, "Can", "sage", b)
    b = (thumb(400, 600, 1.1, sleeve=BLUE) + badge(470, 230, 70, True)
         + bubble(710, 330, 150, 150, (600, 480)) + txt(710, 400, 210, "?", fill=TERRA, sw=14))
    save(C, "Can_", "sage", b)
    b = thumb(470, 470, 1.25, rot=180, sleeve=BLUE) + badge(730, 720, 105, False)
    save(C, "Cannot", "rose", b)

    # Change to: two shirts swap arrows
    b = (shadow(300, 740, 150) + shadow(724, 740, 150)
         + shirt(RED, 0.72, 300, 590) + shirt(TEAL, 0.72, 724, 590)
         + arrow("M330 330 Q512 170 680 300", MUST, 30, "650,250 730,330 630,350")
         + arrow("M694 800 Q512 930 344 820", MUST, 30, "372,870 290,790 395,770"))
    save(C, "Change to", "beige", b)

    # Cheap / Expensive
    b = (coin_stack(300, 800, 9, op=0.3) + shadow(620, 800, 140)
         + tag(640, 420, 1.0, -8, OLIVE, "฿", 130)
         + coin(620, 700, 85)
         + arrow("M840 560 L840 700", OLIVE, 26, "800,680 880,680 840,740"))
    save(C, "Cheap", "sage", b)
    b = (shadow(420, 850, 220) + coin_stack(420, 840, 12, rx=150, ry=44, h=34)
         + coin_stack(250, 850, 5, rx=90, ry=28, h=30) + banknote(640, 700, 0.9, -10)
         + coin(820, 780, 45, ) if False else "")
    b = (shadow(470, 860, 260)
         + coin_stack(560, 850, 12, rx=130, ry=40, h=32) + coin_stack(360, 860, 7, rx=110, ry=34, h=32)
         + banknote(480, 830, 0.8, -6)
         + tag(420, 280, 0.85, -8, RED, "฿฿฿", 90)
         + arrow("M820 560 L820 400", RED, 26, "780,420 860,420 820,360")
         + coin(840, 780, 45).replace("", "") )
    save(C, "Expensive", "sand", b)

    # Fit perfect: man in shirt with check + sparkles
    b = (person(470, 390, 0.95, False, TEAL, "smile", "happy")
         + f'<path d="M470 620 L470 850" {ST} stroke-width="8" opacity="0.5"/>'
         + ''.join(f'<circle cx="470" cy="{y}" r="9" fill="{CREAM}" {ST} stroke-width="6"/>' for y in (700, 780))
         + badge(770, 280, 90, True) + sparkle(210, 250, 40) + sparkle(820, 560, 34) + sparkle(180, 560, 28))
    save(C, "Fit perfect", "sky", b)

    # How much: person with bubble ฿?
    b = (person(340, 530, 0.78, True, TERRA, "o", "up")
         + bubble(680, 300, 200, 150, (520, 440)) + txt(620, 370, 190, "฿", fill=MUST, sw=14) + txt(760, 370, 190, "?", fill=TERRA, sw=14))
    save(C, "How much", "rose", b)

    # I_ll take: basket with shirt going in + check
    b = (shadow(512, 850, 280)
         + shirt(RED, 0.55, 500, 360, rot=10)
         + arrow("M720 300 L720 420", OLIVE, 24, "685,405 755,405 720,455")
         + f'<path d="M300 560 Q512 380 724 560" fill="none" stroke="{D}" stroke-width="36" stroke-linecap="round"/>'
         + f'<path d="M300 560 Q512 380 724 560" fill="none" stroke="{WOOD}" stroke-width="16" stroke-linecap="round"/>'
         + f'<path d="M230 560 L794 560 L740 840 L284 840 Z" fill="{MUST}" {ST} stroke-width="15"/>'
         + ''.join(f'<path d="M{x} 580 L{x - (x - 512) * 0.1:.0f} 820" {ST} stroke-width="9" opacity="0.5"/>' for x in (330, 420, 512, 604, 694))
         + f'<path d="M250 660 L774 660 M268 750 L756 750" {ST} stroke-width="9" opacity="0.5"/>'
         + badge(790, 760, 80, True))
    save(C, "I_ll take_I want to buy_order", "beige", b)

    # Kilogram: scale with kg
    b = (shadow(512, 860, 280)
         + f'<path d="M300 420 L724 420 L770 850 L254 850 Z" fill="{RED}" {ST} stroke-width="15"/>'
         + f'<path d="M650 420 L724 420 L770 850 L680 850 Z" fill="{D}" opacity="0.14"/>'
         + f'<circle cx="512" cy="640" r="150" fill="{CREAM}" {ST} stroke-width="14"/>'
         + ''.join(f'<path d="M{512 + 120 * c:.0f} {640 - 120 * s_:.0f} L{512 + 140 * c:.0f} {640 - 140 * s_:.0f}" {ST} stroke-width="8"/>'
                   for c, s_ in ((-0.94, 0.34), (-0.7, 0.7), (0, 1), (0.7, 0.7), (0.94, 0.34)))
         + f'<path d="M512 640 L600 540" stroke="{D}" stroke-width="24" stroke-linecap="round"/>'
         + f'<path d="M512 640 L600 540" stroke="{RED}" stroke-width="10" stroke-linecap="round"/>'
         + f'<circle cx="512" cy="640" r="16" fill="{D}"/>'
         + txt(512, 740, 80, "kg", fill=D, sw=0)
         + f'<rect x="490" y="370" width="44" height="50" fill="{DBROWN}" {ST} stroke-width="10"/>'
         + f'<path d="M260 370 Q512 400 764 370 L740 330 L284 330 Z" fill="{CREAM}" {ST} stroke-width="14"/>'
         # bananas / fruits on plate
         + f'<circle cx="420" cy="290" r="60" fill="{TERRA}" {ST} stroke-width="12"/>'
         + f'<circle cx="530" cy="270" r="66" fill="{OLIVE}" {ST} stroke-width="12"/>'
         + f'<circle cx="630" cy="295" r="54" fill="{MUST}" {ST} stroke-width="12"/>'
         + f'<path d="M530 204 Q540 180 560 176" fill="none" {ST} stroke-width="9"/>')
    save(C, "Kilogram", "sand", b)

    # Look_see: big eye looking at shirt
    b = (f'<path d="M140 470 Q300 290 460 470 Q300 650 140 470 Z" fill="{CREAM}" {ST} stroke-width="15"/>'
         f'<circle cx="330" cy="470" r="78" fill="{TEAL}" {ST} stroke-width="12"/>'
         f'<circle cx="345" cy="470" r="36" fill="{D}"/><circle cx="325" cy="445" r="14" fill="#fff"/>'
         f'<path d="M160 390 L130 350 M230 345 L215 300 M310 330 L310 282 M390 345 L405 300" {ST} stroke-width="12"/>'
         + f'<path d="M490 420 L600 390 M500 470 L610 470 M490 520 L600 550" {ST} stroke-width="12" stroke-dasharray="20 22" opacity="0.6"/>'
         + shadow(760, 700, 130) + shirt(RED, 0.62, 760, 560))
    save(C, "Look_see", "sky", b)

    # Lower: faded high tag, curved arrow, highlighted low tag
    b = (tag(360, 300, 0.8, -6, RED, "฿฿฿", 85, op=0.35, string=False)
         + tag(620, 690, 0.95, -6, OLIVE, "฿", 120, string=False)
         + arrow("M620 260 Q800 330 790 500", OLIVE, 28, "750,480 830,480 795,560"))
    save(C, "Lower", "sage", b)

    # May I: person raising hand + ? bubble
    b = (limb("M470 700 L560 560 L600 380", PLUM, 64) + hand(605, 360, 36)
         + person(380, 480, 0.8, True, PLUM, "smile", "dot")
         + bubble(740, 280, 110, 110, (660, 400)) + txt(740, 330, 150, "?", fill=TERRA, sw=12))
    # arm should be in front of body: redraw order
    b = (person(380, 480, 0.8, True, PLUM, "smile", "dot")
         + limb("M520 740 L600 590 L630 420", PLUM, 64) + hand(632, 395, 38)
         + bubble(790, 250, 105, 105, (700, 360)) + txt(790, 298, 145, "?", fill=TERRA, sw=12))
    save(C, "May I_Asking permissions", "rose", b)

    # Pay: arm with banknote handing to a cash register
    reg = (f'<path d="M600 560 L880 560 L900 820 L580 820 Z" fill="{TEAL}" {ST} stroke-width="15"/>'
           f'<path d="M820 560 L880 560 L900 820 L830 820 Z" fill="{D}" opacity="0.14"/>'
           f'<rect x="560" y="740" width="360" height="90" rx="10" fill="{WOOD}" {ST} stroke-width="14"/>'
           f'<circle cx="740" cy="785" r="14" fill="{CREAM}" {ST} stroke-width="7"/>'
           f'<rect x="640" y="420" width="200" height="110" rx="12" fill="{DBROWN}" {ST} stroke-width="13"/>'
           f'<rect x="665" y="445" width="150" height="60" rx="6" fill="#a9c47f"/>'
           f'<path d="M720 530 L720 560 M760 530 L760 560" {ST} stroke-width="12"/>'
           + "".join(f'<rect x="{x}" y="{y}" width="46" height="30" rx="6" fill="{CREAM}" {ST} stroke-width="7"/>'
                     for x in (630, 700, 770) for y in (595, 650)))
    b = (shadow(740, 840, 200) + reg
         + limb("M110 920 L290 650", BLUE, 110, "butt")
         + banknote(430, 520, 0.8, -14)
         + hand(320, 610, 58)
         + f'<path d="M290 575 Q330 560 360 580" fill="none" {ST} stroke-width="9"/>'
         + arrow("M470 690 L560 690", MUST, 22, "545,660 545,720 595,690")
         + coin(560, 300, 55))
    save(C, "Pay", "beige", b)

    # Price: shirt with price tag
    b = (shadow(420, 850, 220) + shirt(TEAL, 1.05, 420, 580)
         + f'<path d="M500 420 Q560 380 590 430" fill="none" {ST} stroke-width="9"/>'
         + tag(710, 520, 0.9, 30, MUST, "฿", 130, string=False))
    save(C, "Price", "sand", b)

    # Size: shirt with S M L tags
    b = shadow(512, 640, 200) + shirt(BLUE, 0.9, 512, 430)
    for x, l, fs in ((300, "S", 70), (512, "M", 90), (724, "L", 110)):
        b += (f'<rect x="{x - 80}" y="700" width="160" height="150" rx="22" fill="{CREAM if l != "M" else MUST}" {ST} stroke-width="13"/>'
              + txt(x, 775 + fs * 0.36, fs, l, fill=D, sw=0))
    save(C, "Size", "sky", b)

    # Too much: overflowing coins over a dashed limit
    b = (shadow(512, 860, 280)
         + f'<path d="M200 520 L824 520" stroke="{D}" stroke-width="12" stroke-dasharray="36 26" stroke-linecap="round" opacity="0.6"/>'
         + coin_stack(360, 850, 8, rx=110, ry=34, h=32, op=0.35)
         + coin_stack(640, 850, 16, rx=120, ry=36, h=32)
         + f'<path d="M520 560 L520 334 A120 36 0 0 0 760 334 L760 560 Z" fill="{RED}" opacity="0.35"/>'
         + txt(850, 400, 170, "!", fill=RED, sw=14) + txt(190, 400, 0, "")
         )
    save(C, "Too much", "rose", b)

    # Try on: person holding shirt in front, mirror frame
    b = (f'<ellipse cx="512" cy="470" rx="330" ry="390" fill="#cfe0e6" {ST} stroke-width="22" />'
         f'<ellipse cx="512" cy="470" rx="330" ry="390" fill="none" stroke="{WOOD}" stroke-width="10"/>'
         f'<path d="M300 250 L380 170 M290 350 L420 220" stroke="#fff" stroke-width="18" opacity="0.5" stroke-linecap="round"/>'
         + person(512, 400, 0.75, True, CREAM, "smile", "dot")
         + shirt(RED, 0.8, 512, 700)
         + hand(370, 605, 34) + hand(654, 605, 34))
    save(C, "Try_Try on", "sand", b)

    # Wear: shirt coming down onto person
    b = (person(512, 560, 0.72, False, CREAM, "smile", "happy")
         + shirt(TERRA, 0.55, 512, 200)
         + arrow("M300 170 L300 290", OLIVE, 22, "268,275 332,275 300,320")
         + arrow("M724 170 L724 290", OLIVE, 22, "692,275 756,275 724,320"))
    save(C, "Wear", "beige", b)


# ======================================================================== MIX
def mix():
    C = "Mix"
    # Be at_Located: map with pin and person
    b = (shadow(512, 830, 330)
         + f'<path d="M180 400 L390 330 L620 400 L840 330 L840 790 L620 860 L390 790 L180 860 Z" fill="{CREAM}" {ST} stroke-width="15"/>'
         + f'<path d="M390 330 L390 790 L180 860 L180 400 Z" fill="{D}" opacity="0.08"/><path d="M620 400 L840 330 L840 790 L620 860 Z" fill="{D}" opacity="0.08"/>'
         + f'<path d="M390 330 L390 790 M620 400 L620 860" {ST} stroke-width="10"/>'
         + f'<path d="M200 700 Q350 600 460 660 Q560 720 820 560" fill="none" stroke="{BLUE}" stroke-width="22" opacity="0.7"/>'
         + f'<path d="M230 500 L360 460 M660 480 L800 440" stroke="{OLIVE}" stroke-width="18" opacity="0.7" stroke-linecap="round"/>'
         + f'<ellipse cx="512" cy="640" rx="60" ry="18" fill="{D}" opacity="0.25"/>'
         + f'<path d="M512 640 Q400 480 400 380 A112 112 0 0 1 624 380 Q624 480 512 640 Z" fill="{RED}" {ST} stroke-width="15"/>'
         + f'<circle cx="512" cy="380" r="46" fill="{CREAM}" {ST} stroke-width="11"/>'
         + kid(730, 700, 0.52, TEAL, walking=False)
         )
    save(C, "Be at_Located", "sage", b)

    # Buy: person with bags, handing coin
    b = (person(430, 400, 0.78, True, TEAL, "smile", "happy")
         + bag(250, 780, 0.55, TERRA)
         + limb("M270 620 L240 700", TEAL, 56)
         + limb("M580 610 L680 520 L720 430", TEAL, 58) + coin(740, 360, 70)
         + hand(712, 440, 32)
         + arrow("M820 300 L870 240", MUST, 16, "850,225 890,225 880,265"))
    b = (bag(280, 760, 0.6, TERRA) + bag(420, 800, 0.5, MUST)
         + person(512, 400, 0.78, True, TEAL, "smile", "happy")
         + limb("M680 620 L760 520 L780 440", TEAL, 58)
         + coin(790, 330, 72) + hand(780, 425, 34)
         + limb("M350 620 L330 690", TEAL, 58)
         + bag(300, 820, 0.62, TERRA) + hand(330, 610, 0))
    b = (person(512, 380, 0.76, True, TEAL, "smile", "happy")
         + limb("M360 610 L300 700 L300 720", TEAL, 58)
         + bag(300, 810, 0.55, TERRA) + hand(300, 690, 32)
         + limb("M664 610 L740 520 L770 440", TEAL, 58)
         + coin(790, 330, 72) + hand(772, 430, 34))
    save(C, "Buy", "beige", b)

    # Drink: person drinking via straw
    glass = (f'<path d="M-70 -110 L70 -110 L55 120 L-55 120 Z" fill="#cfe0e6" {ST} stroke-width="12"/>'
             f'<path d="M-62 -30 L62 -30 L55 120 L-55 120 Z" fill="{TERRA}" {ST} stroke-width="0"/>'
             f'<path d="M-70 -110 L70 -110 L55 120 L-55 120 Z" fill="none" {ST} stroke-width="12"/>'
             f'<path d="M-40 -80 L-32 90" stroke="#fff" stroke-width="12" opacity="0.5" stroke-linecap="round"/>')
    b = (person(470, 400, 0.85, False, MUST, "o", "happy")
         + f'<path d="M475 485 L545 425 L565 600" fill="none" stroke="{D}" stroke-width="26" stroke-linejoin="round" stroke-linecap="round"/>'
         + f'<path d="M475 485 L545 425 L565 600" fill="none" stroke="{RED}" stroke-width="12" stroke-linejoin="round" stroke-linecap="round"/>'
         + G(glass, 565, 690, 1.1)
         + limb("M600 800 L700 770 L660 710", MUST, 76) + hand(650, 705, 42))
    save(C, "Drink", "sky", b)

    # Eat: person with spoon, bowl
    b = (person(512, 360, 0.8, True, OLIVE, "open", "happy")
         + f'<path d="M270 700 Q512 740 754 700 Q740 880 512 890 Q284 880 270 700 Z" fill="{BLUE}" {ST} stroke-width="15"/>'
         + f'<ellipse cx="512" cy="700" rx="242" ry="40" fill="{CREAM}" {ST} stroke-width="13"/>'
         + f'<path d="M300 780 Q512 830 724 780" fill="none" stroke="{CREAM}" stroke-width="14" opacity="0.6"/>'
         + f'<path d="M640 690 L560 470" stroke="{D}" stroke-width="30" stroke-linecap="round"/>'
         + f'<path d="M640 690 L560 470" stroke="#d8d2c8" stroke-width="14" stroke-linecap="round"/>'
         + f'<ellipse cx="550" cy="445" rx="34" ry="46" fill="#d8d2c8" {ST} stroke-width="11" transform="rotate(-20 550 445)"/>'
         + f'<ellipse cx="548" cy="440" rx="20" ry="26" fill="{CREAM}" transform="rotate(-20 548 440)"/>'
         + hand(630, 640, 36)
         + ''.join(f'<path d="M{x} 640 Q{x - 20} 610 {x} 580" fill="none" {ST} stroke-width="8" opacity="0.5"/>' for x in (380, 440)))
    save(C, "Eat", "sand", b)

    # Go: walking kid + arrow
    b = (f'<path d="M120 860 L904 860" stroke="{D}" stroke-width="12" opacity="0.25" stroke-linecap="round"/>'
         + shadow(420, 860, 170, 26) + kid(420, 850, 0.95, TERRA)
         + arrow("M620 500 L820 500", OLIVE, 44, "790,430 790,570 890,500"))
    save(C, "Go", "sage", b)

    # Half: orange half + dashed missing half
    b = (shadow(512, 820, 280)
         + f'<path d="M512 250 A250 250 0 0 0 512 750" fill="none" stroke="{D}" stroke-width="12" stroke-dasharray="30 26" opacity="0.4"/>'
         + f'<path d="M512 250 A250 250 0 0 1 512 750 Z" fill="{TERRA}" {ST} stroke-width="15"/>'
         + f'<path d="M530 290 A210 210 0 0 1 530 710 Z" fill="#f2b27a" {ST} stroke-width="8"/>'
         + ''.join(f'<path d="M530 500 L{530 + 210 * c:.0f} {500 + 210 * s_:.0f}" {ST} stroke-width="8"/>'
                   for c, s_ in ((0.5, -0.866), (0.866, -0.5), (1, 0), (0.866, 0.5), (0.5, 0.866)))
         + f'<circle cx="530" cy="500" r="16" fill="{CREAM}" {ST} stroke-width="7"/>')
    save(C, "Half", "sand", b)

    # Have_There is: open box with fruits + check
    b = (shadow(500, 850, 300)
         + f'<circle cx="400" cy="540" r="80" fill="{TERRA}" {ST} stroke-width="13"/>'
         + f'<circle cx="540" cy="510" r="90" fill="{OLIVE}" {ST} stroke-width="13"/>'
         + f'<circle cx="660" cy="560" r="70" fill="{MUST}" {ST} stroke-width="13"/>'
         + f'<path d="M540 420 Q550 390 580 385" fill="none" {ST} stroke-width="10"/>'
         + f'<path d="M230 590 L770 590 L770 850 L230 850 Z" fill="{WOOD}" {ST} stroke-width="15"/>'
         + f'<path d="M230 590 L150 690 M770 590 L850 690" {ST} stroke-width="0"/>'
         + f'<path d="M230 590 L150 670 L270 700 Z M770 590 L850 670 L730 700 Z" fill="#b8865c" {ST} stroke-width="13"/>'
         + f'<path d="M660 590 L770 590 L770 850 L660 850 Z" fill="{D}" opacity="0.14"/>'
         + f'<path d="M260 620 L260 820" stroke="#fff" stroke-width="16" opacity="0.3" stroke-linecap="round"/>'
         + badge(760, 280, 85, True))
    save(C, "Have_There is", "beige", b)

    # No: red circle + stop hand
    b = (f'<circle cx="512" cy="500" r="320" fill="{CREAM}" {ST} stroke-width="15"/>'
         + palm(512, 500, 1.05, sleeve=None)
         + f'<circle cx="512" cy="500" r="300" fill="none" stroke="{RED}" stroke-width="44"/>'
         + f'<path d="M300 288 L724 712" stroke="{RED}" stroke-width="44" stroke-linecap="round" opacity="0"/>'
         + f'<circle cx="512" cy="500" r="322" fill="none" {ST} stroke-width="12"/><circle cx="512" cy="500" r="278" fill="none" {ST} stroke-width="10"/>')
    save(C, "No_Not_Don_t", "rose", b)

    # Put_Add: spoon adding to bowl + plus
    b = (shadow(470, 860, 260)
         + f'<path d="M230 620 Q470 660 710 620 Q700 850 470 860 Q240 850 230 620 Z" fill="{TEAL}" {ST} stroke-width="15"/>'
         + f'<ellipse cx="470" cy="620" rx="240" ry="44" fill="{TERRA}" {ST} stroke-width="13"/>'
         + f'<path d="M270 700 Q470 750 670 700" fill="none" stroke="#fff" stroke-width="14" opacity="0.3"/>'
         + ''.join(f'<circle cx="{x}" cy="{y}" r="10" fill="{CREAM}" {ST} stroke-width="5"/>' for x, y in ((520, 470), (545, 520), (505, 560), (560, 580)))
         + f'<path d="M830 200 L590 390" stroke="{D}" stroke-width="32" stroke-linecap="round"/>'
         + f'<path d="M830 200 L590 390" stroke="#d8d2c8" stroke-width="16" stroke-linecap="round"/>'
         + f'<ellipse cx="545" cy="420" rx="70" ry="44" fill="#d8d2c8" {ST} stroke-width="12" transform="rotate(-38 545 420)"/>'
         + f'<ellipse cx="545" cy="415" rx="44" ry="24" fill="{CREAM}" transform="rotate(-38 545 415)"/>'
         + f'<path d="M250 300 L250 460 M170 380 L330 380" stroke="{D}" stroke-width="64" stroke-linecap="round"/>'
         + f'<path d="M250 300 L250 460 M170 380 L330 380" stroke="{OLIVE}" stroke-width="38" stroke-linecap="round"/>')
    save(C, "Put_Add", "sage", b)

    # Question particle: big ? in bubble
    b = bubble(512, 460, 300, 260, (330, 860)) + txt(512, 600, 400, "?", fill=TERRA, sw=24)
    save(C, "Question particle", "sky", b)

    # Very: thermometer maxed + !!
    b = (f'<rect x="440" y="150" width="144" height="560" rx="72" fill="{CREAM}" {ST} stroke-width="15"/>'
         f'<circle cx="512" cy="760" r="120" fill="{RED}" {ST} stroke-width="15"/>'
         f'<rect x="478" y="175" width="68" height="560" rx="34" fill="{RED}"/>'
         f'<circle cx="512" cy="760" r="106" fill="{RED}"/>'
         f'<path d="M480 190 L480 640" stroke="#fff" stroke-width="16" opacity="0.35" stroke-linecap="round"/>'
         f'<circle cx="475" cy="730" r="26" fill="#fff" opacity="0.35"/>'
         + ''.join(f'<path d="M584 {y} L624 {y}" {ST} stroke-width="10"/>' for y in (260, 350, 440, 530, 620))
         + f'<path d="M380 180 Q350 150 380 120 M660 180 Q690 150 660 120" fill="none" {ST} stroke-width="0"/>'
         + ''.join(f'<path d="M{x} 380 Q{x - 25} 330 {x} 280 Q{x + 25} 230 {x} 180" fill="none" stroke="{TERRA}" stroke-width="14" stroke-linecap="round"/>' for x in (330, 380))
         + txt(760, 480, 280, "!!", fill=RED, sw=20))
    save(C, "Very", "sand", b)

    # Want: person looking up at thought bubble with ice cream
    ice = (f'<path d="M-70 -10 L70 -10 L0 170 Z" fill="{MUST}" {ST} stroke-width="12"/>'
           f'<path d="M-45 30 L25 110 M45 30 L-25 110" {ST} stroke-width="7" opacity="0.5"/>'
           f'<circle cx="-35" cy="-40" r="55" fill="#e8907f" {ST} stroke-width="12"/>'
           f'<circle cx="40" cy="-40" r="55" fill="{CREAM}" {ST} stroke-width="12"/>'
           f'<circle cx="0" cy="-100" r="55" fill="{OLIVE}" {ST} stroke-width="12"/>')
    b = (person(360, 560, 0.72, False, BLUE, "smile", "up")
         + think(700, 290, 170, 150, (520, 470)) + G(ice, 700, 290, 0.95)
         + f'<path d="M340 760 Q360 740 380 760" fill="none" {ST} stroke-width="0"/>')
    save(C, "Want", "rose", b)


if __name__ == "__main__":
    shopping()
    mix()

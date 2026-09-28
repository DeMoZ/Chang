"""Generator for Prepositions + PointThings SVGs."""
import math, os

ROOT = os.path.dirname(os.path.abspath(__file__))
D = "#2e211b"
BG = {
    "beige": ("#f3e6d6", "#c9ab93"), "rose": ("#f1e0dc", "#bf9a98"), "sage": ("#e7ecdc", "#a9b595"),
    "sky": ("#e3ecf1", "#9fb3c1"), "sand": ("#f5ead0", "#cfb27e"),
}


def svg(bg, body, extra_defs=""):
    a, b = BG[bg]
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">\n'
            f'<defs><radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{a}"/>'
            f'<stop offset="100%" stop-color="{b}"/></radialGradient>{extra_defs}</defs>\n'
            f'<rect width="1024" height="1024" fill="url(#bg)"/>\n{body}\n</svg>\n')


def S(w=14):
    return f'stroke="{D}" stroke-width="{w}" stroke-linejoin="round" stroke-linecap="round"'


def shadow(cx, cy, rx, ry=None, op=0.15):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry or rx * 0.14:.0f}" fill="{D}" opacity="{op}"/>\n'


def poly(pts, fill, w=14, extra=""):
    p = " ".join(f"{x:.1f},{y:.1f}" for x, y in pts)
    return f'<polygon points="{p}" fill="{fill}" {S(w)} {extra}/>\n'


def box(x, y, w, h, d=80, front="#b07a50", top="#c9935f", side="#7d5337"):
    s = poly([(x, y), (x + d, y - d), (x + w + d, y - d), (x + w, y)], top)
    s += poly([(x + w, y), (x + w + d, y - d), (x + w + d, y + h - d), (x + w, y + h)], side)
    s += f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{front}" {S()}/>\n'
    s += f'<path d="M{x} {y + h * 0.5} L{x + w} {y + h * 0.5}" stroke="{D}" stroke-width="8" opacity="0.35"/>\n'
    s += f'<path d="M{x + 36} {y + 40} L{x + 36} {y + h - 40}" stroke="#fff" stroke-width="16" opacity="0.25" stroke-linecap="round"/>\n'
    return s


def ball(cx, cy, r, fill="#c8574b", op=1):
    g = f'<g opacity="{op}">' if op != 1 else "<g>"
    return (g + f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {S(14 if r > 50 else 10)}/>'
            f'<path d="M{cx - r * 0.55} {cy - r * 0.45} Q{cx - r * 0.25} {cy - r * 0.75} {cx + r * 0.1} {cy - r * 0.7}" '
            f'fill="none" stroke="#fff" stroke-width="{max(6, r * 0.16):.0f}" opacity="0.4" stroke-linecap="round"/></g>\n')


def arrow(x1, y1, x2, y2, w=16, head=55):
    a = math.atan2(y2 - y1, x2 - x1)
    p1 = (x2 - head * math.cos(a - 0.6), y2 - head * math.sin(a - 0.6))
    p2 = (x2 - head * math.cos(a + 0.6), y2 - head * math.sin(a + 0.6))
    return (f'<path d="M{x1} {y1} L{x2} {y2}" stroke="{D}" stroke-width="{w}" stroke-linecap="round"/>'
            f'<path d="M{p1[0]:.1f} {p1[1]:.1f} L{x2} {y2} L{p2[0]:.1f} {p2[1]:.1f}" fill="none" {S(w)}/>\n')


def bigarrow(d, end, ang, color="#e0b04f", w=46, head=70):
    a = math.radians(ang)
    ex, ey = end
    ux, uy = math.cos(a), math.sin(a)
    px, py = -uy, ux
    hw = w * 1.2
    pts = [(ex + px * hw, ey + py * hw), (ex + ux * head, ey + uy * head), (ex - px * hw, ey - py * hw)]
    s = f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{w + 24}" stroke-linecap="round" stroke-linejoin="round"/>\n'
    s += poly(pts, color, 12)
    s += f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"/>\n'
    s += f'<path d="M{ex - ux * 20:.1f} {ey - uy * 20:.1f} L{ex + ux * 8:.1f} {ey + uy * 8:.1f}" stroke="{color}" stroke-width="{w}"/>\n'
    s += f'<path d="{d}" fill="none" stroke="#fff" stroke-width="{w * 0.25:.0f}" opacity="0.3" stroke-linecap="round" stroke-linejoin="round" transform="translate({-px * w * 0.2:.1f},{-py * w * 0.2:.1f})"/>\n'
    return s


def road(d, w=200, dash=True, color="#8f8780"):
    s = f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{w + 28}" stroke-linejoin="round"/>\n'
    s += f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linejoin="round"/>\n'
    if dash:
        s += f'<path d="{d}" fill="none" stroke="#f6ecd8" stroke-width="12" stroke-dasharray="40 34"/>\n'
    return s


def road_multi(paths, w=200, dash=True):
    s = "".join(f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{w + 28}" stroke-linejoin="round"/>\n' for d in paths)
    s += "".join(f'<path d="{d}" fill="none" stroke="#8f8780" stroke-width="{w}" stroke-linejoin="round"/>\n' for d in paths)
    if dash:
        s += "".join(f'<path d="{d}" fill="none" stroke="#f6ecd8" stroke-width="12" stroke-dasharray="40 34"/>\n' for d in paths)
    return s


def car_top(cx, cy, ang=0, color="#5d82a8", s=1.0):
    # local: pointing up
    return (f'<g transform="translate({cx},{cy}) rotate({ang}) scale({s})">'
            f'<rect x="-52" y="-50" width="14" height="30" rx="5" fill="{D}"/><rect x="38" y="-50" width="14" height="30" rx="5" fill="{D}"/>'
            f'<rect x="-52" y="30" width="14" height="30" rx="5" fill="{D}"/><rect x="38" y="30" width="14" height="30" rx="5" fill="{D}"/>'
            f'<rect x="-46" y="-80" width="92" height="160" rx="30" fill="{color}" {S(12)}/>'
            f'<path d="M-34 -30 Q0 -44 34 -30 L30 -8 Q0 -16 -30 -8 Z" fill="#cfe0ea" {S(8)}/>'
            f'<path d="M-30 40 Q0 34 30 40 L32 58 Q0 64 -32 58 Z" fill="#cfe0ea" {S(8)}/>'
            f'<rect x="-30" y="-6" width="60" height="44" rx="10" fill="#fff" opacity="0.2"/>'
            f'<circle cx="-28" cy="-70" r="7" fill="#f6ecd8"/><circle cx="28" cy="-70" r="7" fill="#f6ecd8"/>'
            f'</g>\n')


def pin(x, y, s=1.0, color="#c8574b"):
    return (f'<g transform="translate({x},{y}) scale({s})">'
            f'<ellipse cx="0" cy="0" rx="34" ry="10" fill="{D}" opacity="0.2"/>'
            f'<path d="M0 0 C-10 -40 -62 -70 -62 -118 A62 62 0 1 1 62 -118 C62 -70 10 -40 0 0 Z" fill="{color}" {S(12)}/>'
            f'<circle cx="0" cy="-118" r="24" fill="#f6ecd8" {S(8)}/>'
            f'<path d="M-40 -140 Q-30 -165 -8 -170" fill="none" stroke="#fff" stroke-width="10" opacity="0.35" stroke-linecap="round"/>'
            f'</g>\n')


def tree_top(x, y, r=55, c="#5f8f4e"):
    return (f'<circle cx="{x + 8}" cy="{y + 10}" r="{r}" fill="{D}" opacity="0.15"/>'
            f'<circle cx="{x}" cy="{y}" r="{r}" fill="{c}" {S(10)}/>'
            f'<circle cx="{x - r * 0.3}" cy="{y - r * 0.3}" r="{r * 0.35}" fill="#fff" opacity="0.2"/>\n')


def tree_side(x, base, s=1.0, c="#5f8f4e"):
    return (f'<g transform="translate({x},{base}) scale({s})">'
            f'<rect x="-14" y="-90" width="28" height="90" fill="#9a6a45" {S(10)}/>'
            f'<circle cx="0" cy="-140" r="70" fill="{c}" {S(10)}/>'
            f'<circle cx="-25" cy="-165" r="22" fill="#fff" opacity="0.2"/></g>\n')


def house_top(cx, cy, w=170, h=150, roof="#d9825b", ang=0):
    ww = w * 1.0
    return shadow(cx, cy + h * 0.45, ww * 0.6, 12, 0.18) + house_front(cx, cy + h * 0.45, ww, roof=roof)


def _old_house_top(cx, cy, w=170, h=150, roof="#d9825b", ang=0):
    x, y = -w / 2, -h / 2
    return (f'<g transform="translate({cx},{cy}) rotate({ang})">'
            f'<rect x="{x + 10}" y="{y + 12}" width="{w}" height="{h}" fill="{D}" opacity="0.15"/>'
            f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{roof}" {S(12)}/>'
            f'<path d="M{x} {y} L{x + h / 2} 0 L{-x - h / 2} 0 L{-x} {y} M{x} {-y} L{x + h / 2} 0 M{-x - h / 2} 0 L{-x} {-y}" fill="none" {S(8)}/>'
            f'<polygon points="{x},{y} {x + h / 2},0 {-x - h / 2},0 {-x},{y}" fill="#fff" opacity="0.22"/>'
            f'<polygon points="{-x},{y} {-x - h / 2},0 {-x},{-y}" fill="{D}" opacity="0.12"/>'
            f'</g>\n')


def house_front(cx, base, w=240, wall="#f6ecd8", roof="#c8574b", door="#9a6a45", op=1):
    h = w * 0.8
    x0, x1 = cx - w / 2, cx + w / 2
    top = base - h
    s = f'<g opacity="{op}">' if op != 1 else "<g>"
    s += f'<rect x="{x0}" y="{top}" width="{w}" height="{h}" fill="{wall}" {S()}/>'
    s += f'<rect x="{x0}" y="{top}" width="{w * 0.18}" height="{h}" fill="{D}" opacity="0.08"/>'
    s += poly([(x0 - 30, top + 6), (cx, top - w * 0.5), (x1 + 30, top + 6)], roof)
    s += f'<rect x="{cx - w * 0.13}" y="{base - h * 0.5}" width="{w * 0.26}" height="{h * 0.5}" fill="{door}" {S(10)}/>'
    s += f'<rect x="{x0 + w * 0.08}" y="{top + h * 0.2}" width="{w * 0.2}" height="{w * 0.2}" fill="#9fc3d6" {S(8)}/>'
    s += f'<rect x="{x1 - w * 0.28}" y="{top + h * 0.2}" width="{w * 0.2}" height="{w * 0.2}" fill="#9fc3d6" {S(8)}/>'
    s += "</g>\n"
    return s


def face(cx, cy, r, mouth="smile"):
    s = f'<circle cx="{cx - r * 0.35}" cy="{cy}" r="{r * 0.09}" fill="{D}"/><circle cx="{cx + r * 0.35}" cy="{cy}" r="{r * 0.09}" fill="{D}"/>'
    s += f'<circle cx="{cx - r * 0.55}" cy="{cy + r * 0.3}" r="{r * 0.16}" fill="#e8907f" opacity="0.5"/><circle cx="{cx + r * 0.55}" cy="{cy + r * 0.3}" r="{r * 0.16}" fill="#e8907f" opacity="0.5"/>'
    if mouth == "smile":
        s += f'<path d="M{cx - r * 0.25} {cy + r * 0.4} Q{cx} {cy + r * 0.62} {cx + r * 0.25} {cy + r * 0.4}" fill="none" stroke="{D}" stroke-width="{r * 0.08:.0f}" stroke-linecap="round"/>'
    elif mouth == "o":
        s += f'<ellipse cx="{cx}" cy="{cy + r * 0.48}" rx="{r * 0.13}" ry="{r * 0.17}" fill="{D}"/>'
    return s


def person(cx, base, s=1.0, shirt="#5d82a8", pants="#5c3d2e", skin="#f1c9a5", hair="long", arms="down",
           mouth="smile", brows=False):
    """Front-view person, feet at (cx, base). Local height ~480."""
    o = f'<g transform="translate({cx},{base}) scale({s})">'
    o += shadow(0, 0, 110, 18)
    o += f'<path d="M-40 -150 L-44 -8 M40 -150 L44 -8" stroke="{D}" stroke-width="58" stroke-linecap="round"/>'
    o += f'<path d="M-40 -150 L-44 -8 M40 -150 L44 -8" stroke="{pants}" stroke-width="34" stroke-linecap="round"/>'
    o += f'<ellipse cx="-50" cy="-4" rx="34" ry="16" fill="{D}"/><ellipse cx="50" cy="-4" rx="34" ry="16" fill="{D}"/>'
    if arms == "down":
        armd = "M-78 -270 Q-110 -200 -100 -140 M78 -270 Q110 -200 100 -140"
        hands = [(-100, -130), (100, -130)]
    elif arms == "up_right":
        armd = "M-78 -270 Q-110 -200 -100 -140 M78 -270 Q140 -300 170 -370"
        hands = [(-100, -130), (172, -380)]
    else:  # custom list of arm paths handled by caller
        armd, hands = arms
    o += f'<path d="{armd}" fill="none" stroke="{D}" stroke-width="50" stroke-linecap="round"/>'
    o += f'<path d="{armd}" fill="none" stroke="{shirt}" stroke-width="28" stroke-linecap="round"/>'
    for hx, hy in hands:
        o += f'<circle cx="{hx}" cy="{hy}" r="22" fill="{skin}" {S(10)}/>'
    o += f'<path d="M-85 -130 L-80 -250 Q-78 -290 -30 -295 L30 -295 Q78 -290 80 -250 L85 -130 Z" fill="{shirt}" {S(12)}/>'
    o += f'<path d="M-62 -140 L-60 -250" stroke="#fff" stroke-width="12" opacity="0.3" stroke-linecap="round"/>'
    o += f'<rect x="-20" y="-320" width="40" height="34" fill="{skin}" {S(10)}/>'
    if hair == "long":
        o += f'<path d="M-82 -380 Q-86 -470 0 -472 Q86 -470 82 -380 Q88 -300 60 -290 L-60 -290 Q-88 -300 -82 -380 Z" fill="#3b2a22" {S(12)}/>'
    o += f'<circle cx="0" cy="-390" r="72" fill="{skin}" {S(12)}/>'
    o += f'<path d="M-72 -400 Q-62 -466 0 -466 Q62 -466 72 -400 Q30 -430 0 -428 Q-36 -428 -72 -400 Z" fill="#3b2a22" {S(8)}/>'
    o += face(0, -385, 72, mouth)
    if brows:
        o += f'<path d="M-40 -418 Q-26 -430 -12 -420 M12 -420 Q26 -430 40 -418" fill="none" stroke="{D}" stroke-width="6" stroke-linecap="round"/>'
    o += "</g>\n"
    return o


def hand(tip_x, tip_y, ang, s=1.0, sleeve="#5d82a8", skin="#f1c9a5"):
    a = math.radians(ang)
    lx, ly = 170 * s, -46 * s
    X = tip_x - (lx * math.cos(a) - ly * math.sin(a))
    Y = tip_y - (lx * math.sin(a) + ly * math.cos(a))
    return (f'<g transform="translate({X:.1f},{Y:.1f}) rotate({ang}) scale({s})">'
            f'<rect x="-420" y="-78" width="320" height="156" rx="20" fill="{sleeve}" {S(12)}/>'
            f'<rect x="-400" y="-56" width="280" height="22" rx="10" fill="#fff" opacity="0.25"/>'
            f'<rect x="-140" y="-84" width="44" height="168" rx="16" fill="#f6ecd8" {S(12)}/>'
            f'<rect x="-110" y="-72" width="170" height="142" rx="56" fill="{skin}" {S(12)}/>'
            f'<rect x="20" y="-72" width="160" height="50" rx="25" fill="{skin}" {S(12)}/>'
            f'<path d="M150 -60 Q162 -58 166 -48" fill="none" stroke="#e8907f" stroke-width="6" opacity="0.7" stroke-linecap="round"/>'
            f'<rect x="20" y="-24" width="62" height="34" rx="17" fill="{skin}" {S(10)}/>'
            f'<rect x="16" y="10" width="60" height="32" rx="16" fill="{skin}" {S(10)}/>'
            f'<rect x="10" y="40" width="54" height="30" rx="15" fill="{skin}" {S(10)}/>'
            f'<path d="M-70 -30 Q-10 -40 30 -10" fill="none" stroke="{D}" stroke-width="44" stroke-linecap="round"/>'
            f'<path d="M-70 -30 Q-10 -40 30 -10" fill="none" stroke="{skin}" stroke-width="22" stroke-linecap="round"/>'
            f'<path d="M-80 30 Q-60 50 -20 55" fill="none" stroke="{D}" stroke-width="6" opacity="0.3" stroke-linecap="round"/>'
            f'</g>\n')


def sparkle(x, y, r=22, c="#e0b04f"):
    return (f'<path d="M{x} {y - r} Q{x + r * 0.15} {y - r * 0.15} {x + r} {y} Q{x + r * 0.15} {y + r * 0.15} {x} {y + r} '
            f'Q{x - r * 0.15} {y + r * 0.15} {x - r} {y} Q{x - r * 0.15} {y - r * 0.15} {x} {y - r} Z" fill="{c}" {S(6)}/>\n')


def ring(cx, cy, r, c="#e0b04f", w=10):
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="{c}" stroke-width="{w}" stroke-dasharray="{w * 2} {w * 1.6}" stroke-linecap="round"/>\n'


OUT = {}


def put(cat, key, bg, body, defs=""):
    OUT[(cat, key)] = svg(bg, body, defs)


P = "Prepositions"
# ---------------- spatial set (sage, ball + box) -----------------
put(P, "In", "sage", open(os.path.join(ROOT, "svg/_Sample/In.svg")).read().split('url(#bg)"/>', 1)[1].rsplit("</svg>", 1)[0])

b = shadow(512, 810, 320) + box(262, 480, 440, 320) + shadow(532, 440, 80, 14, 0.25) + ball(532, 348, 100) + arrow(532, 110, 532, 222)
put(P, "On", "sage", b)

# Under: table with ball beneath
b = shadow(512, 812, 360)
b += ball(540, 700, 105)
b += f'<rect x="200" y="420" width="40" height="390" rx="8" fill="#7d5337" {S()}/><rect x="784" y="420" width="40" height="390" rx="8" fill="#7d5337" {S()}/>'
b += poly([(170, 400), (230, 340), (914, 340), (854, 400)], "#c9935f")
b += f'<rect x="170" y="400" width="684" height="46" fill="#b07a50" {S()}/>'
b += f'<path d="M210 372 L860 372" stroke="#fff" stroke-width="10" opacity="0.3" stroke-linecap="round"/>'
b += arrow(310, 520, 400, 610)
put(P, "Under", "sage", b)

# Behind: ball peeking from behind the box
b = shadow(720, 722, 150, 22) + ball(710, 572, 150) + shadow(460, 812, 300) + box(200, 510, 420, 300)
b += arrow(880, 240, 780, 350)
put(P, "Behind_After", "sage", b)

# Beside: ball touching the side
b = shadow(430, 812, 290) + box(160, 470, 400, 340) + shadow(760, 812, 110, 18) + ball(752, 700, 110) + arrow(752, 400, 752, 532)
put(P, "Beside", "sage", b)

# Near: ball close (small gap) + far ball faded for contrast
b = shadow(340, 812, 230) + box(140, 520, 330, 290, 70)
b += shadow(642, 812, 90, 16) + ball(642, 722, 90)
b += ball(860, 610, 38, op=0.35)
b += arrow(642, 440, 642, 570)
put(P, "Near", "sage", b)

# Between: ball between two boxes
b = shadow(270, 812, 170) + box(120, 540, 230, 270, 60) + shadow(760, 812, 170) + box(640, 540, 230, 270, 60)
b += shadow(522, 812, 100, 16) + ball(522, 710, 100) + arrow(522, 410, 522, 540)
put(P, "Between", "sage", b)

# In front of
b = shadow(512, 680, 320) + box(232, 340, 480, 340)
b += shadow(470, 862, 130, 20) + ball(470, 742, 120) + arrow(880, 640, 650, 730)
put(P, "In front of", "sage", b)

# Next to: two houses side by side
b = shadow(512, 780, 380)
b += house_front(360, 780, 280, wall="#f6ecd8", roof="#9a6a45", door="#5c3d2e")
b += house_front(660, 780, 280, wall="#f6ecd8", roof="#c8574b")
b += arrow(660, 150, 660, 300)
put(P, "Next to", "sage", b)

# Opposite: two houses across a road (top-down)
b = road("M110 512 L914 512", 200)
b += house_top(512, 270, 220, 170, "#9a6a45") + house_top(512, 754, 220, 170, "#c8574b")
b += f'<path d="M512 370 L512 654" stroke="#f6ecd8" stroke-width="14" stroke-dasharray="4 28" stroke-linecap="round"/>'
b += tree_top(240, 260, 55) + tree_top(790, 760, 55) + tree_top(790, 270, 45) + tree_top(240, 760, 45)
b += arrow(880, 880, 650, 810)
put(P, "Opposite", "sage", b)

# Area near / row: neighbourhood with dashed area around a pin
b = road_multi(["M110 400 L914 400", "M110 700 L914 700"], 90, dash=False)
for i, x in enumerate([200, 360, 520, 680, 840]):
    b += house_top(x, 250, 112, 110, ["#d9825b", "#9a6a45", "#c8574b", "#5d82a8", "#d9825b"][i])
    b += house_top(x, 550, 112, 110, ["#5d82a8", "#e0b04f", "#d9825b", "#9a6a45", "#c8574b"][i])
    b += house_top(x, 850 - 0, 120, 90, ["#9a6a45", "#c8574b", "#5d82a8", "#e0b04f", "#9a6a45"][i]) if 0 else ""
b += f'<circle cx="520" cy="520" r="250" fill="#e0b04f" opacity="0.22"/>'
b += f'<circle cx="520" cy="520" r="250" fill="none" stroke="{D}" stroke-width="12" stroke-dasharray="30 26" stroke-linecap="round"/>'
b += pin(520, 560, 1.0)
put(P, "Area_near_row", "sage", b)

# At: pin exactly on the target spot
b = f'<ellipse cx="512" cy="700" rx="330" ry="130" fill="#f6ecd8" {S()}/>'
b += f'<ellipse cx="512" cy="700" rx="230" ry="90" fill="#c8574b" {S(10)}/>'
b += f'<ellipse cx="512" cy="700" rx="140" ry="55" fill="#f6ecd8" {S(10)}/>'
b += f'<ellipse cx="512" cy="700" rx="55" ry="22" fill="#c8574b" {S(10)}/>'
b += pin(512, 700, 2.2, "#5d82a8")
put(P, "At_at exact area", "sage", b)

# ---------------- road scenes -----------------
cross = road_multi(["M512 110 L512 914", "M110 440 L914 440"], 200)
cross += "".join(tree_top(x, y, 60) for x, y in [(250, 220), (780, 220), (250, 700), (790, 700)])
b = cross + car_top(462, 780, 0, "#5d82a8") + bigarrow("M462 680 L462 570 Q462 490 540 490 L770 490", (770, 490), 0)
put(P, "Turn", "sage", b)

b = cross + car_top(462, 790, 0, "#5d82a8") + bigarrow("M462 690 L462 250", (462, 250), -90) + pin(462, 190, 0.8)
put(P, "Go straight", "sage", b)


def side_scene(left):
    x_road = 560 if left else 464
    b = road(f"M{x_road} 110 L{x_road} 914", 220)
    hx = 250 if left else 774
    b += f'<rect x="{hx - 140}" y="200" width="280" height="620" rx="40" fill="#e0b04f" opacity="0.3"/>'
    b += house_top(hx, 460, 170, 150, "#c8574b") + pin(hx, 360, 0.9)
    ox = 830 if left else 194
    b += house_top(ox, 330, 120, 110, "#9a6a45") + tree_top(ox, 640, 50)
    b = f'<g>{b}</g>'
    b += car_top(x_road - 50, 790, 0, "#5d82a8")
    ang = 180 if left else 0
    ex = 380 if left else 644
    b += bigarrow(f"M{x_road - 50} 700 L{x_road - 50} 620 Q{x_road - 50} 560 {x_road - 110 if left else x_road + 10} 560 L{ex} 560", (ex, 560), ang)
    return b


put(P, "Left side", "sage", side_scene(True))
put(P, "Right side", "sage", side_scene(False))

b = road_multi(["M512 110 L512 914"], 320, dash=False)
b += f'<path d="M512 110 L512 914" stroke="#f6ecd8" stroke-width="12" stroke-dasharray="40 34"/>'
b += tree_top(220, 300, 60) + tree_top(810, 700, 60) + tree_top(220, 760, 50)
b += car_top(430, 800, 0, "#d9825b")
b += bigarrow("M430 710 L430 380 A82 82 0 0 1 594 380 L594 650", (594, 650), 90)
put(P, "Make a U_turn", "sage", b)

b = road("M512 110 L512 914", 300)
b += f'<path d="M362 120 L362 904" stroke="#e0b04f" stroke-width="16" stroke-linecap="round"/>'
b += tree_top(220, 300, 60) + tree_top(810, 700, 60) + tree_top(820, 260, 45)
b += car_top(420, 790, 0, "#5d82a8") + bigarrow("M420 700 L420 250", (420, 250), -90)
for y in (330, 480, 630):
    b += arrow(610, y, 510, y, 14, 36)
put(P, "Keep at some side", "sage", b)

b = road("M560 110 L560 914", 220)
b += house_top(300, 480, 200, 170, "#c8574b") + tree_top(300, 720, 55) + tree_top(820, 300, 55)
b += f'<path d="M420 480 L530 480" stroke="{D}" stroke-width="10" stroke-dasharray="4 22" stroke-linecap="round"/>'
b += car_top(510, 800, 0, "#5d82a8") + bigarrow("M510 710 L510 270", (510, 270), -90)
b += pin(510, 210, 0.75)
put(P, "Pass", "sage", b)

b = road("M512 110 L512 914", 240)
b += f'<rect x="380" y="100" width="264" height="260" fill="#a9b595" opacity="0.55"/>'
for i in range(6):
    b += f'<rect x="{392 + i * 40}" y="{360 + (i % 2) * 20}" width="40" height="20" fill="{D}"/>'
    b += f'<rect x="{392 + i * 40}" y="{380 - (i % 2) * 20}" width="40" height="20" fill="#f6ecd8"/>'
b += f'<rect x="392" y="360" width="240" height="40" fill="none" {S(8)}/>'
b += f'<path d="M660 400 L660 220" {S(12)}/><path d="M660 220 L760 250 L660 285 Z" fill="#c8574b" {S(10)}/>'
b += tree_top(230, 300, 55) + tree_top(820, 680, 55) + tree_top(230, 760, 50)
b += car_top(462, 790, 0, "#5d82a8") + bigarrow("M462 700 L462 480", (462, 480), -90, head=62)
put(P, "Until", "sage", b)

b = road("M512 914 L512 330", 240)
b += f'<rect x="360" y="240" width="304" height="60" rx="12" fill="#f6ecd8" {S(12)}/>'
for i in range(5):
    b += f'<path d="M{380 + i * 60} 296 L{420 + i * 60} 244" stroke="#c8574b" stroke-width="22"/>'
b += f'<rect x="360" y="240" width="304" height="60" rx="12" fill="none" {S(12)}/>'
b += f'<path d="M380 300 L380 350 M644 300 L644 350" {S(12)}/>'
b += tree_top(260, 230, 60) + tree_top(770, 230, 60) + tree_top(240, 620, 55) + tree_top(790, 700, 55)
b += pin(512, 420, 1.1)
b += bigarrow("M512 850 L512 560", (512, 560), -90)
put(P, "End of", "sage", b)

b = road("M512 914 L512 400", 200, dash=False)
b += house_top(512, 330, 220, 180, "#c8574b")
b += car_top(512, 570, 0, "#5d82a8")
b += "".join(f'<path d="M{x} 690 L{x} {740 + (x % 3) * 20}" stroke="#f6ecd8" stroke-width="12" stroke-linecap="round"/>' for x in (478, 512, 546))
b += tree_top(250, 480, 60) + tree_top(780, 520, 60) + tree_top(260, 780, 50) + tree_top(770, 800, 50)
b += pin(512, 330, 0.85)
b += f'<circle cx="700" cy="330" r="44" fill="#7fa05a" {S(10)}/><path d="M680 330 L696 348 L722 314" fill="none" stroke="#fff" stroke-width="12" stroke-linecap="round" stroke-linejoin="round"/>'
put(P, "Arrive", "sage", b)

b = road("M320 600 L914 600", 190, dash=True)
b += house_top(250, 520, 220, 190, "#c8574b")
b += f'<g opacity="0.8">{pin(250, 450, 0.7, "#9a6a45")}</g>'
b += car_top(520, 640, 90, "#5d82a8")
b += "".join(f'<circle cx="{x}" cy="{y}" r="{r}" fill="#f6ecd8" {S(8)}/>' for x, y, r in [(410, 650, 18), (380, 620, 12)])
b += bigarrow("M620 640 L800 640", (800, 640), 0)
b += tree_top(600, 300, 60) + tree_top(820, 330, 50) + tree_top(420, 850, 50) + tree_top(760, 850, 50)
put(P, "Leave from", "sage", b)

# Road: perspective road to horizon
b = f'<rect x="0" y="470" width="1024" height="554" fill="#8faa6a"/>'
b += f'<path d="M0 470 L1024 470" {S(12)}/>'
b += f'<path d="M180 900 L492 470 L532 470 L844 900 Z" fill="#8f8780" {S(14)}/>'
b += "".join(f'<path d="M512 {y1} L512 {y2}" stroke="#f6ecd8" stroke-width="{w}" stroke-linecap="round"/>' for y1, y2, w in [(495, 520, 6), (560, 610, 10), (670, 760, 16), (820, 880, 22)])
b += tree_side(270, 560, 0.7) + tree_side(780, 560, 0.7) + tree_side(400, 490, 0.3) + tree_side(640, 490, 0.3) + tree_side(140, 720, 1.0) + tree_side(900, 720, 1.0)
b += f'<rect x="0" y="900" width="1024" height="124" fill="#8faa6a"/>'
b += f'<circle cx="760" cy="250" r="70" fill="#e0b04f" {S(12)}/>'
put(P, "Road", "sky", b)

# Alley: narrow lane between buildings
b = f'<path d="M150 120 L440 380 L440 520 L150 900 Z" fill="#d9825b" {S(14)}/>'
b += f'<path d="M874 120 L584 380 L584 520 L874 900 Z" fill="#e0b04f" {S(14)}/>'
b += f'<path d="M150 900 L440 520 L584 520 L874 900 Z" fill="#8f8780" {S(14)}/>'
b += f'<rect x="440" y="380" width="144" height="140" fill="#f6ecd8" {S(10)}/>'
b += f'<path d="M512 540 L512 560 M512 600 L512 640 M512 700 L512 770 M512 830 L512 880" stroke="#f6ecd8" stroke-width="12" stroke-linecap="round" opacity="0.8"/>'
for i, (x, y, w, h) in enumerate([(200, 270, 70, 110), (320, 370, 50, 80), (200, 520, 70, 110), (320, 540, 50, 70)]):
    b += f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="#9fc3d6" {S(8)}/>'
    b += f'<rect x="{1024 - x - w}" y="{y}" width="{w}" height="{h}" fill="#9fc3d6" {S(8)}/>'
b += f'<path d="M150 120 L440 380 L440 520 L150 900 Z" fill="{D}" opacity="0.1"/>'
b += f'<path d="M440 380 Q512 420 584 380" fill="none" stroke="{D}" stroke-width="6"/>'
b += "".join(f'<path d="M{454 + i * 26} 380 l0 {10 + (i % 2) * 8}" stroke="#c8574b" stroke-width="10"/>' for i in range(5))
put(P, "Alley", "sand", b)

# Way: winding path to a pin
b = f'<path d="M230 880 C230 700 760 740 740 560 C720 400 300 480 320 330 C335 230 560 230 640 240" fill="none" stroke="{D}" stroke-width="112" stroke-linecap="round"/>'
b += f'<path d="M230 880 C230 700 760 740 740 560 C720 400 300 480 320 330 C335 230 560 230 640 240" fill="none" stroke="#e8cf9a" stroke-width="84" stroke-linecap="round"/>'
b += f'<path d="M230 880 C230 700 760 740 740 560 C720 400 300 480 320 330 C335 230 560 230 640 240" fill="none" stroke="#9a6a45" stroke-width="12" stroke-dasharray="4 30" stroke-linecap="round"/>'
b += tree_top(200, 560, 55) + tree_top(560, 420, 45) + tree_top(840, 820, 55) + tree_top(820, 330, 45) + tree_top(460, 640, 40)
b += f'<circle cx="230" cy="870" r="26" fill="#f6ecd8" {S(10)}/>'
b += pin(660, 250, 1.0)
put(P, "Way", "sage", b)

# Walk: side-view person walking + footprints
def walker(x, base, s=1.0, shirt="#7fa05a"):
    o = f'<g transform="translate({x},{base}) scale({s})">' + shadow(0, 0, 140, 18)
    legs = "M-10 -190 L-80 -10 M10 -190 L70 -10"
    o += f'<path d="{legs}" stroke="{D}" stroke-width="56" stroke-linecap="round"/><path d="{legs}" stroke="#5c3d2e" stroke-width="32" stroke-linecap="round"/>'
    o += f'<path d="M-110 -8 L-66 -8 Q-60 -34 -84 -34 L-104 -30 Z" fill="{D}" {S(8)}/><path d="M50 -8 L110 -8 Q110 -34 80 -34 L56 -30 Z" fill="{D}" {S(8)}/>'
    armb = "M-5 -330 Q-50 -270 -90 -220"
    o += f'<path d="{armb}" fill="none" stroke="{D}" stroke-width="46" stroke-linecap="round"/><path d="{armb}" fill="none" stroke="{shirt}" stroke-width="24" stroke-linecap="round" opacity="0.85"/>'
    o += f'<rect x="-50" y="-370" width="100" height="200" rx="40" fill="{shirt}" {S(12)}/>'
    o += f'<path d="M-30 -340 L-30 -200" stroke="#fff" stroke-width="12" opacity="0.3" stroke-linecap="round"/>'
    armf = "M5 -330 Q50 -270 95 -240"
    o += f'<path d="{armf}" fill="none" stroke="{D}" stroke-width="46" stroke-linecap="round"/><path d="{armf}" fill="none" stroke="{shirt}" stroke-width="24" stroke-linecap="round"/>'
    o += f'<circle cx="104" cy="-236" r="20" fill="#e0a882" {S(10)}/>'
    o += f'<rect x="-16" y="-400" width="34" height="36" fill="#e0a882" {S(10)}/>'
    o += f'<circle cx="0" cy="-460" r="72" fill="#e0a882" {S(12)}/>'
    o += f'<path d="M-72 -455 Q-76 -535 0 -536 Q60 -536 70 -490 Q20 -500 -10 -470 Q-20 -440 -40 -430 Q-60 -420 -72 -455 Z" fill="#3b2a22" {S(8)}/>'
    o += f'<circle cx="38" cy="-462" r="8" fill="{D}"/><circle cx="36" cy="-432" r="12" fill="#e8907f" opacity="0.5"/>'
    o += f'<path d="M44 -420 Q56 -416 62 -426" fill="none" stroke="{D}" stroke-width="6" stroke-linecap="round"/>'
    return o + "</g>\n"


b = ""
for i, (fx, fy) in enumerate([(170, 850), (250, 820), (330, 850), (410, 820)]):
    b += f'<ellipse cx="{fx}" cy="{fy}" rx="26" ry="12" fill="{D}" opacity="{0.12 + i * 0.05:.2f}"/>'
b += walker(560, 860, 1.25)
b += "".join(f'<path d="M{x} {y} L{x + 70} {y}" stroke="{D}" stroke-width="10" stroke-linecap="round" opacity="0.5"/>' for x, y in [(260, 420), (230, 490), (270, 560)])
put(P, "Walk", "sand", b)

# Drive: side-view car with driver
b = f'<rect x="100" y="720" width="824" height="80" rx="20" fill="#8f8780" {S(12)}/>'
b += f'<path d="M150 760 L250 760 M350 760 L450 760 M550 760 L650 760 M750 760 L850 760" stroke="#f6ecd8" stroke-width="10" stroke-linecap="round"/>'
b += shadow(520, 725, 300, 20, 0.25)
b += f'<path d="M230 700 L230 580 Q235 545 280 540 L360 530 L430 430 Q445 410 480 410 L640 410 Q670 410 690 435 L760 530 L820 540 Q850 548 850 580 L850 700 Z" fill="#c8574b" {S(14)}/>'
b += f'<path d="M450 520 L490 440 L570 440 L570 520 Z" fill="#cfe0ea" {S(10)}/><path d="M600 520 L600 440 L660 440 L710 520 Z" fill="#cfe0ea" {S(10)}/>'
b += f'<circle cx="530" cy="480" r="30" fill="#f1c9a5" {S(8)}/><path d="M500 474 Q505 440 535 445 Q560 450 560 470 Q530 460 500 474 Z" fill="#3b2a22"/>'
b += f'<circle cx="546" cy="480" r="4" fill="{D}"/>'
b += f'<path d="M260 580 L820 580" stroke="#fff" stroke-width="14" opacity="0.3" stroke-linecap="round"/>'
b += f'<rect x="820" y="590" width="26" height="30" rx="8" fill="#f6ecd8" {S(8)}/>'
for wx in (350, 730):
    b += f'<circle cx="{wx}" cy="700" r="62" fill="#3b2a22" {S(14)}/><circle cx="{wx}" cy="700" r="26" fill="#bfb3a8" {S(8)}/>'
b += "".join(f'<path d="M{x} {y} L{x + 80} {y}" stroke="{D}" stroke-width="12" stroke-linecap="round" opacity="0.5"/>' for x, y in [(110, 560), (80, 620), (120, 680)])
b += f'<circle cx="780" cy="240" r="60" fill="#e0b04f" {S(12)}/>'
put(P, "Drive", "sky", b)

# Keep going / continue: winding road with chevrons
wd = "M400 914 C400 760 640 720 640 560 C640 400 400 400 420 250 L430 110"
b = road(wd, 210, dash=False)
b += tree_top(210, 560, 60) + tree_top(830, 330, 60) + tree_top(820, 820, 50) + tree_top(220, 250, 50)
b += car_top(410, 820, -8, "#5d82a8", 0.9)
for (x, y, a) in [(570, 660, -60), (620, 470, -110), (455, 320, -100), (432, 190, -92)]:
    r = math.radians(a)
    ux, uy = math.cos(r), math.sin(r)
    px, py = -uy, ux
    pts = [(x - ux * 20 + px * 50, y - uy * 20 + py * 50), (x + ux * 30, y + uy * 30), (x - ux * 20 - px * 50, y - uy * 20 - py * 50)]
    b += f'<path d="M{pts[0][0]:.0f} {pts[0][1]:.0f} L{pts[1][0]:.0f} {pts[1][1]:.0f} L{pts[2][0]:.0f} {pts[2][1]:.0f}" fill="none" stroke="{D}" stroke-width="40" stroke-linecap="round" stroke-linejoin="round"/>'
    b += f'<path d="M{pts[0][0]:.0f} {pts[0][1]:.0f} L{pts[1][0]:.0f} {pts[1][1]:.0f} L{pts[2][0]:.0f} {pts[2][1]:.0f}" fill="none" stroke="#e0b04f" stroke-width="20" stroke-linecap="round" stroke-linejoin="round"/>'
put(P, "_Keep_continue", "sage", b)

# ---------------- abstract -----------------
b = f'<rect x="170" y="600" width="260" height="60" rx="14" fill="#9a6a45" {S()}/><rect x="594" y="600" width="260" height="60" rx="14" fill="#9a6a45" {S()}/>'
b += shadow(300, 740, 150) + shadow(724, 740, 150)
b += f'<path d="M210 660 L230 740 L370 740 L390 660" fill="#7d5337" {S()}/><path d="M634 660 L654 740 L794 740 L814 660" fill="#7d5337" {S()}/>'
b += ball(300, 500, 100, "#c8574b") + ball(724, 500, 100, "#4f9a9a")
b += f'<path d="M512 150 m-110 0 a110 90 0 1 0 220 0 a110 90 0 1 0 -220 0" fill="#f6ecd8" {S()}/>'
b += f'<path d="M470 225 L450 290 L520 238 Z" fill="#f6ecd8" {S(12)}/><path d="M462 232 L520 232" stroke="#f6ecd8" stroke-width="16"/>'
b += f'<text x="512" y="190" text-anchor="middle" font-family="Arial, Helvetica, sans-serif" font-weight="bold" font-size="130" fill="#c8574b" stroke="{D}" stroke-width="10" paint-order="stroke">?</text>'
b = b.replace('M512 150 m-110 0 a110 90 0 1 0 220 0 a110 90 0 1 0 -220 0', 'M512 145 m-110 0 a110 90 0 1 0 220 0 a110 90 0 1 0 -220 0')
put(P, "Which", "beige", b)

# With: two friends holding hands
b = person(370, 860, 1.05, shirt="#d9825b", pants="#5c3d2e", arms=("M-78 -270 Q-110 -200 -100 -140 M78 -270 Q120 -210 142 -150", [(-100, -130), (142, -150)]))
b += person(654, 860, 1.05, shirt="#4f9a9a", pants="#3b2a22", skin="#e0a882", hair="short", arms=("M-78 -270 Q-120 -210 -128 -150 M78 -270 Q110 -200 100 -140", [(-128, -150), (100, -130)]))
b += f'<path d="M512 170 C480 130 430 160 470 210 L512 250 L554 210 C594 160 544 130 512 170 Z" fill="#c8574b" {S(10)}/>'
put(P, "With", "rose", b)

# See: eye
b = f'<path d="M150 512 Q512 160 874 512 Q512 864 150 512 Z" fill="#f6ecd8" {S(16)}/>'
b += f'<circle cx="512" cy="512" r="170" fill="#5d82a8" {S(14)}/><circle cx="512" cy="512" r="80" fill="{D}"/>'
b += f'<circle cx="560" cy="460" r="36" fill="#fff" opacity="0.85"/><circle cx="470" cy="570" r="14" fill="#fff" opacity="0.6"/>'
b += f'<path d="M190 480 Q512 150 834 480" fill="none" stroke="{D}" stroke-width="6" opacity="0"/>'
b += "".join(f'<path d="M{x1} {y1} L{x2} {y2}" {S(14)}/>' for x1, y1, x2, y2 in [(512, 330, 512, 250), (360, 360, 320, 290), (664, 360, 704, 290), (240, 430, 190, 380), (784, 430, 834, 380)])
b += f'<path d="M150 512 Q512 160 874 512 Q512 864 150 512 Z" fill="none" {S(16)}/>'
put(P, "See", "sky", b)

# Once / When: clock
b = shadow(512, 880, 260)
b += f'<circle cx="512" cy="500" r="330" fill="#c8574b" {S(16)}/><circle cx="512" cy="500" r="270" fill="#f6ecd8" {S(12)}/>'
for i in range(12):
    a = math.radians(i * 30)
    r1 = 230 if i % 3 else 205
    b += f'<path d="M{512 + r1 * math.sin(a):.0f} {500 - r1 * math.cos(a):.0f} L{512 + 250 * math.sin(a):.0f} {500 - 250 * math.cos(a):.0f}" {S(14 if i % 3 == 0 else 10)}/>'
b += f'<path d="M512 500 L512 330" {S(22)}/><path d="M512 500 L640 560" {S(22)}/>'
b += f'<circle cx="512" cy="500" r="26" fill="{D}"/>'
b += f'<path d="M290 330 Q360 240 470 220" fill="none" stroke="#fff" stroke-width="18" opacity="0.4" stroke-linecap="round"/>'
b += f'<path d="M270 190 L330 250 M754 190 L694 250" {S(26)}/>'
b += f'<circle cx="265" cy="180" r="50" fill="#e0b04f" {S(12)}/><circle cx="759" cy="180" r="50" fill="#e0b04f" {S(12)}/>'
put(P, "Once_When", "beige", b)

# Come across / find / see: surprised person discovering a wallet
b = person(380, 860, 1.2, shirt="#8a5a86", arms=("M-78 -270 Q-130 -300 -150 -350 M78 -270 Q150 -240 190 -190", [(-150, -360), (196, -184)]), mouth="o", brows=True)
b += shadow(740, 860, 110, 18)
b += f'<rect x="640" y="770" width="200" height="90" rx="14" fill="#9a6a45" {S()}/><path d="M640 800 L840 800" {S(8)}/><rect x="780" y="790" width="46" height="34" rx="8" fill="#e0b04f" {S(8)}/>'
b += sparkle(660, 700, 26) + sparkle(820, 690, 20) + sparkle(740, 640, 30)
b += "".join(f'<path d="M{x1} {y1} L{x2} {y2}" {S(12)}/>' for x1, y1, x2, y2 in [(520, 330, 560, 300), (530, 400, 580, 395), (480, 270, 500, 225)])
b += f'<path d="M430 480 Q560 560 640 760" fill="none" stroke="{D}" stroke-width="10" stroke-dasharray="4 26" stroke-linecap="round"/>'
put(P, "Come across_find_see", "rose", b)

# ---------------- PointThings -----------------
PT = "PointThings"
GROUND = (f'<rect x="0" y="480" width="1024" height="544" fill="url(#gr)"/>'
          f'<path d="M0 480 L1024 480" stroke="{D}" stroke-width="10"/>'
          f'<path d="M-10 480 Q120 400 260 470 Q380 410 520 475 Q700 390 860 470 Q960 420 1034 470" fill="#b8c7a0" stroke="{D}" stroke-width="10"/>')
GDEF = '<linearGradient id="gr" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#c9d6ac"/><stop offset="100%" stop-color="#8faa6a"/></linearGradient>'
OTHER = "#e0b04f"


def pt(key, balls, tip, ang, s=1.0, extra=""):
    b = GROUND
    for (x, y, r, main) in balls:
        b += shadow(x, y + r * 0.95, r * 1.0, r * 0.2, 0.2)
        b += ball(x, y, r, "#c8574b" if main else OTHER, 1 if main else 0.4)
    b += extra
    b += hand(tip[0], tip[1], ang, s)
    put(PT, key, "sky", b, GDEF)


def dotted(x1, y1, x2, y2):
    return f'<path d="M{x1} {y1} L{x2} {y2}" stroke="{D}" stroke-width="10" stroke-dasharray="2 24" stroke-linecap="round"/>\n'


# This: object close at hand
pt("This", [(620, 720, 150, True)], (440, 640), 20, 1.0)
pt("This one", [(420, 780, 95, False), (640, 720, 110, True), (850, 780, 80, False)],
   (520, 620), 25, 0.9, sparkle(780, 560, 28) + sparkle(520, 560, 20))
# That: medium distance
pt("That", [(680, 590, 60, True)], (380, 760), -20, 0.95, dotted(400, 740, 610, 620))
pt("That one", [(520, 590, 45, False), (680, 600, 60, True), (840, 590, 45, False)], (340, 780), -18, 0.9,
   dotted(360, 760, 610, 640) + ring(680, 600, 90))
# Over there: far away on horizon
pt("Over there", [(760, 462, 20, True)], (360, 700), -30, 0.95, dotted(380, 690, 730, 480) + sparkle(810, 420, 22) + sparkle(720, 410, 14))
pt("That one over there", [(640, 462, 16, False), (720, 460, 20, True), (800, 462, 16, False)], (340, 720), -28, 0.95,
   dotted(360, 705, 690, 480) + ring(720, 460, 44, w=8))

for (cat, key), s in OUT.items():
    d = os.path.join(ROOT, "svg", cat)
    os.makedirs(d, exist_ok=True)
    open(os.path.join(d, key + ".svg"), "w").write(s)
print(len(OUT))

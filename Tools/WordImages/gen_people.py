"""Generator for FamilyAndPeople / Gender / Ocupation SVGs."""
import os

ROOT = os.path.dirname(os.path.abspath(__file__))
OL = "#2e211b"
SKIN = "#f1c9a5"
SKIN2 = "#e0a882"
SKIN3 = "#b57d58"
HAIR = "#3b2a22"
GREY = "#d9d3cc"
CHEEK = "#e8907f"

BGS = {
    "beige": ("#f3e6d6", "#c9ab93"),
    "rose": ("#f1e0dc", "#bf9a98"),
    "sage": ("#e7ecdc", "#a9b595"),
    "sky": ("#e3ecf1", "#9fb3c1"),
    "sand": ("#f5ead0", "#cfb27e"),
}


def svg(bg, body):
    a, b = BGS[bg]
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">
<defs>
<radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{a}"/><stop offset="100%" stop-color="{b}"/></radialGradient>
<radialGradient id="halo" cx="50%" cy="50%" r="50%"><stop offset="0%" stop-color="#fffaf0" stop-opacity="0.95"/><stop offset="60%" stop-color="#fff4dc" stop-opacity="0.55"/><stop offset="100%" stop-color="#fff4dc" stop-opacity="0"/></radialGradient>
</defs>
<rect width="1024" height="1024" fill="url(#bg)"/>
{body}
</svg>
'''


def pts(p):
    return "M" + " L".join(f"{x:.0f} {y:.0f}" for x, y in p)


def limb(p, color, w=34, sleeve=None, sleeve_frac=0.45):
    """Thick outlined limb along polyline p."""
    d = pts(p)
    s = f'<path d="{d}" fill="none" stroke="{OL}" stroke-width="{w + 22}" stroke-linecap="round" stroke-linejoin="round"/>'
    s += f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"/>'
    if sleeve:
        (x0, y0), (x1, y1) = p[0], p[1]
        xm, ym = x0 + (x1 - x0) * sleeve_frac * 2, y0 + (y1 - y0) * sleeve_frac * 2
        if sleeve_frac >= 0.5:  # long sleeve -> whole arm
            s = s.replace(f'stroke="{color}"', f'stroke="{sleeve}"')
            return s
        s += f'<path d="M{x0:.0f} {y0:.0f} L{xm:.0f} {ym:.0f}" fill="none" stroke="{OL}" stroke-width="{w + 22}" stroke-linecap="butt"/>'
        s += f'<path d="M{x0:.0f} {y0:.0f} L{xm:.0f} {ym:.0f}" fill="none" stroke="{sleeve}" stroke-width="{w + 4}" stroke-linecap="round"/>'
    return s


ARM_POSES = {
    "down": [(86, -352), (114, -272), (112, -200)],
    "wave": [(86, -352), (150, -410), (168, -505)],
    "wai": [(82, -350), (98, -262), (26, -318)],
    "self": [(86, -352), (104, -262), (30, -300)],
    "hold": [(86, -352), (106, -268), (64, -236)],
    "out": [(86, -352), (150, -310), (215, -305)],
    "outlow": [(86, -352), (140, -280), (190, -240)],
    "up": [(86, -352), (140, -430), (150, -520)],
    "chest": [(86, -352), (110, -270), (70, -268)],
    "hip": [(86, -352), (140, -290), (96, -228)],
}


def hair_back(style, hc):
    if style == "long":
        return f'<path d="M-106 -30 Q-112 -150 0 -154 Q112 -150 106 -30 Q118 90 110 150 L-110 150 Q-118 90 -106 -30 Z" fill="{hc}" stroke="{OL}" stroke-width="12"/>'
    if style == "bob":
        return f'<path d="M-108 -20 Q-112 -150 0 -152 Q112 -150 108 -20 Q112 50 90 60 L-90 60 Q-112 50 -108 -20 Z" fill="{hc}" stroke="{OL}" stroke-width="12"/>'
    if style == "bun":
        return (f'<circle cx="0" cy="-118" r="46" fill="{hc}" stroke="{OL}" stroke-width="12"/>'
                f'<path d="M-104 0 Q-110 -140 0 -142 Q110 -140 104 0 Q100 30 80 30 L-80 30 Q-100 30 -104 0 Z" fill="{hc}" stroke="{OL}" stroke-width="12"/>')
    if style == "ponytail":
        return (f'<path d="M80 -70 Q170 -40 150 60 Q130 20 100 0 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
                f'<path d="M-104 -10 Q-110 -150 0 -152 Q110 -150 104 -10 Z" fill="{hc}" stroke="{OL}" stroke-width="12"/>')
    if style == "pigtails":
        return (f'<path d="M-90 -40 Q-170 -20 -150 70 Q-120 20 -95 10 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
                f'<path d="M90 -40 Q170 -20 150 70 Q120 20 95 10 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
                f'<path d="M-106 -10 Q-110 -150 0 -152 Q110 -150 106 -10 Q106 20 90 20 L-90 20 Q-106 20 -106 -10 Z" fill="{hc}" stroke="{OL}" stroke-width="12"/>')
    return ""


def hair_front(style, hc):
    if style in ("long", "bob", "pigtails", "ponytail"):
        s = f'<path d="M-104 -6 Q-106 -112 0 -113 Q106 -112 104 -6 Q70 -58 10 -62 Q-54 -58 -104 -6 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
        if style == "pigtails":
            s += f'<circle cx="-96" cy="-30" r="14" fill="#c8574b" stroke="{OL}" stroke-width="7"/><circle cx="96" cy="-30" r="14" fill="#c8574b" stroke="{OL}" stroke-width="7"/>'
        if style == "ponytail":
            s += f'<circle cx="92" cy="-62" r="13" fill="#e0b04f" stroke="{OL}" stroke-width="7"/>'
        return s
    if style == "bun":
        return f'<path d="M-104 -8 Q-106 -112 0 -113 Q106 -112 104 -8 Q70 -66 0 -64 Q-70 -66 -104 -8 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/><path d="M-40 -90 Q0 -100 40 -90" fill="none" stroke="{OL}" stroke-width="6" opacity="0.4"/>'
    if style == "short":
        return f'<path d="M-100 -12 Q-108 -112 0 -114 Q108 -112 100 -12 Q92 -58 60 -66 Q30 -48 -4 -70 Q-46 -50 -84 -60 Q-98 -40 -100 -12 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    if style == "spiky":
        return f'<path d="M-100 -12 Q-110 -90 -60 -110 L-40 -128 L-20 -112 L10 -132 L30 -110 L64 -120 L70 -98 Q108 -80 100 -12 Q92 -58 60 -64 Q30 -46 -4 -68 Q-46 -48 -84 -58 Q-98 -40 -100 -12 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    if style == "old":  # receding grey
        return (f'<path d="M-101 -2 Q-108 -60 -80 -80 Q-78 -40 -92 -2 Z" fill="{hc}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
                f'<path d="M101 -2 Q108 -60 80 -80 Q78 -40 92 -2 Z" fill="{hc}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>')
    if style == "neat":  # side part, adult man
        return f'<path d="M-100 -8 Q-110 -114 0 -116 Q110 -114 100 -8 Q96 -60 70 -74 Q10 -70 -30 -96 Q-40 -60 -90 -56 Q-98 -36 -100 -8 Z" fill="{hc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    return ""


def hat(kind):
    if kind == "police":
        return (f'<path d="M-112 -58 Q-128 -150 0 -156 Q128 -150 112 -58 Z" fill="#6b4a33" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
                f'<rect x="-100" y="-80" width="200" height="34" rx="8" fill="#3b2a22" stroke="{OL}" stroke-width="10"/>'
                f'<path d="M-96 -48 Q0 -6 96 -48 Q0 -28 -96 -48 Z" fill="#2e211b" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
                f'<path d="M0 -140 L16 -112 L0 -86 L-16 -112 Z" fill="#e0b04f" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>'
                f'<path d="M-80 -120 Q-40 -145 10 -145" fill="none" stroke="#fff" stroke-width="10" opacity="0.3" stroke-linecap="round"/>')
    if kind == "guard":
        return (f'<path d="M-104 -52 Q-110 -150 0 -152 Q110 -150 104 -52 Z" fill="#34445f" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
                f'<path d="M-100 -52 Q0 -18 118 -40 Q130 -30 110 -22 Q0 -8 -100 -40 Z" fill="#22304a" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
                f'<circle cx="0" cy="-100" r="20" fill="#e0b04f" stroke="{OL}" stroke-width="7"/>')
    if kind == "nurse":
        return ""
    return ""


def person(x, y, s=1.0, *, skin=SKIN, hair="short", hc=HAIR, top="#5d82a8", sleeve="short",
           bottom="pants", bc="#5c3d2e", arms=("down", "down"), glasses=False, mustache=False,
           earrings=False, opacity=1.0, head_scale=1.0, torso="tshirt", hat_kind=None, extra_back="",
           extra_front="", extra_top="", shadow=True, collar=None, shoes="#5c3d2e", mouth="smile",
           flip=False, legs_hidden=False):
    """Standing person, feet at (x, y). Returns (svg, hands dict world coords)."""
    o = []
    if shadow:
        o.append(f'<ellipse cx="{x}" cy="{y + 4}" rx="{105 * s:.0f}" ry="{20 * s:.0f}" fill="{OL}" opacity="0.15"/>')
    g = []
    g.append(extra_back)
    # legs / bottom
    if not legs_hidden:
        if bottom in ("pants", "shorts"):
            if bottom == "shorts":
                g.append(f'<path d="M-44 -110 L-44 -24" stroke="{OL}" stroke-width="50" stroke-linecap="round"/><path d="M44 -110 L44 -24" stroke="{OL}" stroke-width="50" stroke-linecap="round"/>')
                g.append(f'<path d="M-44 -110 L-44 -24" stroke="{skin}" stroke-width="28" stroke-linecap="round"/><path d="M44 -110 L44 -24" stroke="{skin}" stroke-width="28" stroke-linecap="round"/>')
                g.append(f'<path d="M-80 -205 L-84 -100 L-8 -100 L0 -140 L8 -100 L84 -100 L80 -205 Z" fill="{bc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>')
            else:
                g.append(f'<path d="M-78 -205 L-80 -26 L-10 -26 L0 -150 L10 -26 L80 -26 L78 -205 Z" fill="{bc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>')
                g.append(f'<path d="M-58 -180 L-60 -50" stroke="#fff" stroke-width="10" opacity="0.2" stroke-linecap="round"/>')
        elif bottom in ("skirt", "longskirt"):
            g.append(f'<path d="M-40 -130 L-40 -24" stroke="{OL}" stroke-width="48" stroke-linecap="round"/><path d="M40 -130 L40 -24" stroke="{OL}" stroke-width="48" stroke-linecap="round"/>')
            g.append(f'<path d="M-40 -130 L-40 -24" stroke="{skin}" stroke-width="26" stroke-linecap="round"/><path d="M40 -130 L40 -24" stroke="{skin}" stroke-width="26" stroke-linecap="round"/>')
            yb = -110 if bottom == "skirt" else -44
            wb = 100 if bottom == "skirt" else 92
            g.append(f'<path d="M-80 -215 L-{wb} {yb} Q0 {yb + 14} {wb} {yb} L80 -215 Z" fill="{bc}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>')
            if bottom == "longskirt":
                g.append(f'<path d="M-84 -120 Q0 -108 84 -120" fill="none" stroke="{OL}" stroke-width="8" opacity="0.5"/><path d="M-88 -90 Q0 -78 88 -90" fill="none" stroke="#e0b04f" stroke-width="12" opacity="0.8"/>')
        # shoes
        g.append(f'<ellipse cx="-44" cy="-14" rx="36" ry="17" fill="{shoes}" stroke="{OL}" stroke-width="10"/><ellipse cx="44" cy="-14" rx="36" ry="17" fill="{shoes}" stroke="{OL}" stroke-width="10"/>')
    # arms behind (down poses) drawn before torso
    la, ra = arms
    hands = {}
    arm_front = []
    arm_back = []
    sl = {"short": 0.3, "long": 0.6, "none": 0.0}[sleeve]
    slc = top if torso != "coat" else "#fbf8f2"
    if torso == "coat":
        sl = 0.6
    for side, pose in ((-1, la), (1, ra)):
        if pose is None:
            continue
        p = ARM_POSES[pose] if isinstance(pose, str) else pose
        p = [(side * px, py) for px, py in p]
        seg = limb(p, skin, 34, sleeve=slc if sl > 0 else None, sleeve_frac=sl if sl > 0 else 0.3)
        hx, hy = p[-1]
        if pose == "wai":
            hand = ""
        else:
            hand = f'<circle cx="{hx}" cy="{hy}" r="23" fill="{skin}" stroke="{OL}" stroke-width="10"/>'
        hands["l" if side < 0 else "r"] = (x + s * hx * (-1 if flip else 1), y + s * hy)
        (arm_front if pose in ("wai", "self", "hold", "chest", "hip") else arm_back).append(seg + hand)
    if hair == "long":
        g.append(f'<g transform="translate(0 -500) scale({head_scale})">' + hair_back("long", hc) + '</g>')
    g.extend(arm_back)
    # torso
    body = "M-96 -325 Q-96 -382 -40 -388 L40 -388 Q96 -382 96 -325 L84 -196 Q0 -186 -84 -196 Z"
    if torso == "coat":
        g.append(f'<path d="M-40 -388 L40 -388 L60 -300 L-60 -300 Z" fill="{top}" stroke="{OL}" stroke-width="10"/>')
        g.append(f'<path d="M-96 -325 Q-96 -382 -40 -388 L0 -300 L40 -388 Q96 -382 96 -325 L102 -118 Q0 -108 -102 -118 Z" fill="#fbf8f2" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>')
        g.append(f'<path d="M-40 -388 L0 -300 L40 -388 Z" fill="{top}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>')
        g.append(f'<path d="M0 -300 L0 -114" stroke="{OL}" stroke-width="9"/><path d="M-40 -388 L-12 -290 L-44 -296 M40 -388 L12 -290 L44 -296" fill="none" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/>')
        g.append(f'<rect x="34" y="-250" width="44" height="40" rx="6" fill="none" stroke="{OL}" stroke-width="8"/>')
        g.append(f'<path d="M-70 -350 L-78 -150" stroke="#2e211b" stroke-width="12" opacity="0.08" stroke-linecap="round"/>')
    else:
        g.append(f'<path d="{body}" fill="{top}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>')
        g.append(f'<path d="M-72 -340 Q-78 -280 -70 -215" fill="none" stroke="#fff" stroke-width="14" opacity="0.25" stroke-linecap="round"/>')
        if torso == "tshirt":
            g.append(f'<path d="M-32 -388 Q0 -350 32 -388" fill="none" stroke="{OL}" stroke-width="9"/>')
        elif torso == "collar":
            g.append(f'<path d="M-38 -390 L-6 -340 L-40 -330 Z M38 -390 L6 -340 L40 -330 Z" fill="{collar or "#f6ecd8"}" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/>')
            g.append(f'<path d="M0 -345 L0 -200" stroke="{OL}" stroke-width="7" opacity="0.6"/><circle cx="0" cy="-310" r="5" fill="{OL}"/><circle cx="0" cy="-260" r="5" fill="{OL}"/>')
        elif torso == "vneck":
            g.append(f'<path d="M-34 -388 L0 -336 L34 -388" fill="none" stroke="{OL}" stroke-width="9" stroke-linejoin="round"/>')
        elif torso == "mandarin":
            g.append(f'<path d="M-30 -392 L-30 -372 Q0 -364 30 -372 L30 -392" fill="{top}" stroke="{OL}" stroke-width="8"/><path d="M0 -370 L0 -200" stroke="{OL}" stroke-width="7" opacity="0.6"/>')
            g.append(f'<path d="M-4 -330 L22 -330 M-4 -290 L22 -290 M-4 -250 L22 -250" stroke="{OL}" stroke-width="6" opacity="0.6"/>')
    g.append(extra_top)
    # neck + head
    g.append(f'<rect x="-22" y="-420" width="44" height="40" fill="{skin}" stroke="{OL}" stroke-width="10"/>')
    hs = head_scale
    hd = [f'<g transform="translate(0 -500) scale({hs})">']
    hd.append(hair_back(hair, hc) if hair != "long" else "")
    hd.append(f'<circle cx="0" cy="0" r="100" fill="{skin}" stroke="{OL}" stroke-width="12"/>')
    hd.append(f'<circle cx="-98" cy="12" r="16" fill="{skin}" stroke="{OL}" stroke-width="9"/><circle cx="98" cy="12" r="16" fill="{skin}" stroke="{OL}" stroke-width="9"/>' if hair in ("short", "neat", "spiky", "old") else "")
    hd.append(f'<circle cx="0" cy="0" r="100" fill="none" stroke="{OL}" stroke-width="12"/>' if hair in ("short", "neat", "spiky", "old") else "")
    hd.append(f'<circle cx="0" cy="0" r="94" fill="{skin}"/>' if hair in ("short", "neat", "spiky", "old") else "")
    hd.append(hair_front(hair, hc))
    hd.append(f'<circle cx="-36" cy="10" r="10" fill="{OL}"/><circle cx="36" cy="10" r="10" fill="{OL}"/>')
    hd.append(f'<circle cx="-60" cy="42" r="17" fill="{CHEEK}" opacity="0.5"/><circle cx="60" cy="42" r="17" fill="{CHEEK}" opacity="0.5"/>')
    if mouth == "smile":
        hd.append(f'<path d="M-24 46 Q0 68 24 46" fill="none" stroke="{OL}" stroke-width="8" stroke-linecap="round"/>')
    elif mouth == "open":
        hd.append(f'<path d="M-24 44 Q0 80 24 44 Z" fill="#8f3b35" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>')
    if glasses:
        hd.append(f'<circle cx="-36" cy="10" r="24" fill="#fff" fill-opacity="0.25" stroke="{OL}" stroke-width="7"/><circle cx="36" cy="10" r="24" fill="#fff" fill-opacity="0.25" stroke="{OL}" stroke-width="7"/><path d="M-12 8 Q0 2 12 8" fill="none" stroke="{OL}" stroke-width="7"/>')
    if mustache:
        hd.append(f'<path d="M-30 38 Q-14 26 0 34 Q14 26 30 38 Q14 44 0 40 Q-14 44 -30 38 Z" fill="{hc}" stroke="{OL}" stroke-width="5" stroke-linejoin="round"/>')
    if earrings:
        hd.append(f'<circle cx="-96" cy="40" r="9" fill="#e0b04f" stroke="{OL}" stroke-width="5"/><circle cx="96" cy="40" r="9" fill="#e0b04f" stroke="{OL}" stroke-width="5"/>')
    if hat_kind:
        hd.append(hat(hat_kind))
    hd.append("</g>")
    g.extend(hd)
    g.extend(arm_front)
    # wai palms
    if la == "wai" and ra == "wai":
        g.append(f'<path d="M0 -380 Q-30 -340 -28 -300 Q-24 -282 0 -280 Q24 -282 28 -300 Q30 -340 0 -380 Z" fill="{skin}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/><path d="M0 -372 L0 -284" stroke="{OL}" stroke-width="6"/>')
    g.append(extra_front)
    fl = " scale(-1 1)" if flip else ""
    op = f' opacity="{opacity}"' if opacity < 1 else ""
    o.append(f'<g transform="translate({x} {y}) scale({s}){fl}"{op}>' + "".join(g) + "</g>")
    return "".join(o), hands


def halo(x, y, r):
    return f'<circle cx="{x}" cy="{y}" r="{r}" fill="url(#halo)"/>'


def heart(x, y, s=1.0, fill="#c8574b"):
    return (f'<g transform="translate({x} {y}) scale({s})"><path d="M0 -10 C-20 -45 -70 -30 -60 10 Q-50 40 0 70 Q50 40 60 10 C70 -30 20 -45 0 -10 Z" fill="{fill}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
            f'<path d="M-40 -8 Q-38 -22 -24 -24" fill="none" stroke="#fff" stroke-width="9" opacity="0.4" stroke-linecap="round"/></g>')


def ring(x, y, s=1.0):
    return (f'<g transform="translate({x} {y}) scale({s})"><circle cx="0" cy="10" r="30" fill="none" stroke="{OL}" stroke-width="22"/><circle cx="0" cy="10" r="30" fill="none" stroke="#e0b04f" stroke-width="10"/>'
            f'<path d="M-16 -30 L16 -30 L24 -42 L0 -60 L-24 -42 Z" fill="#cfe6ee" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/></g>')


def star(x, y, r=22, fill="#e0b04f"):
    import math
    p = []
    for i in range(10):
        a = -math.pi / 2 + i * math.pi / 5
        rr = r if i % 2 == 0 else r * 0.45
        p.append((x + rr * math.cos(a), y + rr * math.sin(a)))
    return f'<path d="{pts(p)} Z" fill="{fill}" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>'


def sparkles(x, y, r):
    return star(x - r, y - r * 0.6, 20) + star(x + r, y - r * 0.8, 16) + star(x + r * 0.9, y + r * 0.1, 11)


def arrow_down(x, y, s=1.0, fill="#e0b04f"):
    return f'<g transform="translate({x} {y}) scale({s})"><path d="M-22 -60 L22 -60 L22 -10 L48 -10 L0 40 L-48 -10 L-22 -10 Z" fill="{fill}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/></g>'


def arrow_v(x, y1, y2, fill="#e0b04f", w=26):
    """vertical arrow from y1 (tail) to y2 (head)."""
    d = 1 if y2 > y1 else -1
    hy = y2 - d * 44
    return (f'<path d="M{x - w / 2} {y1} L{x + w / 2} {y1} L{x + w / 2} {hy} L{x + w / 2 + 24} {hy} L{x} {y2} L{x - w / 2 - 24} {hy} L{x - w / 2} {hy} Z" '
            f'fill="{fill}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>')


def bubble(x, y, w, h, tail, inner="", fill="#fbf6ec"):
    tx, ty = tail
    return (f'<path d="M{x - w / 2 + 40} {y - h / 2} L{x + w / 2 - 40} {y - h / 2} Q{x + w / 2} {y - h / 2} {x + w / 2} {y - h / 2 + 40} L{x + w / 2} {y + h / 2 - 40} Q{x + w / 2} {y + h / 2} {x + w / 2 - 40} {y + h / 2} '
            f'L{(x + tx) / 2 + 30} {y + h / 2} L{tx} {ty} L{(x + tx) / 2 - 10} {y + h / 2} L{x - w / 2 + 40} {y + h / 2} Q{x - w / 2} {y + h / 2} {x - w / 2} {y + h / 2 - 40} L{x - w / 2} {y - h / 2 + 40} Q{x - w / 2} {y - h / 2} {x - w / 2 + 40} {y - h / 2} Z" '
            f'fill="{fill}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>' + inner)


def wai_icon(x, y, s=1.0, skin=SKIN):
    return (f'<g transform="translate({x} {y}) scale({s})">'
            f'<path d="M0 -70 Q-40 -20 -36 30 Q-30 60 0 62 Q30 60 36 30 Q40 -20 0 -70 Z" fill="{skin}" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
            f'<path d="M0 -62 L0 58" stroke="{OL}" stroke-width="7"/>'
            f'<path d="M-36 40 L-44 80 L44 80 L36 40" fill="#f6ecd8" stroke="{OL}" stroke-width="9" stroke-linejoin="round"/>'
            f'<path d="M-62 -30 L-80 -44 M-66 0 L-88 0 M62 -30 L80 -44 M66 0 L88 0" stroke="#e0b04f" stroke-width="9" stroke-linecap="round"/></g>')


def pin(x, y, s=1.0, fill="#c8574b"):
    return (f'<g transform="translate({x} {y}) scale({s})"><path d="M0 60 Q-60 -10 -60 -50 Q-60 -110 0 -110 Q60 -110 60 -50 Q60 -10 0 60 Z" fill="{fill}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
            f'<circle cx="0" cy="-50" r="22" fill="#f6ecd8" stroke="{OL}" stroke-width="9"/><path d="M-38 -70 Q-30 -94 -8 -98" fill="none" stroke="#fff" stroke-width="9" opacity="0.35" stroke-linecap="round"/></g>')


# ---------------- character presets -----------------
def P(**kw):
    return kw


MOM = P(hair="long", top="#c8574b", bottom="longskirt", bc="#8a5a86", earrings=True, sleeve="short", torso="vneck")
DAD = P(hair="neat", top="#5d82a8", bottom="pants", bc="#5c3d2e", torso="collar", sleeve="short", skin=SKIN2)
ME = P(hair="spiky", top="#e0b04f", bottom="shorts", bc="#4f9a9a", torso="tshirt")
OBRO = P(hair="short", top="#7fa05a", bottom="pants", bc="#5d82a8", torso="tshirt", skin=SKIN2)
OSIS = P(hair="ponytail", top="#d9825b", bottom="skirt", bc="#5d82a8", torso="tshirt")
YBRO = P(hair="spiky", top="#5d82a8", bottom="shorts", bc="#5c3d2e", torso="tshirt", skin=SKIN2)
YSIS = P(hair="pigtails", top="#e8a0a8", bottom="skirt", bc="#8a5a86", torso="tshirt")
GMA = P(hair="bun", hc=GREY, top="#8a5a86", bottom="longskirt", bc="#5c3d2e", glasses=True, torso="mandarin", sleeve="long")
GPA = P(hair="old", hc=GREY, top="#f6ecd8", bottom="pants", bc="#7d6450", glasses=True, mustache=True, torso="mandarin", skin=SKIN2)

KID = 0.58
TEEN = 0.74
KIDHEAD = 1.18


def fam(x, y, preset, s=1.0, faded=False, **over):
    kw = dict(preset)
    kw.update(over)
    if s < 0.8 and "head_scale" not in kw:
        kw["head_scale"] = KIDHEAD if s < 0.66 else 1.1
    if faded:
        kw["opacity"] = 0.4
    return person(x, y, s, **kw)


def cane(x, y, top_y):
    return (f'<path d="M{x} {y} L{x} {top_y} Q{x} {top_y - 40} {x - 36} {top_y - 40}" fill="none" stroke="{OL}" stroke-width="28" stroke-linecap="round"/>'
            f'<path d="M{x} {y} L{x} {top_y} Q{x} {top_y - 40} {x - 36} {top_y - 40}" fill="none" stroke="#9a6a45" stroke-width="12" stroke-linecap="round"/>')


def head_top(y, s, hs=1.0):
    return y + s * (-500 - 115 * hs)


out = {}
G = 860  # ground


def H(x, s=1.0, hs=None):
    """halo centered on person body"""
    return halo(x, G - 330 * s, int(300 * s + 40))


# ---------------- FamilyAndPeople ----------------
# Mom: portrait-ish single, like the sample but full body.
b = halo(512, 520, 360)
p, _ = fam(512, 890, MOM, 1.12, arms=("down", "wave"))
b += p + heart(512, 590, 0.55, "#f6ecd8")
out["FamilyAndPeople/Mom"] = svg("rose", b)

b = halo(512, 520, 360)
p, _ = fam(512, 890, DAD, 1.12, arms=("down", "wave"))
b += p
out["FamilyAndPeople/Dad"] = svg("sky", b)

# Son / Daughter: faded parents, child highlighted in middle
for key, kid, bg in (("Son", YBRO, "sky"), ("Daughter", YSIS, "rose")):
    b = ""
    p1, _ = fam(260, G, MOM, 0.95, faded=True, arms=("down", "outlow"))
    p2, _ = fam(764, G, DAD, 0.95, faded=True, arms=("outlow", "down"))
    b += p1 + p2 + halo(512, G - 220, 240)
    p3, _ = fam(512, G, kid, 0.62, arms=("wave", "down"))
    b += p3 + sparkles(512, G - 470, 110)
    out[f"FamilyAndPeople/{key}"] = svg(bg, b)

# Husband / Wife
for key, hl, bg in (("Husband", "dad", "sky"), ("Wife", "mom", "rose")):
    b = ""
    fm = hl != "mom"
    fd = hl != "dad"
    if hl == "dad":
        b += halo(640, G - 330, 330)
    else:
        b += halo(384, G - 330, 330)
    p1, _ = fam(384, G, MOM, 0.98, faded=fm, arms=("down", "outlow"))
    p2, _ = fam(640, G, DAD, 1.02, faded=fd, arms=("outlow", "down"))
    b += p1 + p2
    b += ring(512, 215, 1.2) + heart(420, 190, 0.45) + heart(604, 190, 0.45)
    out[f"FamilyAndPeople/{key}"] = svg(bg, b)

# Girlfriend_Boyfriend: young couple holding hands, heart balloon
b = ""
p1, _ = person(390, G, 0.92, hair="bob", top="#e8a0a8", bottom="skirt", bc="#5d82a8", torso="tshirt", arms=("down", "outlow"))
p2, _ = person(634, G, 0.95, hair="spiky", top="#4f9a9a", bottom="pants", bc="#5c3d2e", torso="tshirt", skin=SKIN2, arms=("outlow", "wave"))
b += p1 + p2 + f'<circle cx="512" cy="{G - 0.93 * 240:.0f}" r="24" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>'
b += heart(512, 190, 1.0) + heart(380, 190, 0.45, "#e8a0a8") + heart(650, 175, 0.4, "#e8a0a8")
out["FamilyAndPeople/Girlfriend_Boyfriend"] = svg("rose", b)

# Grandparents
for key, gp, par, bg in (("Grandma_Mom side_", GMA, MOM, "rose"), ("Grandma_Dad side_", GMA, DAD, "sky"),
                         ("Grandpa_Mom side_", GPA, MOM, "rose"), ("Grandpa_Dad side_", GPA, DAD, "sky")):
    b = halo(360, G - 300, 320)
    extra = ""
    if gp is GPA:
        p1, h = fam(360, G, gp, 0.96, arms=("down", "hold"))
        extra = cane(360 + 0.96 * 64 + 6, G - 4, G - 0.96 * 236 + 20)
    else:
        p1, h = fam(360, G, gp, 0.94, arms=("down", "outlow"))
    p2, _ = fam(680, G, par, 1.0, faded=True, arms=("outlow", "down"))
    # family-tree link
    link = f'<path d="M360 {G - 690} Q520 {G - 800} 680 {G - 690}" fill="none" stroke="{OL}" stroke-width="12" stroke-dasharray="4 26" stroke-linecap="round" opacity="0.6"/>'
    b += p2 + link + p1 + extra + heart(520, G - 770, 0.4)
    out[f"FamilyAndPeople/{key}"] = svg(bg, b)

# Grand children: faded grandparents, two small kids highlighted in the middle
b = ""
p1, _ = fam(230, G, GMA, 0.92, faded=True, arms=("down", "outlow"))
p2, _ = fam(794, G, GPA, 0.94, faded=True, arms=("outlow", "down"))
b += p1 + p2 + halo(512, G - 220, 270)
p3, _ = fam(430, G, YSIS, 0.56, arms=("wave", "down"))
p4, _ = fam(594, G, YBRO, 0.56, arms=("down", "wave"))
b += p3 + p4 + star(420, G - 520, 20) + star(612, G - 530, 16)
out["FamilyAndPeople/Grand children"] = svg("sand", b)

# Siblings, relative to faded ME
def sib_scene(targets, me_x, bg, arrows=None):
    b = ""
    for (tx, preset, s, arms) in targets:
        b += halo(tx, G - 300 * s, int(290 * s + 50))
    pm, _ = fam(me_x, G, ME, TEEN, faded=True, arms=("down", "down"))
    b += pm
    for (tx, preset, s, arms) in targets:
        pp, _ = fam(tx, G, preset, s, arms=arms)
        b += pp
    if arrows:
        b += arrows
    return b


# me height ~ 0.74*(615*1.1) ~ 500 -> top y ~ 360
out["FamilyAndPeople/Older brother"] = svg("sage", sib_scene([(420, OBRO, 0.95, ("down", "wave"))], 690, "sage",
                                                             arrow_v(830, 330, 170)))
out["FamilyAndPeople/Older sister"] = svg("rose", sib_scene([(420, OSIS, 0.93, ("down", "wave"))], 690, "rose",
                                                            arrow_v(830, 330, 170)))
out["FamilyAndPeople/Older sibling"] = svg("sand", sib_scene([(300, OBRO, 0.93, ("down", "wave")), (530, OSIS, 0.9, ("wave", "down"))], 780, "sand",
                                                             arrow_v(890, 330, 170)))
out["FamilyAndPeople/Younger brother"] = svg("sky", sib_scene([(640, YBRO, KID, ("wave", "down"))], 400, "sky",
                                                              arrow_v(820, 400, 560)))
out["FamilyAndPeople/Younger sister"] = svg("rose", sib_scene([(640, YSIS, KID, ("wave", "down"))], 400, "rose",
                                                              arrow_v(820, 400, 560)))
out["FamilyAndPeople/Younger sibling"] = svg("sand", sib_scene([(560, YBRO, 0.54, ("wave", "down")), (740, YSIS, 0.54, ("down", "wave"))], 320, "sand",
                                                               arrow_v(880, 380, 540)))

# Aunts/uncles
UNC_O = P(hair="neat", hc="#6b5a50", top="#7fa05a", bottom="pants", bc="#5c3d2e", torso="collar", glasses=True, mustache=True, skin=SKIN2)
AUN_O = P(hair="bun", hc="#4a3a32", top="#4f9a9a", bottom="longskirt", bc="#5c3d2e", torso="mandarin", earrings=True, sleeve="long")
UNC_Y = P(hair="spiky", top="#d9825b", bottom="pants", bc="#5d82a8", torso="tshirt")
AUN_Y = P(hair="bob", top="#7fa05a", bottom="skirt", bc="#5c3d2e", torso="tshirt", skin=SKIN2)

for key, tgt, bg in (("Older brother of mom_dad", UNC_O, "sage"), ("Older sister of mom_dad", AUN_O, "sand")):
    b = halo(250, G - 330, 330)
    p1, _ = fam(520, G, MOM, 0.9, faded=True, arms=("down", "outlow"))
    p2, _ = fam(760, G, DAD, 0.92, faded=True, arms=("outlow", "down"))
    p3, _ = fam(250, G, tgt, 1.05, arms=("down", "wave"))
    b += p1 + p2 + p3 + arrow_v(105, 330, 175)
    out[f"FamilyAndPeople/{key}"] = svg(bg, b)

for key, par, bg in (("Younger brother_Sister of Mom", MOM, "rose"), ("Younger brother_Sister of dad", DAD, "sky")):
    b = halo(560, G - 250, 260) + halo(770, G - 250, 260)
    p1, _ = fam(270, G, par, 1.02, faded=True, arms=("down", "outlow"))
    p2, _ = fam(565, G, UNC_Y, 0.8, arms=("down", "wave"))
    p3, _ = fam(775, G, AUN_Y, 0.78, arms=("wave", "down"))
    b += p1 + p2 + p3 + arrow_v(670, 150, 290)
    out[f"FamilyAndPeople/{key}"] = svg(bg, b)

# In-laws: faded speaker + faded spouse, spouse's parents highlighted
for key, speaker, spouse, bg in (("In_law_for female_", MOM, DAD, "rose"), ("In_law_for male_", DAD, MOM, "sky")):
    b = halo(640, G - 280, 300) + halo(840, G - 280, 290)
    p1, _ = fam(170, G, speaker, 0.8, faded=True, arms=("down", "outlow"))
    p2, _ = fam(360, G, spouse, 0.82, faded=True, arms=("outlow", "outlow"))
    p3, _ = fam(630, G, GMA, 0.8, arms=("outlow", "down") if False else ("down", "wave"))
    p4, _ = fam(850, G, GPA, 0.82, arms=("down", "down"))
    b += p1 + p2 + p3 + p4 + ring(265, 360, 0.8)
    b += f'<path d="M360 {G - 560} Q500 {G - 680} 740 {G - 600}" fill="none" stroke="{OL}" stroke-width="12" stroke-dasharray="4 26" stroke-linecap="round" opacity="0.6"/>'
    out[f"FamilyAndPeople/{key}"] = svg(bg, b)

# Kid of someone: faded adult neighbor lady holding hand of highlighted kid
b = ""
NEIGH = P(hair="bob", hc="#5c3d2e", top="#5f8f4e", bottom="longskirt", bc="#9a6a45", torso="vneck", skin=SKIN3, earrings=True)
p1, _ = fam(380, G, NEIGH, 1.0, faded=True, arms=("down", "outlow"))
b += p1 + halo(640, G - 200, 230)
p2, _ = fam(640, G, P(hair="spiky", hc=HAIR, top="#c8574b", bottom="shorts", bc="#5d82a8", skin=SKIN3), KID, arms=("outlow", "wave"))
b += p2 + sparkles(640, G - 450, 100)
b += f'<path d="M560 {G - 250} Q540 {G - 250} 530 {G - 240}" fill="none" stroke="{OL}" stroke-width="0"/>'
out["FamilyAndPeople/Kid of someone"] = svg("sage", b)

# Friend: two friends side by side high-five
b = ""
F1 = P(hair="spiky", top="#e0b04f", bottom="pants", bc="#5d82a8", torso="tshirt")
F2 = P(hair="short", top="#4f9a9a", bottom="pants", bc="#5c3d2e", torso="tshirt", skin=SKIN3)
p1, _ = person(360, G, 0.95, **F1, arms=("down", "up"))
p2, _ = person(664, G, 0.95, **F2, arms=("up", "down"))
b += p1 + p2
b += star(512, 300, 34) + f'<path d="M470 250 L440 215 M512 236 L512 196 M554 250 L584 215" stroke="{OL}" stroke-width="10" stroke-linecap="round"/>'
out["FamilyAndPeople/Friend"] = svg("sand", b)


def school(cx, by, w=420, h=260):
    x0 = cx - w / 2
    s = f'<ellipse cx="{cx}" cy="{by + 6}" rx="{w / 2 + 30}" ry="24" fill="{OL}" opacity="0.12"/>'
    s += f'<rect x="{x0}" y="{by - h}" width="{w}" height="{h}" fill="#f6ecd8" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    s += f'<path d="M{x0 - 30} {by - h} L{cx} {by - h - 110} L{x0 + w + 30} {by - h} Z" fill="#c8574b" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    s += f'<path d="M{cx - 60} {by - h - 20} L{cx} {by - h - 90} L{cx + 60} {by - h - 20}" fill="none" stroke="#e0b04f" stroke-width="12" stroke-linecap="round" stroke-linejoin="round"/>'
    for i in range(4):
        wx = x0 + 30 + i * (w - 60) / 4 + 8
        s += f'<rect x="{wx:.0f}" y="{by - h + 40}" width="{(w - 60) / 4 - 16:.0f}" height="60" rx="6" fill="#9fc6d6" stroke="{OL}" stroke-width="8"/>'
    s += f'<rect x="{cx - 40}" y="{by - 110}" width="80" height="110" fill="#9a6a45" stroke="{OL}" stroke-width="10"/>'
    s += f'<path d="M{cx + w / 2 - 50} {by - h - 30} L{cx + w / 2 - 50} {by - h - 130}" stroke="{OL}" stroke-width="8"/>'
    s += f'<path d="M{cx + w / 2 - 50} {by - h - 130} L{cx + w / 2 + 20} {by - h - 115} L{cx + w / 2 - 50} {by - h - 100} Z" fill="#5d82a8" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>'
    return s


# Friend at/from (place): building with pin, friend in front waving to faded me
b = school(560, 640, 440, 250) + pin(560, 250, 0.9)
p1, _ = person(620, G, 0.72, **F2, arms=("down", "wave"), head_scale=1.08)
p2, _ = fam(300, G, ME, 0.66, faded=True, arms=("wave", "down"))
b += p2 + halo(620, G - 230, 230) + p1
out["FamilyAndPeople/Friend at_from_place"] = svg("sage", b)

# People at/from: group of 3 people in front of building with pin
b = school(512, 600, 520, 240) + pin(512, 230, 0.85)
pa, _ = person(300, G, 0.64, hair="short", top="#5d82a8", bottom="pants", bc="#5c3d2e", torso="collar", skin=SKIN2, arms=("down", "wave"), head_scale=1.08)
pb, _ = person(512, G + 10, 0.66, hair="long", top="#d9825b", bottom="skirt", bc="#8a5a86", torso="vneck", arms=("down", "down"), head_scale=1.08)
pc, _ = person(724, G, 0.64, hair="bun", hc=HAIR, top="#7fa05a", bottom="pants", bc="#5c3d2e", torso="tshirt", skin=SKIN3, arms=("wave", "down"), head_scale=1.08)
b += pa + pc + pb
out["FamilyAndPeople/People at_"] = svg("sand", b)


def house(cx, by, w, body, roof):
    x0 = cx - w / 2
    h = w * 0.7
    s = f'<rect x="{x0}" y="{by - h}" width="{w}" height="{h}" fill="{body}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    s += f'<path d="M{x0 - 30} {by - h + 10} L{cx} {by - h - w * 0.45} L{x0 + w + 30} {by - h + 10} Z" fill="{roof}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    s += f'<rect x="{cx - w * 0.3}" y="{by - h + 40}" width="{w * 0.25}" height="{w * 0.22}" rx="6" fill="#9fc6d6" stroke="{OL}" stroke-width="8"/>'
    s += f'<rect x="{cx + w * 0.08}" y="{by - w * 0.4}" width="{w * 0.22}" height="{w * 0.4}" fill="#9a6a45" stroke="{OL}" stroke-width="9"/>'
    return s


# Neighbor: two houses with fence, person waving from next house
b = f'<ellipse cx="512" cy="{G - 180}" rx="420" ry="30" fill="{OL}" opacity="0.1"/>'
b += house(265, G - 190, 260, "#f6ecd8", "#c8574b") + house(760, G - 190, 260, "#f5e3b8", "#5d82a8")
fence = ""
for i in range(7):
    fx = 420 + i * 30
    fence += f'<path d="M{fx} {G - 180} L{fx} {G - 290} L{fx + 12} {G - 305} L{fx + 24} {G - 290} L{fx + 24} {G - 180} Z" fill="#e8d3ac" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/>'
fence += f'<path d="M410 {G - 260} L620 {G - 260}" stroke="{OL}" stroke-width="8"/>'
b += fence
p1, _ = fam(330, G, ME, 0.6, faded=True, arms=("down", "wave"))
b += p1 + halo(700, G - 220, 220)
p2, _ = person(700, G, 0.62, **NEIGH, arms=("wave", "down"), head_scale=1.1)
b += p2
out["FamilyAndPeople/Neighbor"] = svg("sage", b)

# ---------------- Gender ----------------
MAN = P(hair="neat", top="#5d82a8", bottom="pants", bc="#5c3d2e", torso="collar", skin=SKIN2)
WOMAN = P(hair="long", top="#c8574b", bottom="longskirt", bc="#8a5a86", torso="vneck", earrings=True)
for key, pr, bg in (("_Polite male_", MAN, "sky"), ("_Polite female_", WOMAN, "rose")):
    p, _ = person(400, G + 10, 0.98, **pr, arms=("wai", "wai"))
    b = p + bubble(720, 290, 340, 310, (560, 470), wai_icon(720, 280, 1.55))
    out[f"Gender/{key}"] = svg(bg, b)

for key, pr, bg in (("_Man I_", MAN, "sky"), ("_Woman I_", WOMAN, "rose")):
    p, h = person(420, G + 10, 0.98, **pr, arms=("down", "self"), mouth="open")
    # bubble with mini self icon (head + body)
    icon = person(730, 410, 0.36, **pr, arms=("down", "self"), shadow=False)[0]
    b = p + bubble(730, 285, 300, 320, (580, 480), icon)
    # pointing finger toward chest
    hx, hy = h["r"]
    b += f'<path d="M{hx - 10} {hy} L{hx - 50} {hy + 4}" stroke="{OL}" stroke-width="26" stroke-linecap="round"/><path d="M{hx - 10} {hy} L{hx - 50} {hy + 4}" stroke="{pr["skin"] if "skin" in pr else SKIN}" stroke-width="12" stroke-linecap="round"/>'
    b += f'<circle cx="{hx}" cy="{hy}" r="{23 * 0.98:.0f}" fill="{pr.get("skin", SKIN)}" stroke="{OL}" stroke-width="10"/>'
    out[f"Gender/{key}"] = svg(bg, b)

# ---------------- Ocupation ----------------
def stethoscope(s=1.0):
    return (f'<path d="M-40 -388 Q-70 -330 -40 -270 Q0 -230 40 -270 Q70 -330 40 -388" fill="none" stroke="{OL}" stroke-width="18" stroke-linecap="round"/>'
            f'<path d="M-40 -388 Q-70 -330 -40 -270 Q0 -230 40 -270 Q70 -330 40 -388" fill="none" stroke="#5d82a8" stroke-width="8" stroke-linecap="round"/>'
            f'<path d="M0 -245 L0 -200" stroke="{OL}" stroke-width="14"/>'
            f'<circle cx="0" cy="-190" r="20" fill="#c9c9c9" stroke="{OL}" stroke-width="9"/>')


# Doctor
p, h = person(512, G + 10, 1.05, hair="neat", top="#7fa05a", torso="coat", bottom="pants", bc="#5c3d2e", skin=SKIN2,
              arms=("down", "hold"), extra_top=stethoscope(), glasses=False)
hx, hy = h["r"]
clip = (f'<g transform="translate({hx - 30} {hy - 20}) rotate(-8)"><rect x="-10" y="-100" width="130" height="170" rx="12" fill="#9a6a45" stroke="{OL}" stroke-width="12"/>'
        f'<rect x="8" y="-80" width="94" height="130" fill="#fbf8f2" stroke="{OL}" stroke-width="8"/><rect x="30" y="-112" width="50" height="26" rx="6" fill="#c9c9c9" stroke="{OL}" stroke-width="8"/>'
        f'<path d="M25 -50 L85 -50 M25 -20 L85 -20 M25 10 L70 10" stroke="{OL}" stroke-width="7" opacity="0.5"/>'
        f'<path d="M55 -65 L55 -35 M40 -50 L70 -50" stroke="#c8574b" stroke-width="0"/></g>')
cross = f'<path d="M-18 -60 L18 -60 L18 -18 L60 -18 L60 18 L18 18 L18 60 L-18 60 L-18 18 L-60 18 L-60 -18 L-18 -18 Z" fill="#c8574b" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
b = f'<g transform="translate(830 210) scale(0.9)"><circle r="95" fill="#fbf8f2" stroke="{OL}" stroke-width="12"/>{cross}</g>' + p + clip
out["Ocupation/Doctor"] = svg("sky", b)

# Dentist: coat, mask under chin, holding big tooth & mirror
tooth = (f'<path d="M-70 -60 Q-80 -110 -30 -110 Q0 -95 30 -110 Q80 -110 70 -60 Q62 -10 50 50 Q40 90 22 60 Q10 20 0 20 Q-10 20 -22 60 Q-40 90 -50 50 Q-62 -10 -70 -60 Z" '
         f'fill="#fbf8f2" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/><path d="M-44 -80 Q-46 -50 -36 -30" fill="none" stroke="#9fc6d6" stroke-width="12" stroke-linecap="round"/>'
         f'<circle cx="-22" cy="-50" r="7" fill="{OL}"/><circle cx="22" cy="-50" r="7" fill="{OL}"/><path d="M-14 -28 Q0 -16 14 -28" fill="none" stroke="{OL}" stroke-width="6" stroke-linecap="round"/>'
         f'<circle cx="-40" cy="-30" r="10" fill="{CHEEK}" opacity="0.6"/><circle cx="40" cy="-30" r="10" fill="{CHEEK}" opacity="0.6"/>')
mask = f'<path d="M-70 60 Q0 40 70 60 L64 100 Q0 120 -64 100 Z" fill="#9fd0c8" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/><path d="M-70 62 L-98 20 M70 62 L98 20" stroke="{OL}" stroke-width="5"/>'
p, h = person(420, G + 10, 1.02, hair="bob", top="#9fd0c8", torso="coat", bottom="pants", bc="#5d82a8", arms=("down", "up"))
# mask on chin: put after person via separate transform
b = p
hx, hy = h["r"]
b = b.replace('</g></g>', '</g></g>', 1)
maskg = f'<g transform="translate(420 {G + 10}) scale(1.02) translate(0 -500)">{mask}</g>'
mirror = (f'<path d="M{hx} {hy} L{hx + 90} {hy - 140}" stroke="{OL}" stroke-width="22" stroke-linecap="round"/><path d="M{hx} {hy} L{hx + 90} {hy - 140}" stroke="#c9c9c9" stroke-width="10" stroke-linecap="round"/>'
          f'<circle cx="{hx + 100}" cy="{hy - 160}" r="30" fill="#cfe6ee" stroke="{OL}" stroke-width="10"/>')
b = mirror + b + maskg + f'<circle cx="{hx}" cy="{hy}" r="23" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>'
b += f'<g transform="translate(740 620) scale(1.25)">{tooth}</g>' + sparkles(760, 460, 90)
out["Ocupation/Dentist"] = svg("sky", b)

# Pharmacist: shelf of bottles behind, holding pill bottle
shelf = ""
for row, yy in enumerate((300, 480)):
    shelf += f'<rect x="150" y="{yy + 110}" width="724" height="22" fill="#9a6a45" stroke="{OL}" stroke-width="10"/>'
    cols = ["#c8574b", "#5d82a8", "#7fa05a", "#e0b04f", "#8a5a86", "#d9825b", "#4f9a9a", "#c8574b"]
    for i in range(8):
        bx = 175 + i * 88 + (row * 30 % 60)
        if 330 < bx < 640:
            continue
        c = cols[(i + row * 3) % 8]
        if i % 2:
            shelf += f'<rect x="{bx}" y="{yy + 30}" width="60" height="80" rx="10" fill="{c}" stroke="{OL}" stroke-width="8"/><rect x="{bx + 6}" y="{yy + 12}" width="48" height="22" rx="4" fill="#f6ecd8" stroke="{OL}" stroke-width="7"/><rect x="{bx + 10}" y="{yy + 55}" width="40" height="30" fill="#fbf8f2" opacity="0.8"/>'
        else:
            shelf += f'<rect x="{bx}" y="{yy + 50}" width="66" height="60" rx="6" fill="#fbf8f2" stroke="{OL}" stroke-width="8"/><path d="M{bx + 33} {yy + 62} L{bx + 33} {yy + 98} M{bx + 15} {yy + 80} L{bx + 51} {yy + 80}" stroke="{c}" stroke-width="10"/>'
p, h = person(512, G + 10, 1.0, hair="ponytail", top="#4f9a9a", torso="coat", bottom="pants", bc="#5c3d2e", arms=("down", "out"), skin=SKIN)
hx, hy = h["r"]
bottle = (f'<g transform="translate({hx + 60} {hy - 30})"><rect x="-50" y="-60" width="100" height="130" rx="16" fill="#d9825b" stroke="{OL}" stroke-width="12"/>'
          f'<rect x="-58" y="-100" width="116" height="46" rx="8" fill="#fbf8f2" stroke="{OL}" stroke-width="12"/>'
          f'<rect x="-36" y="-20" width="72" height="60" rx="6" fill="#fbf8f2" stroke="{OL}" stroke-width="8"/>'
          f'<g transform="translate(0 10) rotate(-35)"><rect x="-26" y="-12" width="52" height="24" rx="12" fill="#c8574b" stroke="{OL}" stroke-width="6"/><path d="M0 -12 L0 12" stroke="{OL}" stroke-width="5"/><rect x="0" y="-12" width="26" height="24" rx="12" fill="#f6ecd8" opacity="0.9"/></g>'
          f'<path d="M-34 -40 L-34 50" stroke="#fff" stroke-width="10" opacity="0.3" stroke-linecap="round"/></g>')
b = shelf + p + f'<circle cx="{hx}" cy="{hy}" r="23" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>' + bottle
b = b.replace(f'<circle cx="{hx}" cy="{hy}" r="23" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>' + bottle, bottle + f'<circle cx="{hx + 10}" cy="{hy + 5}" r="23" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>')
out["Ocupation/Pharmacist"] = svg("sage", b)

# Police (Thai brown uniform), whistle, badge, salute-ish wave
pol_top = (f'<path d="M-96 -325 L-60 -330" stroke="{OL}" stroke-width="0"/>'
           f'<path d="M-70 -300 L-30 -300 L-30 -270 L-70 -270 Z" fill="#6b4a33" stroke="{OL}" stroke-width="7"/>'
           f'<path d="M50 -320 L64 -300 L50 -270 L36 -300 Z" fill="#e0b04f" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>'
           f'<rect x="-86" y="-222" width="172" height="26" fill="#2e211b"/><rect x="-16" y="-226" width="32" height="32" rx="4" fill="#e0b04f" stroke="{OL}" stroke-width="6"/>'
           f'<path d="M-86 -370 L-40 -380 M86 -370 L40 -380" stroke="#3b2a22" stroke-width="14" stroke-linecap="round"/>')
p, h = person(512, G + 10, 1.05, hair="short", hat_kind="police", top="#8a6446", torso="collar", collar="#8a6446", bottom="pants", bc="#5c4535",
              skin=SKIN2, arms=("down", "wave"), extra_top=pol_top, shoes="#2e211b")
hx, hy = h["r"]
whistle = f'<g transform="translate({hx + 6} {hy - 36}) rotate(-20)"><rect x="-30" y="-16" width="46" height="32" rx="14" fill="#c9c9c9" stroke="{OL}" stroke-width="9"/><rect x="10" y="-10" width="30" height="16" fill="#c9c9c9" stroke="{OL}" stroke-width="8"/></g>'
b = p + whistle + f'<path d="M{hx + 60} {hy - 90} L{hx + 95} {hy - 115} M{hx + 66} {hy - 55} L{hx + 108} {hy - 60}" stroke="{OL}" stroke-width="10" stroke-linecap="round"/>'
out["Ocupation/Police"] = svg("sand", b)

# Security guard: navy uniform, cap, walkie-talkie, baton, next to a gate/booth
g_top = (f'<path d="M-72 -318 L-34 -318 L-34 -282 Q-53 -268 -72 -282 Z" fill="#e0b04f" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>'
         f'<rect x="-86" y="-222" width="172" height="26" fill="#2e211b"/><rect x="-16" y="-226" width="32" height="32" rx="4" fill="#c9c9c9" stroke="{OL}" stroke-width="6"/>'
         f'<path d="M-86 -370 L-40 -380 M86 -370 L40 -380" stroke="#e0b04f" stroke-width="12" stroke-linecap="round"/>')
p, h = person(560, G + 10, 1.02, hair="short", hat_kind="guard", top="#4a5d7e", torso="collar", collar="#4a5d7e", bottom="pants", bc="#2f3b52",
              skin=SKIN3, arms=("hip", "chest"), extra_top=g_top, shoes="#2e211b",
              extra_back=f'<path d="M-100 -210 L-130 -90" stroke="{OL}" stroke-width="30" stroke-linecap="round"/><path d="M-100 -210 L-130 -90" stroke="#3b3b3b" stroke-width="14" stroke-linecap="round"/>')
hx, hy = h["r"]
walkie = (f'<g transform="translate({hx} {hy - 40})"><path d="M18 -40 L18 -90" stroke="{OL}" stroke-width="12" stroke-linecap="round"/>'
          f'<rect x="-26" y="-50" width="56" height="96" rx="10" fill="#3b3b3b" stroke="{OL}" stroke-width="10"/><rect x="-14" y="-36" width="32" height="24" rx="4" fill="#9fd0c8"/>'
          f'<path d="M-12 4 L16 4 M-12 18 L16 18" stroke="#888" stroke-width="5"/></g>')
barrier = (f'<rect x="150" y="{G - 360}" width="60" height="360" rx="8" fill="#f6ecd8" stroke="{OL}" stroke-width="12"/>'
           f'<path d="M200 {G - 300} L470 {G - 300}" stroke="{OL}" stroke-width="44" stroke-linecap="round"/><path d="M200 {G - 300} L470 {G - 300}" stroke="#fbf8f2" stroke-width="24" stroke-linecap="round"/>'
           f'<path d="M250 {G - 300} L290 {G - 300} M340 {G - 300} L380 {G - 300} M430 {G - 300} L462 {G - 300}" stroke="#c8574b" stroke-width="24"/>'
           f'<ellipse cx="180" cy="{G + 4}" rx="60" ry="14" fill="{OL}" opacity="0.15"/>')
b = barrier + p + walkie + f'<circle cx="{hx}" cy="{hy}" r="23" fill="{SKIN3}" stroke="{OL}" stroke-width="10"/>'
out["Ocupation/Security guard"] = svg("sky", b)

# Teacher: blackboard with shapes, pointer
board = (f'<rect x="130" y="170" width="520" height="340" rx="14" fill="#9a6a45" stroke="{OL}" stroke-width="14"/>'
         f'<rect x="158" y="198" width="464" height="284" fill="#4f7a5a" stroke="{OL}" stroke-width="8"/>'
         f'<path d="M200 300 L250 220 L300 300 Z" fill="none" stroke="#f6ecd8" stroke-width="10" stroke-linejoin="round"/>'
         f'<circle cx="380" cy="262" r="42" fill="none" stroke="#f6ecd8" stroke-width="10"/>'
         f'<rect x="460" y="222" width="80" height="80" fill="none" stroke="#f6ecd8" stroke-width="10"/>'
         f'<path d="M200 380 Q230 350 260 380 T320 380 T380 380 M200 430 Q240 400 280 430 T360 430" fill="none" stroke="#f6ecd8" stroke-width="8" stroke-linecap="round" opacity="0.8"/>'
         f'<rect x="200" y="496" width="80" height="16" rx="4" fill="#fbf8f2" stroke="{OL}" stroke-width="6"/>')
p, h = person(700, G + 10, 1.0, hair="bun", hc=HAIR, top="#f6ecd8", torso="collar", collar="#fbf8f2", bottom="skirt", bc="#5c3d2e",
              glasses=True, arms=("up", "hold"))
lx, ly = h["l"]
pointer = f'<path d="M{lx} {ly} L{lx - 170} {ly - 20}" stroke="{OL}" stroke-width="22" stroke-linecap="round"/><path d="M{lx} {ly} L{lx - 170} {ly - 20}" stroke="#c8574b" stroke-width="10" stroke-linecap="round"/>'
hx, hy = h["r"]
book = (f'<g transform="translate({hx - 10} {hy + 5}) rotate(-10)"><rect x="-60" y="-50" width="110" height="80" rx="6" fill="#5d82a8" stroke="{OL}" stroke-width="10"/>'
        f'<path d="M-50 -38 L40 -38" stroke="#fbf8f2" stroke-width="8"/></g>')
b = board + pointer + p + f'<circle cx="{lx}" cy="{ly}" r="23" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>' + book + f'<circle cx="{hx}" cy="{hy}" r="23" fill="{SKIN}" stroke="{OL}" stroke-width="10"/>'
out["Ocupation/Teacher"] = svg("sage", b)

# Maid / Housewife: apron, broom, bucket
apron = (f'<path d="M-50 -350 L50 -350 L54 -250 L82 -236 L88 -120 Q0 -108 -88 -120 L-82 -236 L-54 -250 Z" fill="#fbf8f2" stroke="{OL}" stroke-width="10" stroke-linejoin="round"/>'
         f'<path d="M-50 -350 L-80 -390 M50 -350 L80 -390" stroke="{OL}" stroke-width="8"/>'
         f'<rect x="-40" y="-200" width="80" height="52" rx="8" fill="none" stroke="{OL}" stroke-width="7"/>'
         f'<path d="M-84 -236 Q0 -226 84 -236" fill="none" stroke="#e8a0a8" stroke-width="10"/>')
p, h = person(470, G + 10, 1.02, hair="bob", top="#8a5a86", torso="tshirt", bottom="longskirt", bc="#8a5a86", arms=("chest", "out"),
              extra_top=apron, skin=SKIN2)
hx, hy = h["r"]
broom = (f'<path d="M{hx} {hy - 140} L{hx + 10} {G - 90}" stroke="{OL}" stroke-width="26" stroke-linecap="round"/><path d="M{hx} {hy - 140} L{hx + 10} {G - 90}" stroke="#9a6a45" stroke-width="12" stroke-linecap="round"/>'
         f'<path d="M{hx - 20} {G - 110} L{hx + 36} {G - 110} L{hx + 80} {G} L{hx - 60} {G} Z" fill="#e0b04f" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
         f'<path d="M{hx - 30} {G - 10} L{hx - 10} {G - 90} M{hx + 8} {G - 10} L{hx + 8} {G - 90} M{hx + 46} {G - 10} L{hx + 26} {G - 90}" stroke="{OL}" stroke-width="7" opacity="0.5"/>')
bucket = (f'<ellipse cx="230" cy="{G + 4}" rx="80" ry="16" fill="{OL}" opacity="0.15"/><path d="M160 {G - 150} L300 {G - 150} L285 {G} L175 {G} Z" fill="#5d82a8" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
          f'<ellipse cx="230" cy="{G - 150}" rx="70" ry="16" fill="#9fc6d6" stroke="{OL}" stroke-width="10"/><path d="M165 {G - 150} Q230 {G - 250} 295 {G - 150}" fill="none" stroke="{OL}" stroke-width="9"/>'
          f'<circle cx="210" cy="{G - 185}" r="16" fill="#fbf8f2" stroke="{OL}" stroke-width="6"/><circle cx="240" cy="{G - 200}" r="12" fill="#fbf8f2" stroke="{OL}" stroke-width="6"/>')
b = bucket + p + broom + f'<circle cx="{hx}" cy="{hy}" r="23" fill="{SKIN2}" stroke="{OL}" stroke-width="10"/>' + sparkles(760, 330, 70)
out["Ocupation/Maid_Housewife"] = svg("beige", b)

# Masseuse: masseuse behind massage bed, client lying face down, hands pressing the back
MSKIN = SKIN2
p, h = person(512, G - 30, 0.95, hair="bun", hc=HAIR, top="#5f8f4e", torso="mandarin", sleeve="short", bottom="pants", bc="#5c3d2e",
              arms=(None, None), legs_hidden=True, shadow=False, skin=MSKIN)
bed = (f'<ellipse cx="512" cy="{G + 4}" rx="360" ry="26" fill="{OL}" opacity="0.15"/>'
       f'<rect x="170" y="{G - 250}" width="684" height="70" rx="20" fill="#9a6a45" stroke="{OL}" stroke-width="12"/>'
       f'<path d="M200 {G - 230} L820 {G - 230}" stroke="#fff" stroke-width="10" opacity="0.2" stroke-linecap="round"/>'
       f'<rect x="210" y="{G - 190}" width="36" height="190" fill="#7d5337" stroke="{OL}" stroke-width="10"/><rect x="778" y="{G - 190}" width="36" height="190" fill="#7d5337" stroke="{OL}" stroke-width="10"/>'
       f'<rect x="170" y="{G - 285}" width="684" height="44" rx="18" fill="#f6ecd8" stroke="{OL}" stroke-width="10"/>')
client = (f'<path d="M300 {G - 285} Q300 {G - 350} 360 {G - 360} Q470 {G - 372} 600 {G - 350} L610 {G - 285} Z" fill="{SKIN}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
          f'<path d="M560 {G - 285} L570 {G - 352} Q700 {G - 356} 780 {G - 330} Q830 {G - 312} 830 {G - 285} Z" fill="#e8a0a8" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
          f'<path d="M620 {G - 350} L630 {G - 290} M690 {G - 346} L696 {G - 290} M750 {G - 334} L752 {G - 290}" stroke="#fbf8f2" stroke-width="10" opacity="0.6"/>'
          f'<path d="M380 {G - 350} Q460 {G - 330} 540 {G - 346}" fill="none" stroke="{OL}" stroke-width="8" opacity="0.35" stroke-linecap="round"/>'
          f'<circle cx="262" cy="{G - 330}" r="58" fill="{HAIR}" stroke="{OL}" stroke-width="12"/><path d="M226 {G - 368} Q250 {G - 385} 280 {G - 380}" fill="none" stroke="#fff" stroke-width="9" opacity="0.25" stroke-linecap="round"/>')
# arms in person-local coords (s=0.95, feet at G-30)
armL = [(-86, -352), (-150, -300), (-70, -335)]
armR = [(86, -352), (150, -300), (70, -335)]
arms = (f'<g transform="translate(512 {G - 30}) scale(0.95)">' + limb(armL, MSKIN, 34, sleeve="#5f8f4e", sleeve_frac=0.3) + limb(armR, MSKIN, 34, sleeve="#5f8f4e", sleeve_frac=0.3)
        + f'<ellipse cx="-66" cy="-335" rx="30" ry="20" fill="{MSKIN}" stroke="{OL}" stroke-width="10"/><ellipse cx="66" cy="-335" rx="30" ry="20" fill="{MSKIN}" stroke="{OL}" stroke-width="10"/></g>')
orchid = (f'<g transform="translate(800 280)"><path d="M0 0 C-50 -40 -60 20 0 10 C60 20 50 -40 0 0 Z" fill="#b886c4" stroke="{OL}" stroke-width="8"/>'
          f'<path d="M0 0 C-30 -70 30 -70 0 0 Z M0 0 C-20 50 20 50 0 0 Z" fill="#d7aee0" stroke="{OL}" stroke-width="8"/><circle cx="0" cy="2" r="10" fill="#e0b04f" stroke="{OL}" stroke-width="5"/></g>')
b = p + bed + client + arms + orchid + f'<path d="M420 {G - 400} Q404 {G - 420} 420 {G - 440} M604 {G - 400} Q620 {G - 420} 604 {G - 440}" fill="none" stroke="{OL}" stroke-width="9" opacity="0.4" stroke-linecap="round"/>'
out["Ocupation/Masseuse"] = svg("sage", b)

for k, v in out.items():
    path = os.path.join(ROOT, "svg", k + ".svg")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(v)
print(len(out), "written")

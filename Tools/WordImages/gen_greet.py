"""Generator for Greeting + Introduce illustrations."""
import os, math

ROOT = os.path.dirname(os.path.abspath(__file__))
O = "#2e211b"
BG = {
    "beige": ("#f3e6d6", "#c9ab93"), "rose": ("#f1e0dc", "#bf9a98"), "sage": ("#e7ecdc", "#a9b595"),
    "sky": ("#e3ecf1", "#9fb3c1"), "sand": ("#f5ead0", "#cfb27e"),
}
CREAM = "#f6ecd8"
BUB = "#fffaf0"
RED = "#c8574b"; TERRA = "#d9825b"; MUST = "#e0b04f"; OLIVE = "#7fa05a"; LEAF = "#5f8f4e"
TEAL = "#4f9a9a"; BLUE = "#5d82a8"; PLUM = "#8a5a86"; WOOD = "#9a6a45"; DBROWN = "#5c3d2e"; HAIR = "#3b2a22"
CHEEK = "#e8907f"

S = 'stroke="%s" stroke-linejoin="round" stroke-linecap="round"' % O


def svg(bgname, body):
    l, d = BG[bgname]
    return ('<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">\n'
            '<defs><radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="' + l +
            '"/><stop offset="100%" stop-color="' + d + '"/></radialGradient></defs>\n'
            '<rect width="1024" height="1024" fill="url(#bg)"/>\n' + body + '\n</svg>\n')


def g(x, y, s=1, inner="", rot=0):
    r = f" rotate({rot})" if rot else ""
    return f'<g transform="translate({x},{y}) scale({s}){r}">{inner}</g>\n'


import re


def attrs(w, extra):
    d = {"stroke": O, "stroke-linejoin": "round", "stroke-linecap": "round", "stroke-width": str(w)}
    if not w:
        d = {}
    for k, v in re.findall(r'([\w-]+)="([^"]*)"', extra or ""):
        d[k] = v
    if "stroke-width" in d and "stroke" not in d:
        d["stroke"] = O
        d.setdefault("stroke-linecap", "round"); d.setdefault("stroke-linejoin", "round")
    return " ".join(f'{k}="{v}"' for k, v in d.items())


def P(d, fill="none", w=12, extra=""):
    return f'<path d="{d}" fill="{fill}" {attrs(w, extra)}/>'


def C(cx, cy, r, fill, w=12, extra=""):
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {attrs(w, extra)}/>'


def E(cx, cy, rx, ry, fill, w=12, extra=""):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {attrs(w, extra)}/>'


def R(x, y, w_, h, fill, rx=0, w=12, extra=""):
    return f'<rect x="{x}" y="{y}" width="{w_}" height="{h}" rx="{rx}" fill="{fill}" {attrs(w, extra)}/>'


def shadow(cx, cy, rx, ry=None):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry or rx*0.18}" fill="{O}" opacity="0.15"/>'


def pl(pts):
    return "M" + " L".join(f"{x} {y}" for x, y in pts)


def thick(pts, color, w=30, ow=46):
    d = pl(pts)
    return (f'<path d="{d}" fill="none" stroke="{O}" stroke-width="{ow}" stroke-linecap="round" stroke-linejoin="round"/>'
            f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"/>')


def qmark(x, y, size=160, color=RED):
    return (f'<text x="{x}" y="{y}" font-family="Arial, Helvetica, sans-serif" font-weight="bold" font-size="{size}" '
            f'text-anchor="middle" fill="{color}" stroke="{O}" stroke-width="{size*0.09:.1f}" stroke-linejoin="round" '
            f'paint-order="stroke">?</text>')


# ---------------------------------------------------------------- people
WOMAN = dict(skin="#f1c9a5", shirt=TERRA, low=PLUM)
MAN = dict(skin="#e0a882", shirt=BLUE, low=DBROWN)

ARMS = {  # side=+1 (viewer right). list of points from shoulder, hand type
    "wave": ([(92, 205), (175, 175), (190, 50)], "open"),
    "wave2": ([(92, 205), (190, 190), (230, 80)], "open"),
    "thumb": ([(92, 205), (185, 240), (180, 120)], "thumb"),
    "stop": ([(92, 205), (175, 250), (185, 120)], "open"),
    "palm": ([(92, 205), (165, 280), (255, 235)], "open"),
    "belly": ([(92, 205), (135, 300), (45, 285)], "open"),
    "flat": ([(92, 205), (160, 285), (235, 240)], "flat"),
    "shake": ([(92, 205), (160, 290), (250, 260)], "fist"),
    "point": ([(92, 205), (170, 250), (255, 170)], "point"),
    "pointback": ([(92, 205), (180, 250), (200, 115)], "pointup"),
    "hold": ([(92, 205), (130, 300), (60, 255)], "fist"),
    "chin": ([(92, 205), (140, 290), (40, 115)], "fist"),
    "wai": ([(92, 205), (112, 290), (24, 200)], None),
    "hip": ([(92, 205), (165, 265), (105, 320)], None),
}


def hand(x, y, kind, skin, side, ang=0):
    if kind == "open":
        o = ""
        for fx, fl in ((-15, 30), (-3, 36), (10, 34), (22, 26)):
            o += R(fx * side - 7, -12 - fl, 14, fl + 12, skin, 7, 8)
        o += E(-26 * side, 4, 9, 20, skin, 8, f'transform="rotate({-40*side} {-26*side} 4)"')
        o += E(0, 6, 28, 24, skin, 10)
        o += R(-19, -6, 38, 20, skin, 0, 0)
        return f'<g transform="translate({x},{y}) rotate({ang}) scale(1.25)">' + o + '</g>'
    if kind == "fist":
        return C(x, y, 26, skin, 10)
    if kind == "thumb":
        return (R(x - 11, y - 62, 22, 50, skin, 11, 9) + R(x - 30, y - 22, 60, 48, skin, 20, 10) +
                P(f"M{x-28} {y-2} L{x+22} {y-2} M{x-28} {y+12} L{x+22} {y+12}", w=5))
    if kind == "flat":
        return E(x, y, 38, 17, skin, 10)
    if kind == "point":
        a = -35 * side
        return (f'<g transform="translate({x},{y}) rotate({a if side>0 else 180+a})">' +
                R(0, -10, 58, 20, skin, 10, 8) + '</g>' + C(x, y, 25, skin, 10))
    if kind == "pointup":
        return R(x - 10, y - 60, 20, 50, skin, 10, 8) + C(x, y, 25, skin, 10)
    return ""


def arm(name, side, c):
    pts, hk = ARMS[name]
    pts = [(x * side, y) for x, y in pts]
    s = thick(pts[1:], c["skin"]) if len(pts) > 2 else ""
    out = f'<path d="{pl(pts)}" fill="none" stroke="{O}" stroke-width="46" stroke-linecap="round" stroke-linejoin="round"/>'
    out += f'<path d="{pl(pts[1:])}" fill="none" stroke="{c["skin"]}" stroke-width="30" stroke-linecap="round"/>'
    out += f'<path d="{pl(pts[:2])}" fill="none" stroke="{c["shirt"]}" stroke-width="32" stroke-linecap="round"/>'
    if hk:
        (x1, y1), (x2, y2) = pts[-2], pts[-1]
        ang = math.degrees(math.atan2(y2 - y1, x2 - x1)) + 90
        out += hand(x2, y2, hk, c["skin"], side, ang)
    return out


def wai_palms(skin):
    return (P("M-26 205 Q-30 140 0 110 Q30 140 26 205 Z", skin, 10) + P("M0 120 L0 200", w=6))


def face(kind, expr, c, eyes="dot"):
    o = ""
    if kind == "m":
        o += C(-100, 12, 20, c["skin"], 10) + C(100, 12, 20, c["skin"], 10)
    o += C(0, 0, 100, c["skin"], 12)
    if expr == "sick":
        o += C(0, 0, 94, OLIVE, 0, 'opacity="0.28"')
    if kind == "w":
        o += P("M-98 -18 Q-78 -100 0 -100 Q81 -100 99 -18 Q51 -53 0 -59 Q-54 -59 -98 -18 Z", HAIR, 0,
               'stroke="none"')
    else:
        o += P("M-102 0 Q-110 -112 0 -116 Q110 -112 102 0 Q96 -40 70 -55 Q30 -40 -10 -62 Q-60 -48 -90 -25 Z",
               HAIR, 10)
    # eyes
    if expr in ("happy", "bow"):
        o += P("M-50 10 Q-36 -6 -22 10 M22 10 Q36 -6 50 10", w=8)
    elif expr == "sick":
        o += P("M-50 8 L-22 8 M22 8 L50 8", w=8)
    else:
        o += C(-36, 6, 9, O, 0) + C(36, 6, 9, O, 0)
    o += C(-58, 36, 16, CHEEK, 0, 'opacity="0.55"') + C(58, 36, 16, CHEEK, 0, 'opacity="0.55"')
    # mouth
    m = {
        "smile": P("M-26 46 Q0 70 26 46", w=7),
        "happy": P("M-30 42 Q0 88 30 42 Z", "#a8453c", 7),
        "big": P("M-30 42 Q0 88 30 42 Z", "#a8453c", 7),
        "bow": P("M-22 48 Q0 64 22 48", w=7),
        "flat": P("M-24 55 L24 55", w=7),
        "sad": P("M-24 64 Q0 44 24 64", w=7),
        "hungry": E(0, 58, 13, 17, "#a8453c", 7),
        "sick": P("M-26 58 Q-13 48 0 58 Q13 68 26 58", w=7),
        "o": E(0, 56, 10, 13, "#a8453c", 7),
    }[expr]
    o += m
    if expr in ("sad", "hungry", "sick"):
        o += P("M-54 -16 L-22 -28 M54 -16 L22 -28", w=7)
    if kind == "w":
        o += C(-101, 30, 7, MUST, 5) + C(101, 30, 7, MUST, 5)
    return o


def person(x, y, s=1, kind="w", expr="smile", arms=(), tilt=0, legs=None, walk=1, face_extra="", front=""):
    c = WOMAN if kind == "w" else MAN
    o = ""
    if legs:
        o += shadow(0, 548, 140, 22)
        if legs == "stand":
            L = [(-42, 330), (-46, 520)]; Rr = [(42, 330), (46, 520)]
            fl, fr = (-54, 530), (54, 530)
        else:
            L = [(-40, 330), (-100 * walk, 470), (-120 * walk, 518)] if walk > 0 else [(40, 330), (100, 470), (120, 518)]
            Rr = [(40, 330), (75 * walk, 430), (95 * walk, 520)] if walk > 0 else [(-40, 330), (-75, 430), (-95, 520)]
            fl, fr = (L[-1][0] + 12 * walk, 528), (Rr[-1][0] + 12 * walk, 530)
        if kind == "m":
            o += thick(L, c["low"], 44, 60) + thick(Rr, c["low"], 44, 60)
        else:
            o += thick(L, c["skin"], 26, 42) + thick(Rr, c["skin"], 26, 42)
        o += E(fl[0], fl[1], 34, 17, HAIR, 9) + E(fr[0], fr[1], 34, 17, HAIR, 9)
        if kind == "w":
            o += P("M-104 300 L-128 470 Q0 490 128 470 L104 300 Z", c["low"], 12)
    if kind == "w":
        o += f'<g transform="rotate({tilt} 0 125)">' + P("M-124 40 Q-130 -90 0 -90 Q130 -84 124 40 Q134 146 75 168 L-75 168 Q-134 146 -124 40 Z", HAIR, 12) + '</g>'
    o += R(-24, 100, 48, 70, c["skin"], 0, 10)
    o += P("M-114 340 Q-118 178 0 160 Q118 178 114 340 Z", c["shirt"], 12)
    o += P("M-96 320 Q-100 220 -40 190", w=14, extra='stroke="#ffffff" opacity="0.3"')
    if kind == "w":
        o += P("M-38 166 L0 215 L38 166", w=9)
    else:
        o += P("M-30 164 L-42 205 L0 180 L42 205 L30 164", CREAM, 8)
        o += P("M0 185 L0 335", w=7) + C(0, 240, 6, CREAM, 5) + C(0, 290, 6, CREAM, 5)
    o += front
    ha = f'<g transform="rotate({tilt} 0 125) translate(0,28)">' + face(kind, expr, c) + face_extra + '</g>'
    o += ha
    for a in arms:
        name, side = a
        o += arm(name, side, c)
        if name == "wai" and side == 1:
            o += wai_palms(c["skin"])
    return g(x, y, s, o)


# ---------------------------------------------------------------- icons
def bubble(x, y, w, h, tx, ty, inner="", fill=BUB):
    bx = max(x - w / 2 + 70, min(x + w / 2 - 70, tx))
    by = y + h / 2
    tail = f"M{bx-34} {by-4} L{tx} {ty} L{bx+34} {by-4}"
    return (P(tail + " Z", fill, 12) + R(x - w / 2, y - h / 2, w, h, fill, 70, 12) +
            f'<path d="{tail} Z" fill="{fill}" stroke="none" transform="translate(0,-7)"/>' + inner)


def bowl(x, y, s=1, rice=True, steam=True, chop=False):
    o = ""
    if steam:
        o += P("M-40 -80 Q-60 -110 -40 -140 Q-20 -170 -40 -200 M0 -90 Q-20 -125 0 -155 Q20 -185 0 -215 "
               "M40 -80 Q20 -110 40 -140 Q60 -170 40 -200", w=10, extra='opacity="0.55"')
    if rice:
        o += P("M-84 0 Q-70 -70 0 -76 Q70 -70 84 0 Z", "#ffffff", 12)
        o += P("M-40 -30 l10 -6 M10 -50 l10 -4 M30 -20 l10 -6 M-15 -15 l10 -4", w=6, extra='opacity="0.5"')
    else:
        o += E(0, 0, 96, 22, "#d8c4a4", 12)
    if chop:
        o += P("M60 -10 L180 -120 M80 -2 L195 -100", w=0, extra='stroke="none"')
        o += thick([(40, -15), (175, -125)], WOOD, 10, 22) + thick([(60, -5), (195, -105)], WOOD, 10, 22)
    o += P("M-100 0 Q-95 95 0 100 Q95 95 100 0 Z", CREAM, 12)
    o += P("M-92 34 Q0 50 92 34", w=0, extra=f'stroke="{BLUE}" stroke-width="14" opacity="0.9"')
    o += P("M-80 60 Q-50 88 -10 92", w=10, extra='stroke="#ffffff" opacity="0.4"')
    o += R(-40, 96, 80, 22, CREAM, 8, 10)
    return g(x, y, s, o)


def smiley(x, y, r=80, kind="smile", fill=MUST):
    o = C(0, 0, 100, fill, 12)
    o += C(-34, -20, 11, O, 0) + C(34, -20, 11, O, 0)
    o += {"smile": P("M-45 25 Q0 75 45 25", w=11), "sad": P("M-42 50 Q0 15 42 50", w=11),
          "flat": P("M-42 35 L42 35", w=11), "big": P("M-48 18 Q0 90 48 18 Z", "#a8453c", 10)}[kind]
    o += P("M-65 -50 Q-50 -75 -20 -82", w=10, extra='stroke="#ffffff" opacity="0.45"')
    return g(x, y, r / 100, o)


def heart(x, y, s=1, fill=RED, w=12):
    return g(x, y, s, P("M0 -20 C-30 -70 -100 -40 -60 20 L0 70 L60 20 C100 -40 30 -70 0 -20 Z", fill, w))


def check(x, y, s=1, color=OLIVE):
    d = "M-60 0 L-15 45 L65 -50"
    return g(x, y, s, f'<path d="{d}" fill="none" stroke="{O}" stroke-width="46" stroke-linecap="round" stroke-linejoin="round"/>'
                      f'<path d="{d}" fill="none" stroke="{color}" stroke-width="28" stroke-linecap="round" stroke-linejoin="round"/>')


def arrow(pts, color=TEAL, w=24, head=38):
    (x1, y1), (x2, y2) = pts[-2], pts[-1]
    a = math.atan2(y2 - y1, x2 - x1)
    hx, hy = x2 + math.cos(a) * head * 0.6, y2 + math.sin(a) * head * 0.6
    l = (x2 + math.cos(a + 2.3) * head, y2 + math.sin(a + 2.3) * head)
    r = (x2 + math.cos(a - 2.3) * head, y2 + math.sin(a - 2.3) * head)
    return (thick(pts, color, w, w + 16) +
            P(f"M{hx:.0f} {hy:.0f} L{l[0]:.0f} {l[1]:.0f} L{r[0]:.0f} {r[1]:.0f} Z", color, 10))


def curved_arrow(cx, cy, r, a0, a1, color=TEAL, w=18):
    pts = []
    n = 16
    for i in range(n + 1):
        a = math.radians(a0 + (a1 - a0) * i / n)
        pts.append((round(cx + r * math.cos(a)), round(cy + r * math.sin(a))))
    return arrow(pts, color, w, 32)


def clock(x, y, r=90, h=10, m=0, fill=CREAM):
    o = C(0, 0, 100, fill, 12)
    for i in range(12):
        a = math.radians(i * 30)
        o += P(f"M{80*math.sin(a):.0f} {-80*math.cos(a):.0f} L{70*math.sin(a):.0f} {-70*math.cos(a):.0f}", w=7)
    ah = math.radians(h * 30 + m / 2); am = math.radians(m * 6)
    o += P(f"M0 0 L{45*math.sin(ah):.0f} {-45*math.cos(ah):.0f}", w=12)
    o += P(f"M0 0 L{65*math.sin(am):.0f} {-65*math.cos(am):.0f}", w=9)
    o += C(0, 0, 10, RED, 5)
    return g(x, y, r / 100, o)


def house(x, y, s=1, wall=CREAM, roof=RED):
    o = R(-90, -40, 180, 140, wall, 6, 12)
    o += P("M-125 -30 L0 -135 L125 -30 Z", roof, 12)
    o += R(-25, 20, 50, 80, WOOD, 6, 10) + R(35, -10, 40, 40, "#bcd4e0", 4, 8)
    o += R(-78, -5, 36, 36, "#bcd4e0", 4, 8)
    return g(x, y, s, o)


def shield(x, y, s=1, fill=TEAL):
    o = P("M0 -110 Q55 -80 100 -85 Q105 40 0 115 Q-105 40 -100 -85 Q-55 -80 0 -110 Z", fill, 12)
    o += P("M-60 -60 Q-65 20 -20 70", w=12, extra='stroke="#ffffff" opacity="0.3"')
    o += heart(0, 0, 0.75, CREAM, 14)
    return g(x, y, s, o)


def car(x, y, s=1, fill=RED, face_=None):
    o = shadow(0, 95, 230, 20)
    o += P("M-210 60 L-210 0 Q-205 -30 -150 -40 L-100 -110 Q-90 -125 -60 -125 L80 -125 Q110 -125 125 -105 L170 -40 "
           "Q215 -35 220 10 L220 60 Z", fill, 14)
    o += P("M-85 -45 L-55 -100 L-5 -100 L-5 -45 Z", "#bcd4e0", 10)
    o += P("M15 -45 L15 -100 L95 -100 L135 -45 Z", "#bcd4e0", 10)
    o += P("M-190 20 L200 20", w=0, extra='stroke="#ffffff" stroke-width="10" opacity="0.3"')
    o += R(190, 0, 30, 18, MUST, 6, 8)
    if face_:
        o += face_
    o += C(-120, 65, 42, "#3b3b3b", 12) + C(-120, 65, 16, "#bbbbbb", 8)
    o += C(120, 65, 42, "#3b3b3b", 12) + C(120, 65, 16, "#bbbbbb", 8)
    return g(x, y, s, o)


def globe(x, y, r=150):
    o = C(0, 0, 100, BLUE, 12)
    o += P("M-70 -55 Q-40 -80 -15 -60 Q0 -35 -25 -20 Q-35 10 -55 5 Q-80 -10 -70 -55 Z", OLIVE, 7)
    o += P("M15 -25 Q45 -45 70 -25 Q85 5 60 25 Q50 60 25 70 Q10 40 20 20 Q0 0 15 -25 Z", OLIVE, 7)
    o += P("M-60 45 Q-40 35 -25 55 Q-35 75 -55 65 Z", OLIVE, 7)
    o += P("M0 -100 Q-55 0 0 100 M0 -100 Q55 0 0 100 M-100 0 L100 0", w=4, extra='opacity="0.35"')
    o += P("M-70 -60 Q-50 -85 -20 -92", w=10, extra='stroke="#ffffff" opacity="0.4"')
    o += C(0, 0, 100, "none", 12)
    return g(x, y, r / 100, o)


def pin(x, y, s=1, fill=RED):
    o = shadow(0, 8, 45, 12)
    o += P("M0 0 Q-80 -95 -70 -150 Q-60 -215 0 -215 Q60 -215 70 -150 Q80 -95 0 0 Z", fill, 12)
    o += C(0, -148, 28, CREAM, 10)
    o += P("M-48 -160 Q-45 -195 -15 -200", w=10, extra='stroke="#ffffff" opacity="0.35"')
    return g(x, y, s, o)


def door(x, y, s=1, out_arrow=True):
    o = R(-80, -150, 160, 300, DBROWN, 6, 12)
    o += P("M-80 -150 L-10 -120 L-10 180 L-80 150 Z", WOOD, 12)
    o += C(-24, 15, 8, MUST, 5)
    o += R(-68, -138, 136, 276, "#f5e2b8", 4, 0, 'opacity="0.0"')
    if out_arrow:
        o += arrow([(0, 0), (140, 0)], TEAL, 26, 44)
    return g(x, y, s, o)


def hourglass(x, y, s=1):
    o = R(-80, -130, 160, 24, WOOD, 8, 10) + R(-80, 106, 160, 24, WOOD, 8, 10)
    o += P("M-60 -106 L60 -106 Q60 -40 12 0 Q60 40 60 106 L-60 106 Q-60 40 -12 0 Q-60 -40 -60 -106 Z", "#e3ecf1", 12)
    o += P("M-50 -90 L50 -90 Q45 -45 8 -12 L-8 -12 Q-45 -45 -50 -90 Z", MUST, 0, 'stroke="none"')
    o += P("M0 -10 L0 90", w=0, extra=f'stroke="{MUST}" stroke-width="6"')
    o += P("M-22 100 Q0 80 22 100 Z", MUST, 0, 'stroke="none"')
    o += P("M-60 -106 L60 -106 Q60 -40 12 0 Q60 40 60 106 L-60 106 Q-60 40 -12 0 Q-60 -40 -60 -106 Z", "none", 12)
    return g(x, y, s, o)


def calendar(x, y, s=1):
    o = R(-110, -90, 220, 200, CREAM, 18, 12) + P("M-110 -30 L110 -30 L110 -72 Q110 -90 92 -90 L-92 -90 Q-110 -90 -110 -72 Z", RED, 12)
    o += R(-70, -115, 18, 50, "#bbbbbb", 8, 8) + R(52, -115, 18, 50, "#bbbbbb", 8, 8)
    for i in range(4):
        for j in range(3):
            col = O if not (i == 2 and j == 1) else RED
            o += C(-72 + i * 48, 5 + j * 38, 11, col, 0, 'opacity="0.55"' if col == O else "")
    return g(x, y, s, o)


def suitcase(x, y, s=1):
    o = shadow(0, 110, 120, 14)
    o += P("M-40 -80 L-40 -110 Q-40 -125 -25 -125 L25 -125 Q40 -125 40 -110 L40 -80", w=14)
    o += R(-110, -80, 220, 185, TERRA, 22, 12)
    o += P("M-50 -80 L-50 105 M50 -80 L50 105", w=10)
    o += P("M-92 -55 L-92 80", w=12, extra='stroke="#ffffff" opacity="0.3"')
    return g(x, y, s, o)


def plane(x, y, s=1, rot=0):
    o = P("M-150 0 Q-150 -22 -120 -22 L120 -22 Q170 -18 175 0 Q170 18 120 22 L-120 22 Q-150 22 -150 0 Z", CREAM, 12)
    o += P("M-20 -20 L-80 -110 L-45 -110 L50 -20 Z", BLUE, 10) + P("M-20 20 L-80 110 L-45 110 L50 20 Z", BLUE, 10)
    o += P("M-150 -5 L-175 -60 L-140 -60 L-110 -20 Z", BLUE, 10)
    o += C(90, -2, 7, O, 0) + C(60, -2, 7, O, 0) + C(30, -2, 7, O, 0)
    return g(x, y, s, o, rot)


def thermometer(x, y, s=1, rot=0):
    o = R(-14, -120, 28, 130, "#ffffff", 14, 9) + C(0, 18, 24, RED, 9) + R(-6, -60, 12, 75, RED, 6, 0)
    o += P("M8 -100 L18 -100 M8 -80 L18 -80 M8 -60 L18 -60", w=4)
    return g(x, y, s, o, rot)


def signpost(x, y, s=1):
    o = shadow(0, 250, 90, 16)
    o += R(-16, -200, 32, 450, WOOD, 6, 12)
    o += P("M20 -190 L170 -190 L205 -155 L170 -120 L20 -120 Z", MUST, 12)
    o += P("M-20 -100 L-170 -100 L-205 -65 L-170 -30 L-20 -30 Z", TEAL, 12)
    o += P("M20 -10 L150 -10 L185 25 L150 60 L20 60 Z", RED, 12)
    return g(x, y, s, o)


def badge(x, y, s=1, fill=CREAM):
    o = P("M-40 -150 L-20 -95 M40 -150 L20 -95", w=0, extra='stroke="none"')
    o += R(-150, -95, 300, 190, fill, 26, 12)
    o += P("M-150 -40 L150 -40 L150 -69 Q150 -95 124 -95 L-124 -95 Q-150 -95 -150 -69 Z", RED, 12)
    o += C(-90, 25, 36, "#bcd4e0", 10) + C(-90, 14, 13, O, 0, 'opacity="0.6"') + P("M-112 50 Q-90 28 -68 50", w=0, extra='stroke="none"')
    o += P("M-30 5 L110 5 M-30 45 L80 45", w=14, extra='opacity="0.55"')
    return g(x, y, s, o)


def giftbox(x, y, s=1):
    o = shadow(0, 150, 170, 22)
    o += R(-140, -40, 280, 185, MUST, 10, 14) + R(-160, -95, 320, 65, TERRA, 12, 14)
    o += R(-25, -95, 50, 240, RED, 0, 12)
    o += P("M0 -95 Q-90 -190 -110 -130 Q-110 -95 0 -95 Z", RED, 12) + P("M0 -95 Q90 -190 110 -130 Q110 -95 0 -95 Z", RED, 12)
    o += P("M-120 -10 L-120 120", w=14, extra='stroke="#ffffff" opacity="0.3"')
    return g(x, y, s, o)


def temple(x, y, s=1):
    o = shadow(0, 150, 200, 20)
    o += R(-130, 20, 260, 130, CREAM, 4, 12)
    o += P("M-190 40 L-120 -40 L120 -40 L190 40 Z", RED, 12)
    o += P("M-150 -30 L-90 -110 L90 -110 L150 -30 Z", MUST, 12)
    o += P("M-110 -100 L-55 -180 L55 -180 L110 -100 Z", RED, 12)
    o += P("M0 -180 L0 -240", w=12) + P("M-190 40 L-215 15 M190 40 L215 15", w=12)
    o += R(-30, 60, 60, 90, WOOD, 28, 10)
    return g(x, y, s, o)


def lotus(x, y, s=1):
    o = P("M0 40 Q-70 20 -90 -40 Q-30 -40 0 40 Z", "#e8a4a0", 10) + P("M0 40 Q70 20 90 -40 Q30 -40 0 40 Z", "#e8a4a0", 10)
    o += P("M0 40 Q-45 -10 -35 -80 Q10 -40 0 40 Z", "#f0bcb4", 10) + P("M0 40 Q45 -10 35 -80 Q-10 -40 0 40 Z", "#f0bcb4", 10)
    o += P("M0 40 Q-30 -30 0 -100 Q30 -30 0 40 Z", "#f6d2c8", 10)
    o += P("M-80 50 Q0 75 80 50", w=10, extra=f'stroke="{LEAF}"')
    return g(x, y, s, o)


def sparkle(x, y, s=1, fill=MUST):
    return g(x, y, s, P("M0 -40 Q6 -6 40 0 Q6 6 0 40 Q-6 6 -40 0 Q-6 -6 0 -40 Z", fill, 8))


def motion(x, y, s=1, side=1):
    return g(x, y, s, P(f"M0 -40 Q{30*side} 0 0 40 M{-30*side} -65 Q{20*side} 0 {-30*side} 65", w=10,
                        extra='opacity="0.7"'))


def road(y=820):
    return (P(f"M120 {y} Q512 {y-40} 904 {y}", w=0, extra=f'stroke="{O}" stroke-width="96" opacity="0.95"') +
            P(f"M120 {y} Q512 {y-40} 904 {y}", w=0, extra='stroke="#8a8078" stroke-width="72"') +
            P(f"M160 {y-3} Q512 {y-42} 864 {y-3}", w=0, extra='stroke="#f6ecd8" stroke-width="10" stroke-dasharray="40 36"'))


def footsteps(pts, s=1):
    o = ""
    for i, (x, y) in enumerate(pts):
        dx = 12 if i % 2 else -12
        o += E(x, y + dx, 16, 10, O, 0, 'opacity="0.35"')
    return o


# ---------------------------------------------------------------- scenes
W_ = lambda **k: person(300, 575, 0.9, "w", **k)
M_ = lambda **k: person(724, 575, 0.9, "m", **k)
TOPB = (512, 250)  # default bubble center

sc = {}

# ---- Greeting
sc["Greeting/Hi"] = svg("sky",
    W_(expr="big", arms=[("wave", 1)]) + M_(expr="big", arms=[("wave", -1)]) +
    motion(488, 480, 0.8, 1) + motion(536, 480, 0.8, -1) + sparkle(512, 300, 1.4) + sparkle(420, 220, 0.8) + sparkle(610, 220, 0.8))

sc["Greeting/Bye_Informal_"] = svg("sky",
    person(260, 420, 0.8, "w", "happy", [("wave", 1)], legs="stand") +
    person(730, 420, 0.8, "m", "happy", [("wave", -1)], legs="walk", walk=1) +
    arrow([(640, 880), (880, 880)], TEAL, 22, 36) + motion(500, 330, 0.8, 1) + motion(590, 350, 0.6, -1))

sc["Greeting/Good bye_Formal_"] = svg("rose",
    door(760, 560, 1.1, True) +
    person(360, 450, 0.85, "w", "bow", [("wai", 1), ("wai", -1)], tilt=14, legs="stand") +
    P("M180 250 Q230 200 300 205", w=10, extra='opacity="0.6"') + P("M540 250 Q500 200 430 205", w=10, extra='opacity="0.6"') +
    sparkle(560, 320, 0.8))

sc["Greeting/Have you eaten yet_"] = svg("sand",
    bubble(430, 240, 400, 260, 330, 440, bowl(380, 285, 0.75, chop=True) + qmark(560, 320, 190)) +
    W_(expr="smile", arms=[("palm", 1)]) + M_(expr="o"))

sc["Greeting/How are you_"] = svg("sky",
    bubble(430, 245, 380, 250, 330, 440, smiley(370, 245, 85, "smile") + qmark(535, 315, 190)) +
    W_(expr="smile", arms=[("palm", 1)]) + M_(expr="smile"))

gauge = g(360, 290, 1,
          P("M-110 0 A110 110 0 0 1 -55 -95", w=0, extra=f'stroke="{RED}" stroke-width="40"') +
          P("M-55 -95 A110 110 0 0 1 55 -95", w=0, extra=f'stroke="{MUST}" stroke-width="40"') +
          P("M55 -95 A110 110 0 0 1 110 0", w=0, extra=f'stroke="{OLIVE}" stroke-width="40"') +
          P("M-130 0 A130 130 0 0 1 130 0 L90 0 A90 90 0 0 0 -90 0 Z", "none", 10) +
          P("M0 0 L55 -80", w=14) + C(0, 0, 18, O, 0))
sc["Greeting/How is it going_"] = svg("sage",
    bubble(430, 240, 420, 250, 330, 440, gauge + qmark(560, 320, 190)) +
    W_(expr="smile", arms=[("palm", 1)]) + M_(expr="smile"))

sc["Greeting/I am good"] = svg("sage",
    person(512, 520, 1.05, "m", "big", [("thumb", 1), ("hip", -1)]) +
    sparkle(300, 330, 1.2) + sparkle(260, 480, 0.7) + sparkle(740, 280, 0.8) + heart(300, 640, 0.5, RED, 18))

sc["Greeting/So so"] = svg("beige",
    bubble(360, 230, 280, 230, 440, 380, smiley(360, 230, 80, "flat")) +
    person(560, 560, 0.95, "m", "flat", [("flat", 1)]) +
    P("M740 740 Q780 700 820 740", w=9, extra='opacity="0.6"') + P("M740 840 Q780 880 820 840", w=9, extra='opacity="0.6"') +
    P("M720 700 L730 680 M840 700 L830 680", w=0, extra='stroke="none"'))

sc["Greeting/I_m not_so_good"] = svg("sky",
    bubble(360, 230, 280, 230, 440, 380, smiley(360, 230, 80, "sad", "#9fb3c1")) +
    person(560, 560, 0.95, "w", "sad", []) +
    g(760, 230, 1, P("M-90 20 Q-100 -40 -40 -40 Q-20 -90 30 -70 Q90 -80 90 -20 Q120 20 80 40 L-70 40 Q-100 40 -90 20 Z", "#b9c3cc", 12) +
      P("M-50 70 L-60 100 M0 70 L-10 100 M50 70 L40 100", w=10, extra=f'stroke="{BLUE}"')))

sc["Greeting/I_m sick"] = svg("sage",
    person(470, 460, 1.2, "m", "sick", [("chin", -1)],
           face_extra=thermometer(55, 70, 0.8, -60) +
           P("M70 -80 Q85 -55 70 -45 Q55 -55 70 -80 Z", "#bcd4e0", 6)) +
    g(800, 330, 1, bubble(0, 0, 200, 260, -110, 180, thermometer(0, 40, 1.05))) +
    g(300, 330, 1, P("M0 -40 Q30 0 0 20 Q-30 0 0 -40 Z", "#bcd4e0", 8)))

sc["Greeting/I have eaten_already_"] = svg("sand",
    person(400, 470, 1.05, "m", "happy", [("belly", 1)],
           front=E(0, 280, 132, 80, BLUE, 12) + P("M-80 270 Q-60 240 -30 235", w=10, extra='stroke="#ffffff" opacity="0.35"')) +
    bowl(770, 700, 0.9, rice=False, steam=False, chop=True) + check(770, 420, 1.3) +
    P("M180 380 Q160 400 180 420", w=0, extra='stroke="none"'))

sc["Greeting/I haven_t eaten yet"] = svg("beige",
    g(730, 280, 1, bubble(0, 0, 290, 260, -160, 190, bowl(0, 55, 0.7))) +
    person(400, 480, 1.05, "w", "hungry", [("belly", 1)]) +
    P("M170 760 L200 740 L185 790 L215 770 M620 760 L590 740 L605 790 L575 770", w=10, extra=f'stroke="{RED}" opacity="0.7"') +
    bowl(760, 740, 0.9, rice=False, steam=False))

sc["Greeting/Not yet"] = svg("sand",
    bubble(360, 240, 300, 280, 430, 420, hourglass(360, 240, 0.85)) +
    person(590, 560, 0.95, "w", "smile", [("stop", 1)]))

sc["Greeting/I am about to go home_"] = svg("rose",
    bubble(512, 240, 560, 260, 400, 420, clock(330, 240, 80, 5, 0) + arrow([(430, 245), (540, 245)], TEAL, 20, 34) +
           house(670, 265, 0.75)) +
    person(400, 600, 0.9, "m", "smile", [("pointback", -1)],
           front=P("M60 170 L-80 330", w=0, extra='stroke="none"')) +
    g(700, 740, 1, shadow(0, 110, 110, 14) + R(-100, -40, 200, 150, WOOD, 20, 12) + P("M-50 -40 Q-50 -100 0 -100 Q50 -100 50 -40", w=14) +
      P("M-100 20 L100 20", w=10)))

sc["Greeting/I am leaving_I have to go_"] = svg("sand",
    bubble(300, 220, 300, 230, 480, 380, clock(300, 220, 80, 12, 55, "#f6ecd8") + C(300, 220, 94, "none", 0)) +
    door(810, 600, 0.95, False) +
    person(560, 430, 0.8, "m", "o", [("wave", -1)], legs="walk", walk=1) +
    arrow([(420, 880), (690, 880)], TEAL, 22, 36) + motion(420, 520, 0.8, -1))

# I went / I will go
sc["Greeting/I went_I went to_"] = svg("sage",
    temple(250, 420, 0.8) +
    footsteps([(330, 900), (400, 880), (470, 900), (540, 880), (600, 900)]) +
    person(730, 560, 0.9, "m", "smile", [("pointback", -1)]) +
    clock(810, 230, 80, 9, 0) + curved_arrow(810, 230, 115, -30, -250, PLUM, 16) + check(250, 170, 0.7))

sc["Greeting/I will go_I am going to_"] = svg("sage",
    temple(770, 420, 0.8) +
    person(290, 560, 0.9, "m", "smile", [("point", 1)]) +
    arrow([(460, 900), (700, 900)], TEAL, 22, 36) +
    clock(215, 220, 80, 3, 0) + curved_arrow(215, 220, 115, 210, 430, PLUM, 16))

sc["Greeting/Where are you going_"] = svg("sky",
    signpost(700, 560, 1.05) +
    person(330, 470, 0.8, "m", "o", [("chin", -1)], legs="stand") +
    g(330, 190, 1, bubble(0, 0, 170, 170, 20, 140, qmark(0, 58, 160))))

sc["Greeting/Where did you go_Where are you coming from_"] = svg("beige",
    temple(210, 460, 0.6) +
    footsteps([(300, 900), (360, 885), (420, 900), (480, 885)]) +
    person(560, 480, 0.78, "m", "smile", [("wave", 1)], legs="walk", walk=-1) +
    person(830, 610, 0.8, "w", "o", []) +
    g(800, 250, 1, bubble(0, 0, 250, 230, 30, 190, pin(-40, 70, 0.55) + qmark(55, 60, 150))))

sc["Greeting/Drive safe"] = svg("sky",
    road(820) +
    car(470, 700, 1.0, RED, face_=g(40, -72, 0.28, face("m", "smile", MAN))) +
    shield(512, 300, 1.1) + motion(210, 660, 0.9, -1))

sc["Greeting/Go home safely"] = svg("sage",
    P("M200 870 Q420 820 520 760 Q620 700 690 680", w=0, extra=f'stroke="{O}" stroke-width="16" stroke-dasharray="2 34" opacity="0.5"') +
    house(700, 560, 1.15) + shield(720, 230, 0.75) +
    person(290, 480, 0.72, "m", "happy", [("wave", -1)], legs="walk", walk=1))

sc["Greeting/Safe travel"] = svg("sky",
    plane(560, 260, 1.05, -15) + suitcase(380, 700, 1.0) + shield(700, 640, 0.95) +
    P("M180 400 Q280 330 400 330", w=10, extra='stroke-dasharray="4 30" opacity="0.5"'))

sc["Greeting/See you again_See you later"] = svg("rose",
    bubble(512, 240, 380, 260, 380, 440, calendar(470, 250, 0.9) + curved_arrow(470, 250, 150, 120, -60, TEAL, 16)) +
    W_(expr="happy", arms=[("wave", 1)]) + M_(expr="happy", arms=[("wave", -1)]))

sc["Greeting/May I be excused_Formal_"] = svg("rose",
    bubble(360, 230, 360, 250, 420, 400, door(300, 240, 0.6, True) + qmark(480, 310, 170)) +
    person(590, 570, 0.95, "w", "bow", [("wai", 1), ("wai", -1)], tilt=10))

# ---- Introduce
sc["Introduce/Hello"] = svg("rose",
    person(512, 520, 1.05, "w", "happy", [("wai", 1), ("wai", -1)]) +
    sparkle(290, 330, 1.1) + sparkle(740, 300, 0.9) + lotus(760, 720, 1.0) + lotus(270, 740, 0.8))

sc["Introduce/Nice to meet you"] = svg("rose",
    person(300, 575, 0.9, "w", "happy", [("shake", 1)]) + person(724, 575, 0.9, "m", "happy", [("shake", -1)]) +
    E(512, 810, 50, 36, WOMAN["skin"], 10) + P("M470 800 Q512 830 554 800", w=6) +
    heart(512, 250, 1.3) + heart(400, 330, 0.6, "#e8a4a0") + heart(625, 330, 0.6, "#e8a4a0"))

sc["Introduce/Name"] = svg("beige",
    person(512, 520, 1.05, "m", "smile", [("hold", 1), ("hold", -1)]) +
    badge(512, 770, 0.85))

sc["Introduce/Country"] = svg("sky",
    shadow(512, 860, 200, 26) + R(492, 760, 40, 100, WOOD, 8, 12) + R(400, 840, 224, 30, WOOD, 10, 12) +
    globe(512, 480, 290) + g(620, 330, 1, R(-6, -80, 12, 150, O, 4, 0) + P("M0 -80 L90 -55 L0 -30 Z", RED, 10)))

sc["Introduce/Come from"] = svg("sky",
    globe(300, 380, 200) + pin(360, 330, 0.45) +
    P("M380 330 Q560 180 660 360", w=0, extra=f'stroke="{O}" stroke-width="36" stroke-linecap="round"') +
    P("M380 330 Q560 180 660 360", w=0, extra=f'stroke="{TEAL}" stroke-width="20" stroke-linecap="round"') +
    P("M620 340 L672 395 L690 320 Z", TEAL, 10) +
    person(720, 600, 0.85, "w", "smile", [("pointback", -1)]))

sc["Introduce/To be_People_Person from_"] = svg("sand",
    globe(512, 700, 190) +
    person(512, 310, 0.62, "m", "smile", [("wave", 1)], legs="stand") +
    pin(270, 520, 0.45) + pin(760, 560, 0.45, TEAL))

sc["Introduce/Where"] = svg("sage",
    P("M200 640 L380 580 L560 640 L800 580 L830 860 L590 900 L410 850 L230 900 Z", CREAM, 14) +
    P("M380 580 L410 850 M560 640 L590 900", w=10) +
    P("M260 780 Q400 700 520 790 Q640 860 760 720", w=0, extra=f'stroke="{RED}" stroke-width="10" stroke-dasharray="20 18"') +
    P("M230 900 L410 850 L380 580 L200 640 Z", "#2e211b", 0, 'opacity="0.08"') +
    pin(500, 740, 1.2) + qmark(512, 330, 230))

sc["Introduce/What_"] = svg("beige",
    giftbox(512, 660, 1.2) + qmark(512, 390, 260))

sc["Introduce/What about_"] = svg("sand",
    bubble(300, 230, 240, 210, 280, 390, qmark(300, 295, 180)) +
    W_(expr="smile", arms=[("palm", 1)]) + M_(expr="smile") +
    arrow([(420, 170), (560, 170), (650, 250)], TEAL, 18, 32) +
    g(760, 230, 1, bubble(0, 0, 180, 170, -20, 130, smiley(0, 0, 55, "smile"))))

sc["Introduce/_Me_you_too_"] = svg("rose",
    g(300, 240, 1, bubble(0, 0, 220, 200, 0, 190, heart(0, 0, 0.9))) +
    g(724, 240, 1, bubble(0, 0, 220, 200, 0, 190, heart(0, 0, 0.9))) +
    W_(expr="happy", arms=[("point", 1)]) + M_(expr="happy", arms=[("thumb", -1)]) +
    R(462, 215, 100, 20, TEAL, 10, 8) + R(462, 255, 100, 20, TEAL, 10, 8))

sc["Introduce/female_yes"] = svg("rose",
    person(560, 560, 0.95, "w", "happy", [], tilt=8) +
    P("M320 420 Q300 460 320 500 M280 400 Q250 460 280 520", w=10, extra='opacity="0.6"') +
    g(300, 240, 1, bubble(0, 0, 230, 210, 110, 170, check(0, 0, 1.0))))

sc["Introduce/male_yes"] = svg("sky",
    person(560, 560, 0.95, "m", "happy", [], tilt=8) +
    P("M320 420 Q300 460 320 500 M280 400 Q250 460 280 520", w=10, extra='opacity="0.6"') +
    g(300, 240, 1, bubble(0, 0, 230, 210, 110, 170, check(0, 0, 1.0))))

sc["Introduce/A polite particle"] = svg("sand",
    person(300, 575, 0.9, "w", "happy", [("wai", 1), ("wai", -1)], tilt=6) +
    person(724, 575, 0.9, "m", "happy", [("wai", 1), ("wai", -1)], tilt=-6) +
    g(300, 220, 1, bubble(0, 0, 200, 180, 20, 170, lotus(0, 20, 0.8))) +
    g(724, 220, 1, bubble(0, 0, 200, 180, -20, 170, lotus(0, 20, 0.8))))

if __name__ == "__main__":
    for k, v in sc.items():
        p = os.path.join(ROOT, "svg", k + ".svg")
        os.makedirs(os.path.dirname(p), exist_ok=True)
        open(p, "w").write(v)
    print(len(sc))

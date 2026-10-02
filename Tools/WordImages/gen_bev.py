"""Generator for Beverages and Sweetnes SVGs."""
import os

W = os.path.dirname(os.path.abspath(__file__))
S = "#2e211b"
BG = {
    "beige": ("#f3e6d6", "#c9ab93"),
    "rose": ("#f1e0dc", "#bf9a98"),
    "sage": ("#e7ecdc", "#a9b595"),
    "sky": ("#e3ecf1", "#9fb3c1"),
    "sand": ("#f5ead0", "#cfb27e"),
}


def svg(bg, body):
    a, b = BG[bg]
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">
<defs>
  <radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{a}"/><stop offset="100%" stop-color="{b}"/></radialGradient>
</defs>
<rect width="1024" height="1024" fill="url(#bg)"/>
{body}
</svg>
'''


def save(cat, key, bg, body):
    d = os.path.join(W, "svg", cat)
    os.makedirs(d, exist_ok=True)
    with open(os.path.join(d, key + ".svg"), "w") as f:
        f.write(svg(bg, body))


def g(x, y, s=1.0, inner="", op=None):
    o = f' opacity="{op}"' if op is not None else ""
    return f'<g transform="translate({x} {y}) scale({s})"{o}>{inner}</g>'


def shadow(cx, cy, rx, ry=None):
    ry = ry or rx * 0.16
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{S}" opacity="0.15"/>'


ST = f'stroke="{S}" stroke-linejoin="round" stroke-linecap="round"'


def steam(x, y, n=3, gap=70, h=150, op=0.55):
    out = f'<g fill="none" stroke="{S}" stroke-width="12" stroke-linecap="round" opacity="{op}">'
    for i in range(n):
        xx = x + (i - (n - 1) / 2) * gap
        out += f'<path d="M{xx} {y} q-28 -{h/4} 0 -{h/2} q28 -{h/4} 0 -{h/2}"/>'
    return out + "</g>"


# ---------- components (origin = bottom centre, ground at y=0) ----------

def ice_cube(x, y, s=60, rot=12):
    return (f'<g transform="translate({x} {y}) rotate({rot})">'
            f'<rect x="{-s/2}" y="{-s/2}" width="{s}" height="{s}" rx="{s*0.18}" fill="#eef8fa" opacity="0.85" {ST} stroke-width="7"/>'
            f'<path d="M{-s*0.28} {-s*0.12} L{-s*0.28} {-s*0.3} L{-s*0.1} {-s*0.3}" fill="none" stroke="#ffffff" stroke-width="7" stroke-linecap="round"/>'
            f'</g>')


def tumbler(liquid=None, level=-300, top=-420, wt=130, wb=105, ice=(), bubbles=0, surface=None, extra_back="", extra_front=""):
    """Clear glass. ice = list of (x,y,rot)."""
    def hw(y):
        return wt + (wb - wt) * (y - top) / (0 - top)
    out = f'<path d="M{-wt} {top} L{wt} {top} L{wb} 0 L{-wb} 0 Z" fill="#ffffff" opacity="0.35"/>'
    out += extra_back
    if liquid:
        h = hw(level)
        out += f'<path d="M{-h} {level} L{h} {level} L{wb-6} -6 L{-wb+6} -6 Z" fill="{liquid}"/>'
        out += f'<ellipse cx="0" cy="{level}" rx="{h}" ry="{h*0.16}" fill="{surface or liquid}" {ST} stroke-width="6"/>'
        out += f'<ellipse cx="0" cy="{level}" rx="{h}" ry="{h*0.16}" fill="#ffffff" opacity="0.25"/>'
    for (ix, iy, r) in ice:
        out += ice_cube(ix, iy, 64, r)
    import random
    rnd = random.Random(7)
    for _ in range(bubbles):
        bx = rnd.uniform(-wb + 25, wb - 25)
        by = rnd.uniform(level + 25, -30) if liquid else rnd.uniform(top + 30, -30)
        out += f'<circle cx="{bx:.0f}" cy="{by:.0f}" r="{rnd.uniform(6,12):.0f}" fill="#ffffff" opacity="0.8" stroke="{S}" stroke-width="4"/>'
    out += extra_front
    out += f'<path d="M{-wb+10} -10 L{wb-10} -10 L{wb} 0 L{-wb} 0 Z" fill="#ffffff" opacity="0.35"/>'
    out += f'<path d="M{-wt+30} {top+40} L{-wb+26} -40" stroke="#ffffff" stroke-width="16" opacity="0.55" stroke-linecap="round"/>'
    out += f'<path d="M{-wt} {top} L{-wb} 0 L{wb} 0 L{wt} {top}" fill="none" {ST} stroke-width="14"/>'
    out += f'<ellipse cx="0" cy="{top}" rx="{wt}" ry="{wt*0.14}" fill="none" {ST} stroke-width="12"/>'
    return out


def takeaway(liquid="#c98a4b", top_layer=None, ice=True, straw="#c8574b", lid=True, level=-420, sleeve=None):
    """Thai cafe plastic cup, dome lid, straw. Height ~ 600 (720 with straw)."""
    T, wt, wb = -470, 150, 108

    def hw(y):
        return wt + (wb - wt) * (y - T) / (0 - T)
    out = ""
    # straw back part
    if straw:
        out += f'<path d="M20 -20 L70 -560" stroke="{S}" stroke-width="40" stroke-linecap="round"/>'
        out += f'<path d="M20 -20 L70 -560" stroke="{straw}" stroke-width="22" stroke-linecap="round" opacity="0.6"/>'
    out += f'<path d="M{-wt} {T} L{wt} {T} L{wb} 0 L{-wb} 0 Z" fill="#ffffff" opacity="0.35"/>'
    h = hw(level)
    out += f'<path d="M{-h} {level} L{h} {level} L{wb-6} -6 L{-wb+6} -6 Z" fill="{liquid}"/>'
    if top_layer:
        mid = level + 110
        hm = hw(mid)
        out += (f'<path d="M{-h} {level} L{h} {level} L{hm} {mid} Q{hm*0.5} {mid-30} 0 {mid} Q{-hm*0.5} {mid+30} {-hm} {mid} Z" fill="{top_layer}"/>')
    out += f'<path d="M{-wb+6} -6 L{wb-6} -6 L{hw(-120)} -120 Q0 -90 {-hw(-120)} -120 Z" fill="{S}" opacity="0.12"/>'
    if ice:
        out += ice_cube(-60, level + 45, 70, 15) + ice_cube(55, level + 50, 66, -20) + ice_cube(0, level + 130, 66, 35)
        out += ice_cube(-50, level + 230, 60, -10) + ice_cube(60, level + 200, 56, 25)
    if sleeve:
        out += sleeve
    out += f'<path d="M{-wt+36} {T+40} L{-wb+28} -40" stroke="#ffffff" stroke-width="18" opacity="0.5" stroke-linecap="round"/>'
    out += f'<path d="M{-wt} {T} L{-wb} 0 L{wb} 0 L{wt} {T}" fill="none" {ST} stroke-width="14"/>'
    if lid:
        out += f'<path d="M-150 {T-4} Q-150 {T-130} 0 {T-135} Q150 {T-130} 150 {T-4} Z" fill="#ffffff" opacity="0.4" {ST} stroke-width="12"/>'
        out += f'<path d="M-110 {T-30} Q-100 {T-100} -30 {T-115}" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.8" stroke-linecap="round"/>'
        out += f'<rect x="-168" y="{T-18}" width="336" height="30" rx="15" fill="#f6f2ea" {ST} stroke-width="12"/>'
    if straw:
        out += f'<path d="M58 -505 L90 -700" stroke="{S}" stroke-width="44" stroke-linecap="round"/>'
        out += f'<path d="M58 -505 L90 -700" stroke="{straw}" stroke-width="22" stroke-linecap="round"/>'
    return out


def mug(color="#f6ecd8", liquid="#5c3d2e", shade="#e2cfb3", deco=""):
    """Mug like sample coffee, ~400 wide, 350 tall. origin bottom centre."""
    out = f'<path d="M-200 -350 L-175 -60 Q-165 0 0 0 Q165 0 175 -60 L200 -350 Z" fill="{color}" {ST} stroke-width="14"/>'
    out += f'<path d="M20 -340 L200 -350 L175 -60 Q165 0 0 0 Q80 -20 120 -80 Z" fill="{S}" opacity="0.1"/>'
    out += f'<path d="M190 -290 Q300 -300 300 -200 Q300 -100 175 -110" fill="none" stroke="{S}" stroke-width="44" stroke-linecap="round"/>'
    out += f'<path d="M190 -290 Q300 -300 300 -200 Q300 -100 175 -110" fill="none" stroke="{color}" stroke-width="18" stroke-linecap="round"/>'
    out += f'<ellipse cx="0" cy="-350" rx="200" ry="40" fill="{liquid}" {ST} stroke-width="14"/>'
    out += f'<ellipse cx="-30" cy="-358" rx="90" ry="13" fill="#ffffff" opacity="0.22"/>'
    out += f'<path d="M-165 -300 L-148 -90" stroke="#ffffff" stroke-width="18" opacity="0.35" stroke-linecap="round"/>'
    return out + deco


def sugar_cube(x, y, s=55, op=None, dashed=False):
    o = f' opacity="{op}"' if op is not None else ""
    k = 0.87 * s
    sw = 9 if s > 40 else 7
    out = f'<g transform="translate({x} {y})"{o}>'
    out += f'<path d="M0 {-s} L{k} {-s/2} L0 0 L{-k} {-s/2} Z" fill="#ffffff" {ST} stroke-width="{sw}"/>'
    out += f'<path d="M{-k} {-s/2} L0 0 L0 {s} L{-k} {s/2} Z" fill="#f1e9dc" {ST} stroke-width="{sw}"/>'
    out += f'<path d="M{k} {-s/2} L0 0 L0 {s} L{k} {s/2} Z" fill="#d9ccb7" {ST} stroke-width="{sw}"/>'
    out += f'<circle cx="{-k*0.45}" cy="{s*0.15}" r="{s*0.06}" fill="{S}" opacity="0.18"/><circle cx="{-k*0.6}" cy="{s*0.45}" r="{s*0.05}" fill="{S}" opacity="0.18"/>'
    out += f'<circle cx="{k*0.5}" cy="{s*0.3}" r="{s*0.06}" fill="{S}" opacity="0.2"/><circle cx="{-k*0.1}" cy="{-s*0.55}" r="{s*0.06}" fill="{S}" opacity="0.12"/>'
    return out + "</g>"


def prohibit(cx, cy, r, sw=26):
    d = r * 0.707
    return (f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="{S}" stroke-width="{sw+16}"/>'
            f'<path d="M{cx-d} {cy-d} L{cx+d} {cy+d}" stroke="{S}" stroke-width="{sw+16}" stroke-linecap="round"/>'
            f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="#c8574b" stroke-width="{sw}"/>'
            f'<path d="M{cx-d} {cy-d} L{cx+d} {cy+d}" stroke="#c8574b" stroke-width="{sw}" stroke-linecap="round"/>')


def cross(cx, cy, r, sw=26):
    return (f'<path d="M{cx-r} {cy-r} L{cx+r} {cy+r} M{cx+r} {cy-r} L{cx-r} {cy+r}" stroke="{S}" stroke-width="{sw+16}" stroke-linecap="round"/>'
            f'<path d="M{cx-r} {cy-r} L{cx+r} {cy+r} M{cx+r} {cy-r} L{cx-r} {cy+r}" stroke="#c8574b" stroke-width="{sw}" stroke-linecap="round"/>')


def arrow(x1, y1, x2, y2, color="#e0b04f", w=34):
    import math
    ang = math.atan2(y2 - y1, x2 - x1)
    hl = w * 1.5
    bx, by = x2 - hl * math.cos(ang), y2 - hl * math.sin(ang)
    px, py = -math.sin(ang), math.cos(ang)
    head = f"M{x2:.0f} {y2:.0f} L{bx + px*w*1.2:.0f} {by + py*w*1.2:.0f} L{bx - px*w*1.2:.0f} {by - py*w*1.2:.0f} Z"
    return (f'<path d="M{x1} {y1} L{bx:.0f} {by:.0f}" stroke="{S}" stroke-width="{w+24}" stroke-linecap="round"/>'
            f'<path d="{head}" fill="{color}" {ST} stroke-width="12"/>'
            f'<path d="M{x1} {y1} L{bx:.0f} {by:.0f}" stroke="{color}" stroke-width="{w}" stroke-linecap="round"/>')


def plus(cx, cy, r=40, color="#7fa05a", w=30):
    return (f'<path d="M{cx-r} {cy} L{cx+r} {cy} M{cx} {cy-r} L{cx} {cy+r}" stroke="{S}" stroke-width="{w+22}" stroke-linecap="round"/>'
            f'<path d="M{cx-r} {cy} L{cx+r} {cy} M{cx} {cy-r} L{cx} {cy+r}" stroke="{color}" stroke-width="{w}" stroke-linecap="round"/>')


def thermometer(x, y, level, color="#c8574b", h=380):
    """x,y = bulb centre. level 0..1"""
    top = y - h
    fl = y - 40 - (h - 60) * level
    return (f'<rect x="{x-34}" y="{top}" width="68" height="{h}" rx="34" fill="#f6f2ea" {ST} stroke-width="12"/>'
            f'<rect x="{x-14}" y="{fl}" width="28" height="{y-fl}" rx="14" fill="{color}"/>'
            f'<circle cx="{x}" cy="{y}" r="58" fill="{color}" {ST} stroke-width="12"/>'
            f'<circle cx="{x-18}" cy="{y-18}" r="14" fill="#ffffff" opacity="0.4"/>'
            + "".join(f'<path d="M{x+14} {top+50+i*55} L{x+30} {top+50+i*55}" stroke="{S}" stroke-width="7" stroke-linecap="round"/>' for i in range(5)))


def snowflake(cx, cy, r=60, color="#5d82a8"):
    out = f'<g transform="translate({cx} {cy})">'
    for a in (0, 60, 120):
        out += f'<g transform="rotate({a})"><path d="M0 {-r} L0 {r} M-{r*0.3} {-r*0.75} L0 {-r*0.5} L{r*0.3} {-r*0.75} M-{r*0.3} {r*0.75} L0 {r*0.5} L{r*0.3} {r*0.75}" fill="none" stroke="{S}" stroke-width="26" stroke-linecap="round" stroke-linejoin="round"/></g>'
    for a in (0, 60, 120):
        out += f'<g transform="rotate({a})"><path d="M0 {-r} L0 {r} M-{r*0.3} {-r*0.75} L0 {-r*0.5} L{r*0.3} {-r*0.75} M-{r*0.3} {r*0.75} L0 {r*0.5} L{r*0.3} {r*0.75}" fill="none" stroke="{color}" stroke-width="12" stroke-linecap="round" stroke-linejoin="round"/></g>'
    return out + "</g>"


def sparkle(cx, cy, r=26, color="#ffffff"):
    return f'<path d="M{cx} {cy-r} Q{cx+r*0.15} {cy-r*0.15} {cx+r} {cy} Q{cx+r*0.15} {cy+r*0.15} {cx} {cy+r} Q{cx-r*0.15} {cy+r*0.15} {cx-r} {cy} Q{cx-r*0.15} {cy-r*0.15} {cx} {cy-r} Z" fill="{color}" {ST} stroke-width="6"/>'


def wine_glass(liquid, level=-470):
    out = f'<path d="M-125 -600 Q-135 -360 0 -340 Q135 -360 125 -600 Z" fill="#ffffff" opacity="0.35"/>'
    if liquid:
        out += f'<path d="M-128 {level} Q-128 -355 0 -347 Q128 -355 128 {level} Z" fill="{liquid}"/>'
        out += f'<ellipse cx="0" cy="{level}" rx="128" ry="20" fill="{liquid}" {ST} stroke-width="7"/>'
        out += f'<ellipse cx="0" cy="{level}" rx="128" ry="20" fill="#ffffff" opacity="0.25"/>'
        out += f'<path d="M40 -380 Q100 -400 110 -450" fill="none" stroke="{S}" stroke-width="14" opacity="0.15" stroke-linecap="round"/>'
    out += f'<path d="M-95 -560 Q-100 -430 -60 -390" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.6" stroke-linecap="round"/>'
    out += f'<path d="M-125 -600 Q-135 -360 0 -340 Q135 -360 125 -600" fill="none" {ST} stroke-width="14"/>'
    out += f'<ellipse cx="0" cy="-600" rx="125" ry="18" fill="none" {ST} stroke-width="11"/>'
    out += f'<path d="M-14 -342 L-14 -30 L14 -30 L14 -342" fill="#ffffff" fill-opacity="0.5" {ST} stroke-width="12"/>'
    out += f'<ellipse cx="0" cy="-12" rx="105" ry="24" fill="#f6f2ea" {ST} stroke-width="12"/>'
    return out


def bottle(body="#5f8f4e", label="#f6ecd8", cap="#c8574b", labeldeco="", h=430, w=90, neck=30, necklen=150, liquid_op=1.0):
    t = -h
    out = (f'<path d="M{-w} -30 Q{-w} 0 {-w+30} 0 L{w-30} 0 Q{w} 0 {w} -30 L{w} {t+40} Q{w} {t-40} {neck} {t-80} '
           f'L{neck} {t-80-necklen} L{-neck} {t-80-necklen} L{-neck} {t-80} Q{-w} {t-40} {-w} {t+40} Z" fill="{body}" fill-opacity="{liquid_op}" {ST} stroke-width="14"/>')
    out += f'<path d="M{w*0.3} -10 L{w-10} -10 L{w-10} {t+40} Q{w-20} {t-20} {neck-5} {t-70} L{neck-5} {t-80-necklen} L{neck*0.2} {t-80-necklen} L{neck*0.2} {t-60} Q{w*0.3} {t} {w*0.3} {t+60} Z" fill="{S}" opacity="0.13"/>'
    out += f'<path d="M{-w+28} -60 L{-w+28} {t+40}" stroke="#ffffff" stroke-width="18" opacity="0.4" stroke-linecap="round"/>'
    if label:
        out += f'<rect x="{-w}" y="{t+110}" width="{2*w}" height="{h*0.42}" fill="{label}" {ST} stroke-width="11"/>'
        out += labeldeco
    out += f'<rect x="{-neck-6}" y="{t-80-necklen-40}" width="{2*neck+12}" height="50" rx="10" fill="{cap}" {ST} stroke-width="12"/>'
    return out


def can(color="#c8574b", deco=""):
    out = f'<path d="M-100 -390 L100 -390 L100 -30 Q100 0 0 0 Q-100 0 -100 -30 Z" fill="{color}" {ST} stroke-width="14"/>'
    out += f'<path d="M40 -380 L100 -380 L100 -30 Q100 -5 40 -3 Z" fill="{S}" opacity="0.14"/>'
    out += deco
    out += f'<path d="M-72 -340 L-72 -60" stroke="#ffffff" stroke-width="20" opacity="0.35" stroke-linecap="round"/>'
    out += f'<path d="M-100 -390 Q-100 -420 -80 -425 L80 -425 Q100 -420 100 -390" fill="#c9cdd0" {ST} stroke-width="14"/>'
    out += f'<ellipse cx="0" cy="-425" rx="85" ry="22" fill="#dde0e2" {ST} stroke-width="12"/>'
    out += f'<path d="M-30 -432 Q-10 -445 25 -432 L20 -420 Q0 -428 -25 -420 Z" fill="#aeb3b7" {ST} stroke-width="7"/>'
    out += f'<path d="M-100 -60 Q0 -40 100 -60 L100 -30 Q100 0 0 0 Q-100 0 -100 -30 Z" fill="#c9cdd0" {ST} stroke-width="12"/>'
    return out


def orange_slice(x, y, r=70, color="#e89a3c", rind="#d9825b", rot=0):
    out = f'<g transform="translate({x} {y}) rotate({rot})">'
    out += f'<circle r="{r}" fill="{rind}" {ST} stroke-width="11"/><circle r="{r*0.8}" fill="{color}"/>'
    for a in range(0, 360, 45):
        out += f'<path d="M0 0 L{r*0.78} 0" transform="rotate({a})" stroke="#fbe3b8" stroke-width="7" stroke-linecap="round"/>'
    out += f'<circle r="{r*0.12}" fill="#fbe3b8"/>'
    return out + "</g>"


def lemon_slice(x, y, r=70, rot=0):
    return orange_slice(x, y, r, "#f1d56b", "#e0b04f", rot)


# ================= BEVERAGES =================
C = "Beverages"

# Beer: mug with foam
beer = shadow(512, 830, 260)
beer += '<g transform="translate(470 830)">'
beer += f'<path d="M150 -380 Q300 -380 300 -250 L300 -170 Q300 -60 150 -70" fill="none" stroke="{S}" stroke-width="64" stroke-linecap="round"/>'
beer += f'<path d="M150 -380 Q300 -380 300 -250 L300 -170 Q300 -60 150 -70" fill="none" stroke="#f3e2ae" stroke-width="34" stroke-linecap="round"/>'
beer += f'<rect x="-170" y="-450" width="340" height="450" rx="30" fill="#e0a13f" {ST} stroke-width="14"/>'
beer += f'<rect x="60" y="-440" width="100" height="430" rx="20" fill="{S}" opacity="0.12"/>'
for i, xx in enumerate((-100, -35, 30, 95)):
    beer += f'<path d="M{xx} -380 L{xx} -50" stroke="#f3c86a" stroke-width="22" stroke-linecap="round"/>'
for (bx, by, r) in ((-60, -150, 10), (10, -250, 8), (70, -120, 12), (-20, -330, 7), (40, -330, 9), (-110, -260, 8)):
    beer += f'<circle cx="{bx}" cy="{by}" r="{r}" fill="#fff4d0" opacity="0.9"/>'
beer += f'<path d="M-140 -410 L-140 -60" stroke="#ffffff" stroke-width="20" opacity="0.35" stroke-linecap="round"/>'
beer += (f'<path d="M-195 -440 Q-220 -520 -150 -530 Q-140 -600 -60 -580 Q-10 -630 60 -590 Q130 -620 160 -545 '
         f'Q225 -540 200 -460 Q205 -420 170 -420 L170 -380 Q130 -360 120 -400 Q100 -430 60 -420 L-150 -420 Q-190 -415 -195 -440 Z" fill="#fffaf0" {ST} stroke-width="14"/>')
beer += f'<path d="M-130 -500 Q-100 -545 -50 -545" fill="none" stroke="{S}" stroke-width="8" opacity="0.2" stroke-linecap="round"/>'
beer += f'<circle cx="120" cy="-500" r="14" fill="none" stroke="{S}" stroke-width="6" opacity="0.3"/><circle cx="-40" cy="-470" r="10" fill="none" stroke="{S}" stroke-width="6" opacity="0.3"/>'
beer += "</g>"
save(C, "Beer", "sand", beer)


# Size rows
def size_row(target):
    scales = (0.5, 0.68, 0.86)
    xs = (215, 470, 770)
    out = ""
    for i, (sc, x) in enumerate(zip(scales, xs)):
        hi = i == target
        if hi:
            out += f'<ellipse cx="{x}" cy="840" rx="{190*sc+40}" ry="{40*sc+14}" fill="#fff6dc" opacity="0.9" {ST} stroke-width="0"/>'
        out += shadow(x, 840, 170 * sc)
        out += g(x, 840, sc, takeaway("#c98a4b", "#f1dcc0", straw="#c8574b" if hi else "#b8a89a"), None if hi else 0.35)
    tx, ts = xs[target], scales[target]
    top = 840 - 735 * ts
    out += sparkle(tx - 170 * ts - 10, top + 120, 30, "#e0b04f") + sparkle(tx + 170 * ts + 20, top + 60, 22, "#e0b04f")
    return out


save(C, "Small", "beige", size_row(0))
save(C, "Medium", "beige", size_row(1))
save(C, "Big_Large", "beige", size_row(2))

# Bottle: water bottle, clear blue
b = shadow(512, 850, 170)
deco = f'<path d="M-60 {-430+160} Q0 {-430+120} 60 {-430+160}" fill="none" stroke="#5d82a8" stroke-width="10" stroke-linecap="round"/>'
deco += f'<path d="M0 {-430+190} Q-35 {-430+240} 0 {-430+260} Q35 {-430+240} 0 {-430+190} Z" fill="#5d82a8" {ST} stroke-width="7"/>'
b += g(512, 850, 1.05, bottle("#a9d3e0", "#f6f2ea", "#5d82a8", deco, h=440, w=110, neck=38, necklen=110, liquid_op=0.9))
b += g(512, 850, 1.05, f'<path d="M-110 -330 Q0 -345 110 -330" fill="none" stroke="#ffffff" stroke-width="8" opacity="0.7"/>')
save(C, "Bottle", "sky", b)

# Can
cn = shadow(512, 830, 150)
deco = (f'<path d="M-100 -260 Q-40 -300 0 -250 Q40 -200 100 -240 L100 -150 Q40 -110 0 -160 Q-40 -210 -100 -170 Z" fill="#f6ecd8" {ST} stroke-width="9"/>'
        f'<circle cx="0" cy="-310" r="22" fill="#e0b04f" {ST} stroke-width="8"/>')
cn += g(512, 830, 1.35, can("#c8574b", deco))
cn += f'<g opacity="0.8">{"".join(f"<circle cx=\"{x}\" cy=\"{y}\" r=\"{r}\" fill=\"#ffffff\" stroke=\"{S}\" stroke-width=\"4\"/>" for x, y, r in ((690,420,12),(720,360,9),(700,300,7),(335,470,10),(315,410,7)))}</g>'
save(C, "Can", "rose", cn)

# Bucket of ice with beer bottles
bk = shadow(512, 840, 270)
bk += '<g transform="translate(512 840)">'
# bottle necks behind
for (bx, rot, col) in ((-90, -12, "#7d5337"), (20, 4, "#5f8f4e"), (110, 14, "#7d5337")):
    bk += (f'<g transform="translate({bx} -330) rotate({rot})"><path d="M-45 60 L-45 -60 Q-45 -110 -22 -140 L-22 -230 L22 -230 L22 -140 Q45 -110 45 -60 L45 60 Z" fill="{col}" {ST} stroke-width="12"/>'
           f'<rect x="-28" y="-265" width="56" height="42" rx="8" fill="#e0b04f" {ST} stroke-width="10"/>'
           f'<rect x="-45" y="-100" width="90" height="60" fill="#f6ecd8" {ST} stroke-width="9"/>'
           f'<path d="M-30 -130 L-10 -220" stroke="#ffffff" stroke-width="10" opacity="0.4" stroke-linecap="round"/></g>')
# ice mound
for (ix, iy, r) in ((-190, -390, 15), (-120, -410, -20), (-30, -395, 30), (60, -405, -10), (150, -395, 20), (200, -380, -25), (-160, -365, 40), (100, -370, 5)):
    bk += ice_cube(ix, iy, 76, r)
# bucket
bk += f'<path d="M-250 -380 L250 -380 L205 0 L-205 0 Z" fill="#b8c2c8" {ST} stroke-width="14"/>'
bk += f'<path d="M100 -380 L250 -380 L205 0 L90 0 Z" fill="{S}" opacity="0.12"/>'
bk += f'<path d="M-240 -300 L240 -300 M-225 -90 L225 -90" stroke="{S}" stroke-width="10" opacity="0.5"/>'
bk += f'<path d="M-200 -270 L-175 -120" stroke="#ffffff" stroke-width="20" opacity="0.5" stroke-linecap="round"/>'
bk += f'<path d="M-265 -380 L265 -380" stroke="{S}" stroke-width="34" stroke-linecap="round"/><path d="M-265 -380 L265 -380" stroke="#d7dde1" stroke-width="12" stroke-linecap="round"/>'
bk += f'<path d="M-235 -250 Q-310 -250 -300 -190 M235 -250 Q310 -250 300 -190" fill="none" {ST} stroke-width="16"/>'
bk += f'<ellipse cx="-248" cy="-250" rx="18" ry="24" fill="#8f9aa1" {ST} stroke-width="9"/><ellipse cx="248" cy="-250" rx="18" ry="24" fill="#8f9aa1" {ST} stroke-width="9"/>'
for (dx, dy) in ((-120, -200), (60, -160), (150, -250), (-40, -60)):
    bk += f'<path d="M{dx} {dy} q-10 20 0 30 q10 -10 0 -30 Z" fill="#eef8fa" {ST} stroke-width="5"/>'
bk += "</g>"
save(C, "Bucket", "sky", bk)

# Classifiers for small items: group of little bottles/cups
cl = shadow(512, 830, 330)
small_b = lambda col, cap: bottle(col, "#f6ecd8", cap, "", h=210, w=62, neck=26, necklen=40)
for (x, y, s, col, cap) in ((300, 800, 0.95, "#e7a0a0", "#c8574b"), (724, 800, 0.95, "#f1d56b", "#e0b04f")):
    cl += g(x, y, s, small_b(col, cap))
cl += g(512, 830, 1.0, small_b("#a9c98f", "#5f8f4e"))
# tiny cups in front
for (x, col) in ((390, "#c98a4b"), (630, "#e89a3c")):
    cl += g(x, 880, 0.55, tumbler(col, -280, -320, 110, 85))
save(C, "Classifiers for small items", "sage", cl)

# Cocoa: mug with marshmallows
co = shadow(500, 830, 300)
co += f'<ellipse cx="500" cy="810" rx="290" ry="50" fill="#d9c7ae" {ST} stroke-width="14"/>'
deco = (f'<path d="M-120 -220 Q-60 -250 0 -220 Q60 -190 120 -220" fill="none" stroke="#f6ecd8" stroke-width="18" stroke-linecap="round"/>'
        f'<path d="M-110 -150 Q-55 -180 0 -150 Q55 -120 110 -150" fill="none" stroke="#f6ecd8" stroke-width="18" stroke-linecap="round"/>')
co += g(500, 800, 1.0, mug("#b0654a", "#6b3f2a", deco=deco))
for (mx, my, r) in ((440, 440, -15), (520, 435, 10), (590, 448, 25), (470, 465, 30), (380, 455, 5)):
    co += f'<rect x="{mx-30}" y="{my-22}" width="60" height="44" rx="12" transform="rotate({r} {mx} {my})" fill="#fbf3ec" {ST} stroke-width="8"/>'
co += steam(500, 380, 3, 80, 180)
save(C, "Cocoa", "rose", co)

# Coffee: iced? -> hot coffee cup with beans (Thai cafe style)
cf = shadow(470, 830, 280)
cf += f'<ellipse cx="470" cy="810" rx="270" ry="48" fill="#f6ecd8" {ST} stroke-width="14"/>'
cf += f'<ellipse cx="470" cy="805" rx="160" ry="22" fill="{S}" opacity="0.1"/>'
cf += g(470, 800, 0.95, mug("#5d82a8", "#5c3d2e", deco=f'<path d="M-60 -230 L60 -230 M-45 -180 L45 -180" stroke="#f6ecd8" stroke-width="14" stroke-linecap="round"/>'))
cf += f'<path d="M430 470 Q470 455 520 470" fill="none" stroke="#c98a4b" stroke-width="10" stroke-linecap="round" opacity="0.8"/>'
for (bx, by, r) in ((770, 830, 30), (840, 800, -20), (800, 870, 70), (215, 850, -40), (170, 815, 20)):
    cf += (f'<g transform="translate({bx} {by}) rotate({r})"><ellipse rx="32" ry="22" fill="#6b4230" {ST} stroke-width="9"/>'
           f'<path d="M-24 0 Q0 -12 24 0" fill="none" stroke="{S}" stroke-width="7" stroke-linecap="round"/></g>')
cf += steam(470, 390, 3, 80, 180)
save(C, "Coffee", "beige", cf)

# Cold: glass with ice and snowflake
cd = shadow(470, 850, 200)
cd += g(470, 850, 1.3, tumbler("#bfe0ea", -380, -440, 145, 110,
                              ice=((-55, -365, 12), (50, -350, -18), (-10, -290, 35), (-55, -210, -10), (55, -220, 22), (0, -130, 8)),
                              surface="#d8eff5"))
for (dx, dy) in ((300, 420), (650, 480), (640, 650), (310, 700)):
    cd += f'<path d="M{dx} {dy} q-12 22 0 34 q12 -12 0 -34 Z" fill="#eef8fa" {ST} stroke-width="6"/>'
cd += snowflake(760, 260, 70)
cd += snowflake(230, 300, 45)
save(C, "Cold_for drink_with ice", "sky", cd)

# Glass/mug/cup: glass + mug + cup
gm = shadow(512, 830, 360)
gm += g(300, 830, 0.95, tumbler("#bfe0ea", -290, -420, 120, 95))
gm += g(600, 830, 0.8, mug("#e0b04f", "#f6ecd8", deco=f'<circle cx="0" cy="-190" r="45" fill="#c8574b" {ST} stroke-width="9"/>'))
save(C, "Glass_mug_cup", "sand", gm)

# Green tea: iced green milk tea in takeaway
gt = shadow(470, 860, 190)
gt += g(470, 860, 1.0, takeaway("#8fb46a", "#dbe8c2", straw="#5f8f4e"))
leaf = lambda x, y, r, s=1: (f'<g transform="translate({x} {y}) rotate({r}) scale({s})"><path d="M0 60 Q-60 0 0 -70 Q60 0 0 60 Z" fill="#5f8f4e" {ST} stroke-width="10"/>'
                             f'<path d="M0 50 L0 -55" stroke="#2e211b" stroke-width="6" opacity="0.5" stroke-linecap="round"/></g>')
gt += leaf(740, 700, 35) + leaf(810, 610, -15, 0.8) + leaf(215, 760, -30, 0.8)
save(C, "Green tea", "sage", gt)

# Hot: red mug with big steam + thermometer
ht = shadow(440, 830, 280)
ht += g(440, 830, 0.95, mug("#c8574b", "#8a4b2e", deco=f'<path d="M0 -250 Q-40 -200 -20 -160 Q-40 -170 -50 -150 Q-60 -80 0 -80 Q60 -80 50 -150 Q40 -130 30 -140 Q40 -200 0 -250 Z" fill="#e0b04f" {ST} stroke-width="9"/>'))
ht += steam(440, 440, 3, 90, 250, 0.7)
ht += thermometer(820, 760, 0.85)
save(C, "Hot", "rose", ht)

# Ice: pile of ice cubes
ic = shadow(512, 830, 300)
cube3 = lambda x, y, s, r: (f'<g transform="translate({x} {y}) rotate({r})">'
                            f'<path d="M{-s} {-s*0.5} L0 {-s} L{s} {-s*0.5} L0 0 Z" fill="#f4fbfc" {ST} stroke-width="12"/>'
                            f'<path d="M{-s} {-s*0.5} L0 0 L0 {s} L{-s} {s*0.5} Z" fill="#cfe7ee" {ST} stroke-width="12"/>'
                            f'<path d="M{s} {-s*0.5} L0 0 L0 {s} L{s} {s*0.5} Z" fill="#a9cfdc" {ST} stroke-width="12"/>'
                            f'<path d="M{-s*0.75} {s*0.05} L{-s*0.75} {s*0.35}" stroke="#ffffff" stroke-width="12" stroke-linecap="round" opacity="0.8"/>'
                            f'<path d="M{-s*0.3} {-s*0.55} L{s*0.1} {-s*0.75}" stroke="#ffffff" stroke-width="10" stroke-linecap="round"/></g>')
ic += cube3(370, 700, 120, -6) + cube3(650, 710, 120, 8) + cube3(510, 490, 120, 3)
ic += sparkle(760, 380, 34) + sparkle(250, 450, 26) + sparkle(680, 250, 20)
save(C, "Ice", "sky", ic)

# Lemon tea
lt = shadow(480, 850, 210)
lt += g(480, 850, 1.3, tumbler("#d98f3a", -370, -440, 140, 108,
                              ice=((-50, -330, 14), (45, -320, -18), (0, -250, 30)), surface="#e8a84f"))
lt += lemon_slice(640, 290, 85, 0)
lt += f'<rect x="545" y="270" width="60" height="30" fill="none"/>'
lt += lemon_slice(210, 800, 55, 20)
save(C, "Lemon tea", "sand", lt)

# Liquor: whisky bottle + shot glass
lq = shadow(512, 840, 320)
deco = (f'<path d="M-50 {-400+170} L50 {-400+170}" stroke="#9a6a45" stroke-width="10" stroke-linecap="round"/>'
        f'<circle cx="0" cy="{-400+240}" r="34" fill="#c8574b" {ST} stroke-width="8"/>')
lq += g(400, 840, 1.1, bottle("#b8732f", "#f6ecd8", "#5c3d2e", deco, h=400, w=110, neck=30, necklen=130, liquid_op=1))
lq += g(690, 840, 0.75, tumbler("#c98a3b", -170, -260, 110, 85))
save(C, "Liquor", "beige", lq)

# Medium etc done. Milk: carton + glass
mk = shadow(512, 840, 330)
mk += '<g transform="translate(370 840)">'
mk += f'<path d="M-130 -420 L130 -420 L130 0 L-130 0 Z" fill="#f6f2ea" {ST} stroke-width="14"/>'
mk += f'<path d="M40 -420 L130 -420 L130 0 L40 0 Z" fill="{S}" opacity="0.1"/>'
mk += f'<path d="M-130 -420 L-90 -540 L90 -540 L130 -420 Z" fill="#5d82a8" {ST} stroke-width="14"/>'
mk += f'<rect x="-95" y="-590" width="190" height="50" fill="#5d82a8" {ST} stroke-width="12"/>'
mk += f'<path d="M-130 -300 L130 -300 L130 -130 L-130 -130 Z" fill="#5d82a8" {ST} stroke-width="11"/>'
mk += (f'<path d="M-60 -170 Q-70 -230 -40 -250 L-30 -270 L30 -270 L40 -250 Q70 -230 60 -170 Z" fill="#f6f2ea" {ST} stroke-width="8"/>'
       f'<circle cx="-20" cy="-210" r="10" fill="{S}"/><circle cx="25" cy="-200" r="12" fill="{S}"/>')
mk += f'<path d="M-100 -400 L-100 -40" stroke="#ffffff" stroke-width="18" opacity="0.5" stroke-linecap="round"/>'
mk += "</g>"
mk += g(680, 840, 0.95, tumbler("#fbf7ee", -330, -400, 115, 90, surface="#ffffff"))
save(C, "Milk", "sky", mk)

# Not cold (room temp): glass of water, no ice, thermometer mid, crossed snowflake
nc = shadow(420, 850, 200)
nc += g(420, 850, 1.25, tumbler("#cfe6ee", -320, -440, 140, 108, surface="#e1f1f6"))
nc += thermometer(800, 820, 0.45, "#e0b04f", 360)
nc += snowflake(790, 250, 80) + cross(790, 250, 90, 22)
save(C, "Not cold_Room_temperature", "sand", nc)

# Red wine: bottle + glass
rw = shadow(512, 850, 320)
rw += g(350, 850, 1.0, bottle("#6b2b3a", "#f6ecd8", "#8a5a86", f'<path d="M-40 {-430+200} Q0 {-430+160} 40 {-430+200} Q0 {-430+260} -40 {-430+200} Z" fill="#8a2f3f" {ST} stroke-width="7"/>'))
rw += g(660, 850, 1.0, wine_glass("#9b2d3c"))
save(C, "Red wine", "rose", rw)

# White wine
ww = shadow(512, 850, 320)
ww += g(350, 850, 1.0, bottle("#8fae6a", "#f6ecd8", "#e0b04f", f'<path d="M-40 {-430+200} Q0 {-430+160} 40 {-430+200} Q0 {-430+260} -40 {-430+200} Z" fill="#e0c56a" {ST} stroke-width="7"/>', liquid_op=0.95))
ww += g(660, 850, 1.0, wine_glass("#f1dc8e"))
save(C, "White wine", "sage", ww)

# Smoothie/Blend: blender
sm = shadow(512, 860, 230)
sm += '<g transform="translate(512 860)">'
sm += f'<path d="M-170 -200 L170 -200 L190 0 L-190 0 Z" fill="#5d82a8" {ST} stroke-width="14"/>'
sm += f'<path d="M60 -200 L170 -200 L190 0 L70 0 Z" fill="{S}" opacity="0.15"/>'
sm += f'<circle cx="0" cy="-100" r="42" fill="#f6ecd8" {ST} stroke-width="10"/><path d="M0 -100 L0 -135" stroke="{S}" stroke-width="10" stroke-linecap="round"/>'
sm += f'<path d="M-150 -220 L-180 -600 L180 -600 L150 -220 Z" fill="#ffffff" opacity="0.4"/>'
sm += f'<path d="M-160 -330 Q-80 -380 0 -345 Q80 -310 168 -360 L150 -220 L-150 -220 Z" fill="#e58a9a"/>'
sm += f'<path d="M-168 -430 Q-90 -470 0 -440 Q90 -410 172 -450 L168 -360 Q80 -310 0 -345 Q-80 -380 -160 -330 Z" fill="#f2b3bd"/>'
for (fx, fy, c) in ((-80, -300, "#c8574b"), (50, -270, "#c8574b"), (-20, -390, "#7fa05a"), (90, -400, "#e0b04f"), (-110, -400, "#c8574b")):
    sm += f'<circle cx="{fx}" cy="{fy}" r="18" fill="{c}" {ST} stroke-width="7"/>'
sm += f'<path d="M-168 -430 Q-90 -470 0 -440 Q90 -410 172 -450" fill="none" {ST} stroke-width="8"/>'
sm += f'<path d="M-130 -560 L-110 -260" stroke="#ffffff" stroke-width="18" opacity="0.6" stroke-linecap="round"/>'
sm += f'<path d="M-150 -220 L-180 -600 L180 -600 L150 -220 Z" fill="none" {ST} stroke-width="14"/>'
sm += f'<path d="M175 -540 Q270 -540 265 -420 Q260 -300 160 -300" fill="none" stroke="{S}" stroke-width="50" stroke-linecap="round"/>'
sm += f'<path d="M175 -540 Q270 -540 265 -420 Q260 -300 160 -300" fill="none" stroke="#5d82a8" stroke-width="22" stroke-linecap="round"/>'
sm += f'<rect x="-200" y="-640" width="400" height="50" rx="20" fill="#5d82a8" {ST} stroke-width="14"/>'
sm += f'<rect x="-50" y="-690" width="100" height="55" rx="14" fill="#4a6c8f" {ST} stroke-width="12"/>'
sm += f'<path d="M-290 -470 Q-260 -440 -290 -410 M-320 -490 Q-270 -440 -320 -390" fill="none" stroke="{S}" stroke-width="12" stroke-linecap="round" opacity="0.5"/>'
sm += f'<path d="M320 -170 Q290 -140 320 -110 M350 -190 Q300 -140 350 -90" fill="none" stroke="{S}" stroke-width="12" stroke-linecap="round" opacity="0.5"/>'
sm += "</g>"
save(C, "Smoothie_Blend", "rose", sm)

# Soda: tall glass with fizz bubbles + lime
sd = shadow(470, 860, 190)
fizz = "".join(f'<circle cx="{x}" cy="{y}" r="{r}" fill="#ffffff" stroke="{S}" stroke-width="5"/>' for x, y, r in ((-40, -560, 12), (20, -600, 9), (60, -545, 14), (-80, -610, 8), (0, -660, 7), (-10, -520, 10)))
sd += g(470, 860, 1.05, tumbler("#e6f3ee", -490, -540, 140, 110, bubbles=26, ice=((-50, -460, 15), (50, -455, -12)), surface="#f4fbf8", extra_front=fizz))
sd += g(470, 860, 1.05, f'<path d="M40 -350 L120 -720" stroke="{S}" stroke-width="40" stroke-linecap="round"/><path d="M40 -350 L120 -720" stroke="#4f9a9a" stroke-width="20" stroke-linecap="round"/>')
sd += orange_slice(330, 300, 70, "#b8d67a", "#7fa05a", 0)
save(C, "Soda", "sage", sd)

# Tea: teapot + cup
te = shadow(512, 840, 340)
te += '<g transform="translate(410 840)">'
te += f'<path d="M140 -170 Q230 -190 260 -330 L300 -350 L280 -310 Q250 -140 150 -90 Z" fill="#4f9a9a" {ST} stroke-width="14"/>'
te += f'<path d="M-150 -250 Q-270 -270 -250 -160 Q-235 -80 -140 -100" fill="none" stroke="{S}" stroke-width="44" stroke-linecap="round"/>'
te += f'<path d="M-150 -250 Q-270 -270 -250 -160 Q-235 -80 -140 -100" fill="none" stroke="#4f9a9a" stroke-width="18" stroke-linecap="round"/>'
te += f'<path d="M-180 -150 Q-190 -330 0 -340 Q190 -330 180 -150 Q170 0 0 0 Q-170 0 -180 -150 Z" fill="#4f9a9a" {ST} stroke-width="14"/>'
te += f'<path d="M60 -330 Q185 -300 180 -150 Q170 0 0 0 Q140 -60 110 -230 Z" fill="{S}" opacity="0.13"/>'
te += f'<path d="M-180 -170 Q0 -130 180 -170" fill="none" stroke="#f6ecd8" stroke-width="18"/>'
te += "".join(f'<circle cx="{x}" cy="-110" r="12" fill="#f6ecd8"/>' for x in (-100, -50, 0, 50, 100))
te += f'<path d="M-130 -290 Q-100 -320 -40 -320" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.4" stroke-linecap="round"/>'
te += f'<path d="M-110 -330 Q-100 -400 0 -405 Q100 -400 110 -330 Z" fill="#3f8080" {ST} stroke-width="14"/>'
te += f'<circle cx="0" cy="-425" r="26" fill="#e0b04f" {ST} stroke-width="11"/>'
te += "</g>"
te += f'<ellipse cx="740" cy="840" rx="120" ry="24" fill="#f6ecd8" {ST} stroke-width="12"/>'
te += (f'<path d="M650 700 L665 800 Q680 830 740 830 Q800 830 815 800 L830 700 Z" fill="#f6ecd8" {ST} stroke-width="12"/>'
       f'<ellipse cx="740" cy="700" rx="90" ry="20" fill="#b8732f" {ST} stroke-width="11"/>'
       f'<path d="M680 730 Q740 745 800 730" fill="none" stroke="#4f9a9a" stroke-width="10"/>')
te += steam(740, 660, 2, 50, 130)
save(C, "Tea", "sage", te)

# Thai tea: orange tea with milk layer in takeaway
tt = shadow(512, 860, 190)
tt += g(512, 860, 1.0, takeaway("#e0822f", "#f5d9b3", straw="#c8574b"))
tt += f'<path d="M790 500 q-25 45 0 70 q25 -25 0 -70 Z" fill="#eef8fa" {ST} stroke-width="7"/>'
save(C, "Thai tea", "sand", tt)

# Water (normal): glass of water + drop
wa = shadow(440, 850, 200)
wa += g(440, 850, 1.25, tumbler("#cfe6ee", -340, -440, 140, 108, surface="#e1f1f6"))
wa += (f'<path d="M780 200 Q700 330 700 390 Q700 470 780 470 Q860 470 860 390 Q860 330 780 200 Z" fill="#7fb3cf" {ST} stroke-width="14"/>'
       f'<path d="M735 390 Q735 350 760 320" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.6" stroke-linecap="round"/>')
save(C, "Water_Normal_", "sky", wa)

# Juice: orange juice glass + orange
ju = shadow(500, 850, 290)
ju += g(430, 850, 1.25, tumbler("#eea23a", -360, -440, 140, 108, surface="#f5b95a",
                               extra_front=f'<path d="M60 -330 L130 -620" stroke="{S}" stroke-width="36" stroke-linecap="round"/><path d="M60 -330 L130 -620" stroke="#7fa05a" stroke-width="18" stroke-linecap="round"/>'))
ju += orange_slice(600, 300, 78, rot=0)
ju += (f'<circle cx="760" cy="780" r="95" fill="#e8913a" {ST} stroke-width="14"/>'
       f'<path d="M720 720 Q740 705 765 705" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.4" stroke-linecap="round"/>'
       f'<path d="M760 690 Q790 650 830 665 Q800 700 760 690 Z" fill="#5f8f4e" {ST} stroke-width="9"/>')
save(C, "_Juice", "sand", ju)

# ================= SWEETNES =================
C = "Sweetnes"


def sweet_scene(filled, extra=False, arrow_dir=None):
    out = shadow(380, 860, 190)
    out += g(380, 860, 0.95, takeaway("#c98a4b", "#f1dcc0", straw="#c8574b"))
    # meter
    mx = 760
    out += f'<rect x="{mx-95}" y="265" width="190" height="600" rx="50" fill="#f6ecd8" opacity="0.7" {ST} stroke-width="12"/>'
    for i in range(4):
        cy = 790 - i * 140
        if i < filled:
            out += sugar_cube(mx, cy, 50)
        else:
            out += sugar_cube(mx, cy, 50, op=0.22)
    if extra:
        out += sugar_cube(mx, 185, 50) + plus(mx + 115, 170, 34)
        out += sparkle(mx - 110, 180, 26, "#e0b04f")
    if arrow_dir == "down":
        out += arrow(590, 330, 590, 560, "#5d82a8")
    return out


save(C, "Less sweet", "rose", sweet_scene(1, arrow_dir="down"))
save(C, "Standard sweet", "rose", sweet_scene(2))
save(C, "Extra sweet_Add more sweet", "rose", sweet_scene(4, extra=True))

# Not sweet: cup + meter empty
ns = sweet_scene(0)
save(C, "Not sweet", "sage", ns)

# Not sweet at all: cup + big prohibition over sugar & syrup
na = shadow(360, 860, 190)
na += g(360, 860, 0.95, takeaway("#c98a4b", "#f1dcc0", straw="#c8574b"))
na += sugar_cube(650, 620, 62) + sugar_cube(770, 620, 62) + sugar_cube(710, 510, 62)
na += prohibit(712, 570, 190, 30)
save(C, "Not sweet at all", "sage", na)

# No added / without: spoon of sugar pouring into cup, prohibited
nw = shadow(430, 860, 230)
nw += g(430, 860, 1.0, mug("#f6ecd8", "#b07a50"))
nw += f'<path d="M620 250 L850 110" stroke="{S}" stroke-width="44" stroke-linecap="round"/><path d="M620 250 L850 110" stroke="#c9cdd0" stroke-width="22" stroke-linecap="round"/>'
nw += f'<ellipse cx="560" cy="285" rx="100" ry="55" transform="rotate(-30 560 285)" fill="#c9cdd0" {ST} stroke-width="14"/>'
nw += sugar_cube(535, 255, 34) + sugar_cube(590, 250, 30)
nw += sugar_cube(470, 390, 32) + sugar_cube(420, 330, 26)
nw += prohibit(520, 330, 175, 28)
save(C, "No added_not put_without", "beige", nw)

# Add/put in/with: cube dropping into mug with arrow
ad = shadow(470, 860, 230)
ad += g(470, 860, 1.0, mug("#f6ecd8", "#b07a50"))
ad += sugar_cube(470, 320, 72)
ad += f'<path d="M380 490 Q360 450 330 460 M560 490 Q580 450 610 460 M430 480 Q425 440 410 430 M510 480 Q515 440 530 430" fill="none" stroke="#b07a50" stroke-width="14" stroke-linecap="round"/>'
ad += f'<path d="M400 230 L400 170 M540 230 L540 170" stroke="{S}" stroke-width="10" opacity="0.4" stroke-linecap="round"/>'
ad += arrow(250, 180, 250, 420, "#7fa05a")
ad += plus(760, 250, 48)
save(C, "Add_put in_with", "sage", ad)

# Sugar: sugar bowl with cubes
su = shadow(512, 840, 300)
su += '<g transform="translate(512 840)">'
su += f'<ellipse cx="0" cy="-20" rx="150" ry="30" fill="#e2cfb3" {ST} stroke-width="12"/>'
for (x, y, s) in ((-120, -300, 58), (0, -320, 62), (115, -300, 58), (-60, -370, 56), (60, -375, 56), (0, -420, 52)):
    su += sugar_cube(x, y, s)
su += f'<path d="M-250 -300 Q-250 -40 0 -40 Q250 -40 250 -300 Z" fill="#5d82a8" {ST} stroke-width="14"/>'
su += f'<path d="M80 -290 L250 -300 Q250 -40 0 -40 Q170 -90 80 -290 Z" fill="{S}" opacity="0.13"/>'
su += f'<path d="M-240 -220 Q0 -170 240 -220" fill="none" stroke="#f6ecd8" stroke-width="16"/>'
su += "".join(f'<circle cx="{x}" cy="-150" r="14" fill="#f6ecd8"/>' for x in (-120, -60, 0, 60, 120))
su += f'<path d="M-210 -260 Q-200 -150 -150 -100" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.4" stroke-linecap="round"/>'
su += f'<ellipse cx="0" cy="-300" rx="250" ry="34" fill="none" {ST} stroke-width="12"/>'
su += "</g>"
su += sugar_cube(830, 800, 50) + sugar_cube(200, 810, 44)
save(C, "Sugar", "beige", su)

# Syrup: pump bottle with drip
sy = shadow(470, 850, 230)
sy += '<g transform="translate(470 850)">'
sy += f'<path d="M-130 -30 Q-130 0 -100 0 L100 0 Q130 0 130 -30 L130 -380 Q130 -420 60 -440 L60 -480 L-60 -480 L-60 -440 Q-130 -420 -130 -380 Z" fill="#d98f3a" fill-opacity="0.9" {ST} stroke-width="14"/>'
sy += f'<path d="M50 -10 L120 -10 L120 -380 Q115 -410 60 -425 Z" fill="{S}" opacity="0.13"/>'
sy += f'<path d="M-100 -60 L-100 -370" stroke="#ffffff" stroke-width="18" opacity="0.4" stroke-linecap="round"/>'
sy += f'<rect x="-130" y="-300" width="260" height="170" fill="#f6ecd8" {ST} stroke-width="11"/>'
sy += sugar_cube(0, -215, 42)
sy += f'<rect x="-70" y="-530" width="140" height="60" rx="12" fill="#5c3d2e" {ST} stroke-width="12"/>'
sy += f'<rect x="-16" y="-620" width="32" height="95" fill="#c9cdd0" {ST} stroke-width="10"/>'
sy += f'<path d="M-50 -650 L150 -650 L150 -610 L-50 -610 Z" fill="#5c3d2e" {ST} stroke-width="12"/>'
sy += f'<path d="M150 -630 L230 -630 L230 -590" fill="none" stroke="{S}" stroke-width="30" stroke-linecap="round" stroke-linejoin="round"/>'
sy += f'<path d="M150 -630 L230 -630 L230 -590" fill="none" stroke="#5c3d2e" stroke-width="12" stroke-linecap="round" stroke-linejoin="round"/>'
sy += f'<path d="M230 -560 Q205 -515 205 -495 Q205 -470 230 -470 Q255 -470 255 -495 Q255 -515 230 -560 Z" fill="#d98f3a" {ST} stroke-width="10"/>'
sy += "</g>"
sy += f'<path d="M700 780 Q640 790 650 820 Q670 845 760 840 Q840 835 830 805 Q815 780 760 790 Q740 770 700 780 Z" fill="#d98f3a" {ST} stroke-width="11"/>'
sy += f'<path d="M690 805 Q720 795 745 800" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.5" stroke-linecap="round"/>'
save(C, "Syrup", "sand", sy)

# Sweet: lollipop + wrapped candy + donut sparkles
sw = shadow(512, 850, 320)
sw += f'<path d="M420 520 L380 840" stroke="{S}" stroke-width="40" stroke-linecap="round"/><path d="M420 520 L380 840" stroke="#f6ecd8" stroke-width="20" stroke-linecap="round"/>'
sw += f'<circle cx="430" cy="400" r="190" fill="#e58a9a" {ST} stroke-width="14"/>'
sw += (f'<path d="M430 400 m0 -20 a20 20 0 1 1 -20 20 a40 40 0 1 1 40 40 a60 60 0 1 1 -60 -60 a80 80 0 1 1 80 80 a100 100 0 1 1 -100 -100 a120 120 0 1 1 120 120 a140 140 0 1 1 -140 -140" '
       f'fill="none" stroke="#fbeff1" stroke-width="22" stroke-linecap="round"/>')
sw += f'<path d="M300 320 Q330 260 390 245" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.5" stroke-linecap="round"/>'
sw += f'<circle cx="430" cy="400" r="190" fill="none" {ST} stroke-width="14"/>'
# wrapped candy
sw += '<g transform="translate(700 700) rotate(-20)">'
sw += f'<path d="M-90 0 L-170 -60 L-160 0 L-170 60 Z" fill="#e0b04f" {ST} stroke-width="12"/><path d="M90 0 L170 -60 L160 0 L170 60 Z" fill="#e0b04f" {ST} stroke-width="12"/>'
sw += f'<ellipse rx="100" ry="70" fill="#7fa05a" {ST} stroke-width="14"/>'
sw += f'<path d="M-40 -65 L-10 65 M20 -68 L50 62" stroke="#dbe8c2" stroke-width="16"/>'
sw += f'<ellipse rx="100" ry="70" fill="none" {ST} stroke-width="14"/>'
sw += f'<path d="M-60 -35 Q-40 -50 -10 -52" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.5" stroke-linecap="round"/>'
sw += "</g>"
sw += sparkle(720, 260, 36, "#e0b04f") + sparkle(820, 420, 24, "#ffffff") + sparkle(210, 700, 28, "#e0b04f")
save(C, "Sweet", "rose", sw)

print("done")

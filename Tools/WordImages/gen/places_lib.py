"""Drawing helpers for the Places category (self-contained)."""
import math

D = '#2e211b'
BG = {'beige': ('#f3e6d6', '#c9ab93'), 'rose': ('#f1e0dc', '#bf9a98'), 'sage': ('#e7ecdc', '#a9b595'),
      'sky': ('#e3ecf1', '#9fb3c1'), 'sand': ('#f5ead0', '#cfb27e')}
CREAM = '#f6ecd8'
GLASS = '#bfdde6'
WOOD = '#9a6a45'
DBROWN = '#5c3d2e'
RED = '#c8574b'
TERRA = '#d9825b'
MUST = '#e0b04f'
OLIVE = '#7fa05a'
LEAF = '#5f8f4e'
TEAL = '#4f9a9a'
BLUE = '#5d82a8'
PLUM = '#8a5a86'
SKIN = '#f1c9a5'
SKIN2 = '#e0a882'
SKIN3 = '#b57d58'
HAIR = '#3b2a22'
GREY = '#cfd6dc'


def svg(bg, body):
    l, d = BG[bg]
    return ('<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">\n'
            f'<defs><radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{l}"/>'
            f'<stop offset="100%" stop-color="{d}"/></radialGradient></defs>\n'
            '<rect width="1024" height="1024" fill="url(#bg)"/>\n' + body + '\n</svg>\n')


def st(w=12):
    if not w:
        return 'stroke="none"'
    return f'stroke="{D}" stroke-width="{w}" stroke-linejoin="round" stroke-linecap="round"'


def R(x, y, w, h, fill, sw=12, rx=0, extra=''):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {st(sw)} {extra}/>\n'


def C(cx, cy, r, fill, sw=12, extra=''):
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {st(sw)} {extra}/>\n'


def E(cx, cy, rx, ry, fill, sw=12, extra=''):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {st(sw)} {extra}/>\n'


def P(d, fill, sw=12, extra=''):
    return f'<path d="{d}" fill="{fill}" {st(sw)} {extra}/>\n'


def PG(pts, fill, sw=12, extra=''):
    s = ' '.join(f'{x},{y}' for x, y in pts)
    return f'<polygon points="{s}" fill="{fill}" {st(sw)} {extra}/>\n'


def L(x1, y1, x2, y2, sw=10, color=D, extra=''):
    return (f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{sw}" '
            f'stroke-linecap="round" {extra}/>\n')


def TL(x1, y1, x2, y2, w, color, ow=10):
    """Thick outlined line."""
    return L(x1, y1, x2, y2, w + ow * 2 * 0.7, D) + L(x1, y1, x2, y2, w, color)


def TP(d, w, color, ow=7):
    """Thick outlined open path."""
    return (f'<path d="{d}" fill="none" stroke="{D}" stroke-width="{w + ow * 2}" stroke-linecap="round" stroke-linejoin="round"/>\n'
            f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"/>\n')


def F(d, fill='#ffffff', op=0.3):
    return f'<path d="{d}" fill="{fill}" opacity="{op}"/>\n'


def FR(x, y, w, h, fill='#ffffff', op=0.3, rx=0):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" opacity="{op}"/>\n'


def G(tx, ty, s, inner, rot=0):
    return f'<g transform="translate({tx} {ty}) rotate({rot}) scale({s})">\n{inner}</g>\n'


def shadow(cx, cy, rx, ry=28):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{D}" opacity="0.15"/>\n'


def T(x, y, s, txt, fill='#ffffff', sw=8):
    return (f'<text x="{x}" y="{y}" font-family="Arial, Helvetica, sans-serif" font-weight="bold" font-size="{s}" '
            f'text-anchor="middle" fill="{fill}" stroke="{D}" stroke-width="{sw}" paint-order="stroke" '
            f'stroke-linejoin="round">{txt}</text>\n')


def star_pts(cx, cy, r, r2=None, n=5, rot=-90):
    r2 = r2 or r * 0.45
    pts = []
    for i in range(n * 2):
        a = math.radians(rot + i * 180 / n)
        rr = r if i % 2 == 0 else r2
        pts.append((round(cx + rr * math.cos(a), 1), round(cy + rr * math.sin(a), 1)))
    return pts


def win(x, y, w, h, glass=GLASS, sw=10, bars=True, frame=None):
    s = R(x, y, w, h, glass, sw)
    s += F(f'M{x + 8} {y + h * 0.55} L{x + w * 0.55} {y + 8} L{x + w * 0.8} {y + 8} L{x + 8} {y + h * 0.8} Z', '#ffffff', 0.35)
    if bars:
        s += L(x + w / 2, y, x + w / 2, y + h, 8) + L(x, y + h / 2, x + w, y + h / 2, 8)
    return s


def tree(x, y, s=1.0, c=OLIVE):
    inner = R(-14, -120, 28, 122, WOOD, 10)
    inner += P('M-80 -120 Q-104 -180 -52 -212 Q-24 -262 30 -232 Q92 -222 82 -160 Q104 -110 50 -100 L-50 -100 Q-92 -96 -80 -120 Z', c, 12)
    inner += F('M-60 -150 Q-66 -190 -30 -200 Q-10 -226 16 -214 Q-30 -200 -44 -150 Z', '#ffffff', 0.3)
    return G(x, y, s, inner)


def bush(x, y, s=1.0, c=LEAF):
    inner = P('M-62 0 Q-74 -40 -32 -46 Q-12 -78 22 -56 Q64 -62 62 -22 Q72 0 52 0 Z', c, 10)
    inner += F('M-44 -20 Q-46 -38 -24 -38 Q-8 -60 10 -50 Q-20 -40 -30 -18 Z', '#ffffff', 0.3)
    return G(x, y, s, inner)


def cloud(x, y, s=1.0):
    inner = P('M-90 20 Q-110 -12 -70 -22 Q-60 -62 -14 -52 Q10 -86 50 -58 Q96 -60 90 -18 Q116 6 86 22 Z', '#ffffff', 10)
    return G(x, y, s, inner)


def sun(x, y, r=50):
    s = ''
    for i in range(8):
        a = math.radians(i * 45)
        s += L(x + (r + 16) * math.cos(a), y + (r + 16) * math.sin(a), x + (r + 40) * math.cos(a), y + (r + 40) * math.sin(a), 10, D)
    s += C(x, y, r, MUST, 10) + F(f'M{x - r * 0.6} {y - r * 0.1} A{r * 0.65} {r * 0.65} 0 0 1 {x + r * 0.1} {y - r * 0.6} L{x - r * 0.2} {y - r * 0.2} Z', '#ffffff', 0.35)
    return s


# ---------------------------------------------------------------- people
def person(x, y, s=1.0, shirt=BLUE, skin=SKIN, hair=HAIR, pants='#4a5a78', vest=None, hat='', arms='down',
           long_hair=False, extra='', skirt=None):
    o = ''
    if long_hair:
        o += P('M-54 -238 Q-60 -300 0 -298 Q60 -300 54 -238 L58 -170 L-58 -170 Z', hair, 8)
    if skirt:
        o += R(-26, -60, 20, 60, skin, 8) + R(6, -60, 20, 60, skin, 8)
    else:
        o += R(-32, -95, 26, 95, pants, 10) + R(6, -95, 26, 95, pants, 10)
    o += E(-22, -4, 22, 12, HAIR, 8) + E(22, -4, 22, 12, HAIR, 8)
    if arms == 'down':
        o += TP('M-42 -168 Q-66 -130 -62 -96', 20, shirt) + TP('M42 -168 Q66 -130 62 -96', 20, shirt)
        o += C(-62, -90, 13, skin, 8) + C(62, -90, 13, skin, 8)
    elif arms == 'wave':
        o += TP('M-42 -168 Q-66 -130 -62 -96', 20, shirt) + TP('M42 -165 Q80 -190 86 -250', 20, shirt)
        o += C(-62, -90, 13, skin, 8) + C(88, -262, 15, skin, 8)
    elif arms == 'front':
        o += TP('M-42 -168 Q-60 -130 -30 -118', 20, shirt) + TP('M42 -168 Q60 -130 30 -118', 20, shirt)
    if skirt:
        o += P('M-48 -90 L-44 -172 Q0 -192 44 -172 L48 -90 Z', shirt, 10)
        o += P('M-50 -100 L50 -100 L64 -50 L-64 -50 Z', skirt, 10)
    else:
        o += P('M-48 -90 L-44 -172 Q0 -192 44 -172 L48 -90 Z', shirt, 10)
    if vest:
        o += P('M-44 -170 L-16 -182 L-10 -92 L-47 -92 Z', vest, 8) + P('M44 -170 L16 -182 L10 -92 L47 -92 Z', vest, 8)
        o += L(-44, -128, -12, -128, 7, '#f6ecd8') + L(44, -128, 12, -128, 7, '#f6ecd8')
    if arms == 'front':
        o += C(-24, -116, 13, skin, 8) + C(24, -116, 13, skin, 8)
    o += R(-12, -196, 24, 22, skin, 8)
    o += C(0, -236, 48, skin, 10)
    o += P('M-49 -240 Q-52 -292 0 -292 Q52 -292 49 -240 Q30 -266 0 -264 Q-30 -266 -49 -240 Z', hair, 8)
    o += C(-17, -232, 6, D, 0) + C(17, -232, 6, D, 0)
    o += C(-30, -214, 10, '#e8907f', 0, 'opacity="0.5"') + C(30, -214, 10, '#e8907f', 0, 'opacity="0.5"')
    o += P('M-12 -214 Q0 -203 12 -214', 'none', 6)
    o += hat + extra
    return G(x, y, s, o)


def bust(x, y, s=1.0, shirt=BLUE, skin=SKIN, hair=HAIR, hat='', collar=None):
    """Head and shoulders, bottom at y."""
    o = P('M-80 0 Q-80 -80 0 -86 Q80 -80 80 0 Z', shirt, 10)
    if collar:
        o += P('M-22 -84 L0 -50 L22 -84', collar, 7)
    o += R(-14, -110, 28, 28, skin, 8)
    o += C(0, -150, 48, skin, 10)
    o += P('M-49 -154 Q-52 -206 0 -206 Q52 -206 49 -154 Q30 -180 0 -178 Q-30 -180 -49 -154 Z', hair, 8)
    o += C(-17, -146, 6, D, 0) + C(17, -146, 6, D, 0)
    o += C(-30, -128, 10, '#e8907f', 0, 'opacity="0.5"') + C(30, -128, 10, '#e8907f', 0, 'opacity="0.5"')
    o += P('M-12 -128 Q0 -117 12 -128', 'none', 6)
    o += hat
    return G(x, y, s, o)


# ---------------------------------------------------------------- pictograms (fit in +-50)
def p_cross(c=RED):
    return P('M-16 -46 H16 V-16 H46 V16 H16 V46 H-16 V16 H-46 V-16 H-16 Z', c, 8) + F('M-10 -40 H6 V-8 H-10 Z', '#fff', 0.3)


def p_tooth():
    return (P('M-36 -28 C-36 -50 -10 -50 0 -38 C10 -50 36 -50 36 -28 C36 -4 28 12 22 40 C18 52 6 52 4 36 L0 16 L-4 36 C-6 52 -18 52 -22 40 C-28 12 -36 -4 -36 -28 Z', '#ffffff', 8)
            + F('M-26 -28 C-26 -38 -14 -40 -8 -32 C-18 -30 -20 -18 -22 -6 Z', '#9fb3c1', 0.5))


def p_scissors():
    o = TL(-18, 26, 30, -44, 9, GREY, 6) + TL(18, 26, -30, -44, 9, GREY, 6)
    for cx in (-22, 22):
        o += f'<circle cx="{cx}" cy="{32}" r="13" fill="none" stroke="{D}" stroke-width="16"/>\n'
        o += f'<circle cx="{cx}" cy="{32}" r="13" fill="none" stroke="{RED}" stroke-width="7"/>\n'
    o += C(0, 0, 4, D, 0)
    return o


def p_pill():
    o = P('M0 -18 H-28 A18 18 0 0 0 -28 18 H0 Z', RED, 0) + P('M0 -18 H28 A18 18 0 0 1 28 18 H0 Z', '#ffffff', 0)
    o += R(-46, -18, 92, 36, 'none', 8, rx=18) + L(0, -18, 0, 18, 7)
    o += F('M-36 -8 H-8 V-2 H-36 Z', '#fff', 0.45)
    return G(0, 0, 1, o, -35)


def p_env():
    return R(-46, -32, 92, 64, CREAM, 8, rx=6) + P('M-44 -28 L0 6 L44 -28', 'none', 7) + P('M-44 30 L-12 -2 M44 30 L12 -2', 'none', 6)


def p_coin():
    return C(0, 0, 46, MUST, 8) + C(0, 0, 34, 'none', 5) + T(0, 22, 58, '฿', CREAM, 6) + F('M-34 -10 A36 36 0 0 1 -10 -34 L-8 -26 A28 28 0 0 0 -26 -8 Z', '#fff', 0.4)


def p_plane():
    d = ('M0 -48 C8 -48 8 -40 8 -30 L8 -8 L46 12 L46 22 L8 12 L6 32 L18 40 L18 47 L0 43 L-18 47 L-18 40 L-6 32 '
         'L-8 12 L-46 22 L-46 12 L-8 -8 L-8 -30 C-8 -40 -8 -48 0 -48 Z')
    return G(0, 0, 1, P(d, BLUE, 7), 45)


def p_bread():
    o = P('M-46 12 C-46 -30 46 -30 46 12 L46 26 Q46 32 40 32 L-40 32 Q-46 32 -46 26 Z', '#d9a05b', 8)
    o += P('M-24 -4 L-14 -16 M-4 -4 L6 -16 M16 -4 L26 -16', 'none', 6)
    o += F('M-38 8 C-38 -16 -10 -22 10 -20 C-14 -12 -26 0 -30 14 Z', '#fff', 0.35)
    return o


def p_book():
    o = P('M-54 -24 L-54 38 Q-26 28 0 42 Q26 28 54 38 L54 -24 Z', RED, 7)
    o += P('M0 -26 Q-24 -40 -48 -30 L-48 30 Q-24 20 0 34 Z', CREAM, 7) + P('M0 -26 Q24 -40 48 -30 L48 30 Q24 20 0 34 Z', CREAM, 7)
    o += P('M-38 -16 Q-22 -22 -10 -14 M-38 -2 Q-22 -8 -10 0 M-38 12 Q-22 6 -10 14 M10 -14 Q22 -22 38 -16 M10 0 Q22 -8 38 -2 M10 14 Q22 6 38 12', 'none', 4)
    return o


def p_camera():
    o = R(-24, -40, 40, 18, DBROWN, 7, rx=4)
    o += R(-48, -26, 96, 64, DBROWN, 7, rx=12)
    o += C(0, 6, 25, GREY, 7) + C(0, 6, 13, BLUE, 5) + C(-4, 2, 4, '#fff', 0)
    o += R(26, -18, 14, 9, MUST, 4) + FR(-42, -20, 84, 8, '#fff', 0.25)
    return o


def p_cup():
    o = TP('M30 -4 Q50 -6 46 10 Q42 22 26 22', 6, CREAM, 5)
    o += E(0, 42, 48, 9, CREAM, 6)
    o += P('M-34 -16 L34 -16 L28 30 Q26 40 16 40 L-16 40 Q-26 40 -28 30 Z', CREAM, 7)
    o += P('M-32 0 L32 0 L30 14 L-30 14 Z', WOOD, 0)
    o += P('M-12 -26 Q-20 -36 -12 -44 M4 -26 Q-4 -36 4 -46 M20 -26 Q12 -36 20 -44', 'none', 5)
    return o


def p_forkknife():
    o = C(0, 0, 32, CREAM, 7) + C(0, 0, 20, 'none', 4)
    o += P('M-50 -40 L-50 -14 Q-50 -6 -44 -4 L-44 44 M-38 -40 L-38 -14 Q-38 -6 -44 -4', 'none', 6)
    o += P('M44 44 L44 -40 Q54 -28 52 -4 L44 -2', 'none', 6)
    return o


def p_cart():
    o = P('M-28 -16 L46 -16 L36 18 L-20 18 Z', BLUE, 7)
    o += C(-6, -26, 10, RED, 5) + C(14, -28, 10, OLIVE, 5) + C(32, -24, 9, MUST, 5)
    o += P('M-28 -16 L46 -16 L36 18 L-20 18 Z', BLUE, 7)
    o += P('M-50 -30 L-36 -30 L-20 18 L-24 28 L38 28', 'none', 7)
    o += C(-16, 38, 7, D, 0) + C(32, 38, 7, D, 0)
    return o


def p_bag():
    o = P('M-18 -16 Q-18 -46 0 -46 Q18 -46 18 -16', 'none', 7)
    o += P('M-36 -18 L36 -18 L42 44 L-42 44 Z', TERRA, 7) + F('M-30 -12 L-18 -12 L-22 38 L-34 38 Z', '#fff', 0.3)
    return o


def p_bed():
    o = R(-50, -34, 14, 70, WOOD, 6, rx=4) + R(-44, 0, 92, 20, CREAM, 6, rx=5)
    o += E(-24, -8, 14, 9, '#ffffff', 5) + P('M-6 -12 L44 -12 Q50 -12 50 -4 L50 4 L-6 4 Z', BLUE, 6)
    o += L(-40, 22, -40, 34, 6) + L(42, 22, 42, 34, 6)
    return o


def p_shield(c=BLUE):
    return P('M0 -48 L40 -34 Q40 20 0 48 Q-40 20 -40 -34 Z', c, 7) + PG(star_pts(0, -2, 22), MUST, 5)


def p_nails():
    o = ''
    fing = [(-30, -20), (-12, -34), (6, -38), (24, -30)]
    for fx, fy in fing:
        o += R(fx - 8, fy, 18, 60, SKIN, 6, rx=9) + R(fx - 4, fy + 4, 10, 14, '#c8577e', 4, rx=5)
    o += P('M-42 10 Q-44 50 0 50 Q40 50 42 20 L42 10 Z', SKIN, 7)
    o += P('M34 22 Q50 4 58 12 Q60 24 44 40', SKIN, 7) + R(49, 6, 9, 11, '#c8577e', 3, rx=4)
    return o


def p_lotus():
    o = P('M0 30 Q-40 20 -46 -10 Q-20 -10 0 30 Z', '#e8a0b0', 6) + P('M0 30 Q40 20 46 -10 Q20 -10 0 30 Z', '#e8a0b0', 6)
    o += P('M0 30 Q-30 0 -22 -30 Q-2 -14 0 30 Z', '#f1b8c4', 6) + P('M0 30 Q30 0 22 -30 Q2 -14 0 30 Z', '#f1b8c4', 6)
    o += P('M0 30 Q-16 -6 0 -46 Q16 -6 0 30 Z', '#f6ccd4', 6)
    o += P('M-40 36 Q0 26 40 36', 'none', 7)
    return o


def p_dryer():
    o = TL(-6, 10, 2, 44, 12, PLUM, 5)
    o += P('M-40 -24 L20 -30 L20 14 L-40 8 Z', PLUM, 7) + C(-34, -8, 22, PLUM, 7) + C(-34, -8, 9, CREAM, 5)
    o += R(20, -34, 12, 52, DBROWN, 6, rx=3)
    o += P('M38 -26 H52 M40 -8 H56 M38 10 H52', 'none', 5)
    return o


def p_mustache():
    return P('M0 -6 Q-18 -24 -34 -8 Q-44 4 -56 -4 Q-52 22 -28 16 Q-12 12 0 2 Q12 12 28 16 Q52 22 56 -4 Q44 4 34 -8 Q18 -24 0 -6 Z', HAIR, 7)


def p_cap():
    o = P('M-28 4 L-28 26 Q0 40 28 26 L28 4', DBROWN, 7)
    o += PG([(0, -30), (54, -8), (0, 14), (-54, -8)], '#3b3a48', 7)
    o += P('M0 -8 L40 4 L40 30', 'none', 5) + C(40, 34, 6, MUST, 4)
    return o


def p_wrench():
    d = 'M-8 -14 L-8 42 Q0 50 8 42 L8 -14 Q30 -22 28 -40 L14 -34 L6 -46 L14 -54 Q-10 -60 -24 -44 Q-30 -26 -8 -14 Z'
    return G(0, 4, 1, P(d, GREY, 7), 40)


def p_spool():
    o = R(-24, -40, 48, 12, WOOD, 6, rx=3) + R(-24, 28, 48, 12, WOOD, 6, rx=3)
    o += R(-18, -28, 36, 56, RED, 6) + P('M-18 -18 L18 -10 M-18 -4 L18 4 M-18 10 L18 18', 'none', 3)
    o += TL(30, 44, 50, -40, 4, GREY, 4) + P('M18 20 Q34 30 38 12 Q42 -10 48 -26', 'none', 3)
    return o


def p_briefcase():
    o = R(-14, -40, 28, 16, 'none', 7, rx=5)
    o += R(-46, -26, 92, 64, WOOD, 7, rx=8) + L(-46, 0, 46, 0, 5) + R(-8, -6, 16, 12, MUST, 4, rx=2)
    return o


def p_queue():
    o = ''
    for i, x in enumerate((-34, -4, 26)):
        o += C(x, -22, 10, '#ffffff', 5) + P(f'M{x - 12} 30 L{x - 12} 2 Q{x} -8 {x + 12} 2 L{x + 12} 30 Z', '#ffffff', 5)
    o += P('M34 36 L50 36', 'none', 0)
    return o


# ---------------------------------------------------------------- building blocks
def awning(x0, x1, y0, y1, c1, c2, n=8, inset=30):
    w = (x1 - x0) / n
    tw = (x1 - x0 - 2 * inset) / n
    o = ''
    for i in range(n):
        o += C(round(x0 + w * (i + .5), 1), y1, round(w / 2, 1), c1 if i % 2 == 0 else c2, 10)
    for i in range(n):
        bx0 = x0 + w * i
        tx0 = x0 + inset + tw * i
        o += PG([(round(tx0, 1), y0), (round(tx0 + tw, 1), y0), (round(bx0 + w, 1), y1), (round(bx0, 1), y1)],
                c1 if i % 2 == 0 else c2, 0)
    o += FR(x0 + inset, y0, x1 - x0 - 2 * inset, (y1 - y0) * 0.35, '#fff', 0.25)
    o += PG([(x0 + inset, y0), (x1 - inset, y0), (x1, y1), (x0, y1)], 'none', 12)
    return o


def sign_badge(cx, cy, r, picto, s, fill=CREAM, post=True, ring=None):
    o = ''
    if post:
        o += R(cx - r * 0.55, cy + r * 0.6, 16, r * 0.8, DBROWN, 8) + R(cx + r * 0.55 - 16, cy + r * 0.6, 16, r * 0.8, DBROWN, 8)
    o += C(cx, cy, r, fill, 14)
    if ring:
        o += f'<circle cx="{cx}" cy="{cy}" r="{r - 16}" fill="none" stroke="{ring}" stroke-width="6"/>\n'
    o += G(cx, cy, s, picto)
    o += F(f'M{cx - r * 0.75} {cy - r * 0.2} A{r * 0.8} {r * 0.8} 0 0 1 {cx - r * 0.1} {cy - r * 0.78} L{cx - r * 0.12} {cy - r * 0.62} A{r * 0.64} {r * 0.64} 0 0 0 {cx - r * 0.6} {cy - r * 0.16} Z', '#fff', 0.35)
    return o


def shop(bg, wall, aw, picto, window_inner='', door=WOOD, sign_fill=CREAM, extra_front='', extra_back='',
         roof=DBROWN, winb=(240, 500, 330, 300), doorb=(620, 510, 160, 350), extra_wall='', pscale=1.45, glass='#d7eaee'):
    o = shadow(512, 866, 390, 32) + extra_back
    o += sign_badge(512, 200, 100, picto, pscale, sign_fill)
    o += R(200, 330, 624, 530, wall, 14)
    o += FR(760, 337, 57, 516, D, 0.12)
    o += R(180, 296, 664, 50, roof, 14, rx=8) + FR(188, 304, 648, 12, '#fff', 0.25)
    o += extra_wall
    x, y, w, h = winb
    o += R(x, y, w, h, glass, 12)
    o += window_inner
    o += F(f'M{x + 10} {y + h * 0.45} L{x + w * 0.4} {y + 10} L{x + w * 0.55} {y + 10} L{x + 10} {y + h * 0.62} Z', '#fff', 0.3)
    o += R(x, y, w, h, 'none', 12)
    o += R(x - 14, y + h, w + 28, 22, roof, 10, rx=4)
    dx, dy, dw, dh = doorb
    o += R(dx, dy, dw, dh, door, 12) + R(dx + 24, dy + 30, dw - 48, dh * 0.4, glass, 8)
    o += F(f'M{dx + 30} {dy + 110} L{dx + 80} {dy + 36} L{dx + 100} {dy + 36} L{dx + 30} {dy + 140} Z', '#fff', 0.35)
    o += C(dx + dw - 26, dy + dh * 0.58, 9, MUST, 6)
    if aw:
        o += awning(186, 838, 372, 452, aw[0], aw[1])
    o += extra_front
    return svg(bg, o)


def room(bg, wall, floor, floor_y, content, x0=150, y0=160, x1=874, y1=870, rid='room'):
    w, h = x1 - x0, y1 - y0
    o = f'<defs><clipPath id="{rid}"><rect x="{x0}" y="{y0}" width="{w}" height="{h}" rx="40"/></clipPath></defs>\n'
    o += shadow(512, y1 + 6, w / 2 + 10, 26)
    o += f'<g clip-path="url(#{rid})">\n'
    o += R(x0, y0, w, h, wall, 0)
    o += R(x0, floor_y, w, y1 - floor_y, floor, 0) + L(x0, floor_y, x1, floor_y, 12)
    o += content
    o += '</g>\n'
    o += R(x0, y0, w, h, 'none', 16, rx=40)
    return svg(bg, o)


# ---------------------------------------------------------------- vehicles (origin: center bottom of wheels)
def wheel(x, y, r=32):
    return C(x, y, r, HAIR, 10) + C(x, y, r * 0.42, GREY, 6)


def car(color=RED, top=None, light=None):
    top = top or color
    o = P('M-150 -40 L-150 -86 Q-150 -100 -134 -100 L-86 -100 L-52 -150 L56 -150 L96 -100 L136 -94 Q152 -90 152 -72 L152 -40 Q152 -30 142 -30 L-140 -30 Q-150 -30 -150 -40 Z', color, 12)
    if top != color:
        o += P('M-86 -100 L-52 -150 L56 -150 L96 -100 Z', top, 10)
    o += P('M-40 -140 L-4 -140 L-4 -104 L-68 -104 Z', GLASS, 8) + P('M8 -140 L50 -140 L80 -104 L8 -104 Z', GLASS, 8)
    o += FR(-140, -84, 280, 12, '#fff', 0.3)
    o += R(132, -84, 18, 14, MUST, 5, rx=4) + R(-150, -84, 12, 14, RED, 5, rx=3)
    o += wheel(-86, -30) + wheel(90, -30)
    if light:
        o += light
    return o


def van(color='#e4e6e2', stripe=BLUE):
    o = P('M-170 -44 L-170 -176 Q-170 -196 -150 -196 L84 -196 Q98 -196 108 -186 L160 -120 L172 -112 Q180 -106 180 -92 L180 -44 Q180 -32 168 -32 L-158 -32 Q-170 -32 -170 -44 Z', color, 12)
    o += FR(-166, -100, 344, 18, stripe, 0.9)
    for x in (-150, -84, -18):
        o += R(x, -178, 56, 52, GLASS, 8, rx=6)
    o += P('M50 -178 L96 -178 L140 -124 L50 -124 Z', GLASS, 8)
    o += L(40, -178, 40, -44, 6) + R(166, -86, 14, 14, MUST, 5, rx=4)
    o += FR(-160, -190, 240, 10, '#fff', 0.4)
    o += wheel(-104, -32) + wheel(112, -32)
    return o


def bus(color=MUST):
    o = R(-220, -230, 440, 196, color, 12, rx=24)
    for x in (-196, -126, -56, 14, 84):
        o += R(x, -206, 58, 64, GLASS, 8, rx=6)
    o += R(160, -206, 44, 150, GLASS, 8, rx=6)
    o += FR(-216, -128, 432, 16, RED, 0.8) + FR(-200, -224, 400, 10, '#fff', 0.35)
    o += R(196, -58, 18, 14, CREAM, 5, rx=4)
    o += wheel(-130, -34, 34) + wheel(130, -34, 34)
    return o


def songthaew():
    c = RED
    o = R(-196, -186, 246, 26, DBROWN, 10, rx=6)
    o += R(-186, -164, 232, 124, c, 12)
    o += R(-170, -150, 88, 60, '#4a2e24', 8, rx=6) + R(-68, -150, 88, 60, '#4a2e24', 8, rx=6)
    o += bust(-126, -98, 0.36, OLIVE, SKIN2) + bust(-24, -98, 0.36, MUST, SKIN)
    o += R(-176, -104, 206, 14, DBROWN, 6)
    o += P('M46 -40 L46 -166 L112 -166 L150 -112 L184 -106 Q196 -102 196 -86 L196 -40 Z', c, 12)
    o += P('M60 -154 L106 -154 L136 -114 L60 -114 Z', GLASS, 8)
    o += FR(-180, -80, 370, 12, '#fff', 0.25) + FR(-180, -60, 228, 10, CREAM, 0.7)
    o += R(-196, -52, 26, 18, DBROWN, 6, rx=3)
    o += R(182, -96, 14, 14, MUST, 5, rx=4)
    o += wheel(-120, -34) + wheel(128, -34)
    return o


def motorbike(color=BLUE):
    o = wheel(-80, -36, 36) + wheel(86, -36, 36)
    o += P('M-100 -66 Q-96 -110 -40 -110 L10 -110 L30 -60 L60 -60 L70 -130 L90 -130', 'none', 0)
    o += TP('M58 -128 L86 -36', 10, GREY, 6)
    o += P('M-116 -80 Q-110 -118 -50 -118 L-4 -118 Q10 -118 12 -104 L20 -64 L-120 -64 Q-124 -70 -116 -80 Z', color, 10)
    o += P('M12 -80 L62 -80 L74 -150 L50 -150 Z', color, 10)
    o += R(-100, -136, 90, 22, HAIR, 8, rx=10)
    o += TP('M56 -160 L92 -166', 8, HAIR, 5) + C(84, -140, 10, MUST, 5)
    return o

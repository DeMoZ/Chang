"""Places: transport stands, interiors, outdoor scenes."""
import math
from places_lib import *

S = {}
ORANGE = '#e0843c'


def stand_sign(x, yg, icon, top=300, c=BLUE, r=72):
    o = R(x - 9, top, 18, yg - top, GREY, 8) + E(x, yg, 40, 10, GREY, 6)
    o += C(x, top, r, c, 12) + C(x, top, r - 12, 'none', 0, '') + f'<circle cx="{x}" cy="{top}" r="{r - 13}" fill="none" stroke="#ffffff" stroke-width="6"/>\n'
    o += G(x, top, 1, icon)
    o += F(f'M{x - r * 0.7} {top - r * 0.2} A{r * 0.75} {r * 0.75} 0 0 1 {x - r * 0.1} {top - r * 0.72} L{x - r * 0.12} {top - r * 0.58} A{r * 0.6} {r * 0.6} 0 0 0 {x - r * 0.56} {top - r * 0.16} Z', '#fff', 0.35)
    return o


def canopy(x0, x1, top, c):
    o = R(x0 + 20, top + 40, 22, 860 - top - 40, GREY, 8) + R(x1 - 42, top + 40, 22, 860 - top - 40, GREY, 8)
    o += P(f'M{x0} {top + 50} L{x0 + 40} {top} L{x1 - 40} {top} L{x1} {top + 50} Z', c, 14)
    o += FR(x0 + 44, top + 8, x1 - x0 - 88, 12, '#fff', 0.3) + FR(x0 + 10, top + 38, x1 - x0 - 20, 10, D, 0.12)
    return o


def taxi_car():
    lt = R(-44, -176, 88, 28, MUST, 8, rx=6)
    return car('#d9829f', light=lt)


def bus_stop():
    o = shadow(540, 866, 360)
    o += stand_sign(210, 860, G(0, 24, 0.24, bus()))
    o += R(356, 412, 20, 450, GREY, 8) + R(760, 412, 20, 450, GREY, 8)
    o += R(376, 440, 384, 250, GLASS, 10) + F('M390 660 L520 450 L560 450 L420 680 Z', '#fff', 0.35)
    o += R(560, 470, 170, 190, CREAM, 8) + G(645, 565, 1.1, p_cup())
    o += P('M320 380 L820 380 L800 424 L340 424 Z', TEAL, 12) + FR(336, 386, 470, 10, '#fff', 0.3)
    o += R(396, 716, 344, 26, WOOD, 10, rx=6) + R(420, 742, 16, 118, GREY, 6) + R(700, 742, 16, 118, GREY, 6)
    o += person(470, 874, 0.62, OLIVE, SKIN2)
    o += person(830, 874, 0.55, PLUM, long_hair=True, arms='wave')
    return svg('sky', o)


S['Bus stop'] = bus_stop


def taxi_stand():
    o = shadow(560, 866, 360)
    o += canopy(340, 870, 330, TEAL) + stand_sign(190, 860, G(0, 30, 0.34, taxi_car()), top=280, c=TEAL, r=82)
    o += G(600, 850, 1.5, taxi_car())
    o += person(290, 874, 0.62, BLUE, arms='wave')
    return svg('sand', o)


S['Taxi stand'] = taxi_stand


def van_stand():
    o = shadow(540, 866, 370)
    o += canopy(340, 870, 330, BLUE) + stand_sign(190, 860, G(0, 30, 0.3, van()), top=280, c=BLUE, r=82)
    o += G(570, 850, 1.35, van())
    lug = R(56, -150, 80, 110, BLUE, 8, rx=10) + L(96, -150, 96, -176, 7) + L(80, -176, 112, -176, 7) + C(72, -34, 8, D, 0) + C(120, -34, 8, D, 0)
    o += person(280, 874, 0.62, TERRA, SKIN2, extra=lug)
    return svg('beige', o)


S['Van stand'] = van_stand


def minitruck_stand():
    o = shadow(560, 866, 370)
    o += canopy(340, 870, 330, OLIVE) + stand_sign(190, 860, G(0, 30, 0.3, songthaew()), top=280, c=OLIVE, r=82)
    o += G(585, 852, 1.42, songthaew())
    return svg('sage', o)


S['Minitruck stand'] = minitruck_stand


def moto_stand():
    o = shadow(512, 866, 390)
    o += R(250, 420, 20, 440, WOOD, 8) + R(754, 420, 20, 440, WOOD, 8)
    o += R(290, 660, 444, 22, WOOD, 8) + R(300, 682, 14, 90, WOOD, 6) + R(710, 682, 14, 90, WOOD, 6)
    o += person(420, 800, 0.72, '#e8e2d6', SKIN3, vest=ORANGE)
    o += person(610, 800, 0.72, BLUE, SKIN2, vest=ORANGE, arms='wave')
    o += P('M200 440 L300 330 L724 330 L824 440 Z', RED, 14) + P('M300 330 L512 250 L724 330 Z', ORANGE, 14)
    o += FR(214, 420, 596, 14, D, 0.12) + F('M310 340 L500 340 L480 360 L290 360 Z', '#fff', 0.25)
    o += R(452, 440, 120, 70, CREAM, 8, rx=8) + G(512, 490, 0.3, motorbike(ORANGE))
    o += L(480, 440, 480, 424, 6) + L(544, 440, 544, 424, 6)
    o += G(330, 866, 0.82, motorbike(TEAL)) + G(700, 866, 0.82, motorbike(RED))
    return svg('sand', o)


S['Motorcycle stand'] = moto_stand


def queue_stand():
    o = shadow(512, 870, 380)
    o += PG([(210, 880), (760, 720), (800, 740), (300, 900)], '#e9dfc9', 0, 'opacity="0.0"')
    people = [(680, 736, 0.48, PLUM, SKIN, True), (560, 776, 0.54, OLIVE, SKIN2, False), (430, 820, 0.6, MUST, SKIN, True), (290, 870, 0.68, BLUE, SKIN3, False)]
    posts = [(760, 700), (640, 740), (510, 784), (370, 830), (230, 876)]
    o += stand_sign(720, 700, p_queue(), top=360, c=TEAL, r=70)
    for i, (x, y) in enumerate(posts[1:]):
        px, py = posts[i]
        o += P(f'M{px} {py - 90} Q{(px + x) / 2} {(py + y) / 2 - 60} {x} {y - 90}', 'none', 0) + TP(f'M{px} {py - 90} Q{(px + x) / 2} {(py + y) / 2 - 60} {x} {y - 90}', 8, RED, 4)
    for x, y, s, sh, sk, lh in people:
        o += person(x, y, s, sh, sk, long_hair=lh)
    for x, y in posts:
        o += R(x - 6, y - 96, 12, 96, GREY, 5) + E(x, y, 20, 6, GREY, 5) + C(x, y - 98, 9, MUST, 5)
    for i, (x, y) in enumerate(posts[1:]):
        pass
    return svg('sand', '<g transform="translate(-102 -180) scale(1.2)">' + o + '</g>')


S['Transportation stand_Queue'] = queue_stand


def immigration():
    c = ''
    c += R(200, 200, 624, 80, '#9fb3c1', 0)
    cap = P('M-54 -186 Q-50 -234 0 -236 Q50 -234 54 -186 Z', '#3b4a6b', 8) + E(0, -186, 62, 11, HAIR, 7) + C(0, -212, 9, MUST, 5)
    c += bust(512, 560, 1.15, '#6f86a3', SKIN2, hat=cap, collar=CREAM)
    c += R(290, 250, 444, 300, GLASS, 0, extra='opacity="0.35"') + R(290, 250, 444, 300, 'none', 12) + F('M310 520 L440 270 L480 270 L350 540 Z', '#fff', 0.35)
    c += R(170, 540, 684, 40, WOOD, 12, rx=6) + R(190, 580, 644, 300, '#d9cdb6', 12)
    c += R(330, 620, 364, 220, PLUM, 12, rx=18)
    c += R(344, 632, 164, 196, CREAM, 8, rx=6) + R(516, 632, 164, 196, CREAM, 8, rx=6)
    c += R(366, 656, 66, 80, '#dfe9ee', 6) + C(399, 686, 18, SKIN, 5) + P('M375 736 Q375 710 399 710 Q423 710 423 736 Z', BLUE, 5)
    for y in (760, 784, 808):
        c += L(366, y, 486, y, 5)
    c += L(446, 670, 486, 670, 5) + L(446, 694, 486, 694, 5)
    c += f'<circle cx="590" cy="700" r="40" fill="none" stroke="{RED}" stroke-width="7"/>\n' + PG(star_pts(590, 700, 18), RED, 0)
    c += f'<rect x="540" y="760" width="110" height="46" rx="6" fill="none" stroke="{TEAL}" stroke-width="7" transform="rotate(-8 595 783)"/>\n'
    st_ = E(0, 0, 40, 12, D, 0, 'opacity="0.2"') + R(-56, -44, 112, 36, '#3b3a48', 10, rx=6) + R(-14, -110, 28, 70, WOOD, 10, rx=10) + C(0, -124, 30, RED, 10)
    c += G(700, 600, 1, st_, 12)
    return room('sky', '#e3ecf1', '#c9ccd0', 540, c)


S['Immigration'] = immigration


def workplace():
    c = ''
    c += R(200, 210, 200, 170, GLASS, 12)
    for y in range(230, 380, 24):
        c += L(206, y, 394, y, 5, '#ffffff')
    c += C(500, 270, 46, CREAM, 10) + L(500, 270, 500, 242, 7) + L(500, 270, 522, 282, 7)
    c += R(560, 330, 260, 16, WOOD, 7)
    for i, col in enumerate((RED, BLUE, MUST, OLIVE, TEAL)):
        c += R(580 + i * 34, 266, 28, 64, col, 6, rx=3)
    c += R(410, 560, 28, 110, GREY, 8) + R(360, 650, 128, 20, GREY, 8, rx=6)
    c += R(330, 390, 300, 190, '#3b4a55', 12, rx=10) + R(346, 406, 268, 150, '#dff0f4', 8)
    c += R(376, 506, 30, 40, TEAL, 5) + R(420, 476, 30, 70, MUST, 5) + R(464, 446, 30, 100, RED, 5)
    c += P('M520 530 L560 470 L590 490', 'none', 6)
    c += R(460, 580, 40, 70, GREY, 8) + R(420, 640, 120, 20, GREY, 7, rx=6)
    c += R(190, 660, 644, 34, WOOD, 12, rx=6) + R(210, 694, 180, 180, '#b88a62', 12) + R(760, 694, 26, 180, '#b88a62', 10)
    c += L(230, 760, 370, 760, 6) + C(300, 730, 7, D, 0) + C(300, 810, 7, D, 0)
    c += R(560, 630, 180, 26, '#3b4a55', 8, rx=4)
    c += G(660, 628, 0.001, '')
    c += G(750, 640, 0.6, p_cup())
    c += R(250, 626, 110, 16, '#ffffff', 5) + R(256, 612, 110, 16, '#ffffff', 5, extra='transform="rotate(-4 300 620)"')
    c += G(800, 660, 1, P('M-34 0 L34 0 L26 -60 L-26 -60 Z', TERRA, 8)) + bush(800, 600, 0.7)
    return room('beige', '#efe4d2', '#b98a5e', 740, c)


S['Workplace'] = workplace


def bedroom():
    c = ''
    c += R(420, 220, 184, 150, '#3b4a6b', 12) + L(512, 220, 512, 370, 8) + L(420, 295, 604, 295, 8)
    c += P('M452 262 A22 22 0 1 0 480 240 A18 18 0 1 1 452 262 Z', '#f6ecc0', 5) + PG(star_pts(560, 250, 10), '#f6ecc0', 3) + PG(star_pts(572, 334, 8), '#f6ecc0', 3)
    c += R(310, 420, 404, 230, WOOD, 14, rx=40) + FR(330, 440, 364, 20, '#fff', 0.2)
    c += R(284, 600, 456, 130, CREAM, 12, rx=16)
    c += E(410, 588, 70, 34, '#ffffff', 10) + E(612, 588, 70, 34, '#ffffff', 10)
    c += P('M280 640 Q512 600 744 640 L744 760 Q512 780 280 760 Z', '#8fa9c9', 12) + P('M280 640 Q512 600 744 640 L744 680 Q512 650 280 680 Z', '#ffffff', 10)
    c += R(296, 760, 18, 50, WOOD, 8) + R(710, 760, 18, 50, WOOD, 8)
    c += R(160, 610, 110, 150, '#b88a62', 12, rx=6) + L(160, 686, 270, 686, 7) + C(215, 650, 6, D, 0) + C(215, 724, 6, D, 0)
    c += R(206, 560, 18, 50, GREY, 6) + P('M170 560 L260 560 L240 500 L190 500 Z', MUST, 10)
    c += E(512, 830, 200, 30, TERRA, 8, 'opacity="0.9"')
    return room('rose', '#f1d9c9', '#c9975f', 760, c)


S['Bedroom'] = bedroom


def kitchen():
    c = ''
    for x in range(150, 874, 60):
        c += L(x, 360, x, 520, 3, '#c9bfae')
    for y in range(380, 520, 40):
        c += L(150, y, 874, y, 3, '#c9bfae')
    c += R(160, 190, 300, 150, TEAL, 12, rx=6) + L(310, 190, 310, 340, 8) + R(288, 290, 10, 30, D, 0) + R(322, 290, 10, 30, D, 0)
    c += P('M540 190 L700 190 L700 280 L760 340 L480 340 L540 280 Z', GREY, 12)
    c += L(200, 380, 440, 380, 8)
    for x, shape in ((230, 'ladle'), (300, 'spat'), (370, 'whisk')):
        c += L(x, 380, x, 450, 7)
        if shape == 'ladle':
            c += P(f'M{x - 20} 450 Q{x - 20} 480 {x} 480 Q{x + 20} 480 {x + 20} 450 Z', GREY, 7)
        elif shape == 'spat':
            c += R(x - 16, 440, 32, 44, WOOD, 7, rx=6)
        else:
            c += E(x, 470, 16, 26, 'none', 6)
    c += R(560, 440, 160, 80, GREY, 10, rx=12) + R(546, 452, 20, 12, DBROWN, 6) + R(714, 452, 20, 12, DBROWN, 6)
    c += E(640, 440, 86, 14, '#b9c0c6', 8) + C(640, 424, 9, DBROWN, 6) + FR(574, 460, 20, 50, '#fff', 0.35)
    c += P('M600 400 Q588 380 600 360 Q612 340 600 320 M640 396 Q628 376 640 356 Q652 336 640 316 M680 400 Q668 380 680 360 Q692 340 680 320', 'none', 7)
    c += R(150, 520, 724, 30, WOOD, 12)
    c += R(150, 550, 724, 330, CREAM, 12)
    for x in (150, 330, 510, 690):
        c += R(x + 14, 574, 152, 150, '#e9dcc2', 8, rx=6) + R(x + 80, 590, 20, 8, D, 0)
    c += R(530, 574, 200, 150, '#3b3a3a', 8, rx=6) + R(550, 594, 160, 90, '#5a5a5a', 6, rx=4)
    c += R(280, 490, 130, 30, '#e0c79a', 8, rx=6) + C(320, 478, 18, RED, 6) + P('M350 500 L400 468 L408 478 L360 504 Z', OLIVE, 6)
    c += E(560, 520, 60, 6, '#3b3a3a', 0) + E(720, 520, 60, 6, '#3b3a3a', 0)
    return room('beige', '#efe6d4', '#b9a58a', 760, c)


S['Kitchen'] = kitchen


def living():
    c = ''
    c += R(390, 220, 250, 170, WOOD, 12) + R(406, 236, 218, 138, '#bfe0f0', 6) + P('M406 374 L470 300 L520 340 L570 290 L624 374 Z', OLIVE, 6) + C(590, 262, 14, MUST, 5)
    c += R(194, 360, 14, 420, DBROWN, 6) + P('M150 380 L250 380 L230 290 L170 290 Z', MUST, 10) + E(200, 784, 40, 10, DBROWN, 6)
    c += R(290, 470, 444, 170, RED, 14, rx=40)
    c += R(360, 480, 110, 90, '#e0b04f', 10, rx=20) + R(556, 480, 110, 90, TEAL, 10, rx=20)
    c += R(270, 580, 484, 110, RED, 14, rx=24) + L(512, 590, 512, 680, 7)
    c += R(236, 530, 90, 180, '#b04a40', 14, rx=36) + R(698, 530, 90, 180, '#b04a40', 14, rx=36)
    c += R(260, 706, 22, 40, DBROWN, 8) + R(742, 706, 22, 40, DBROWN, 8)
    c += FR(300, 596, 420, 12, '#fff', 0.2)
    c += E(512, 830, 260, 34, '#e3c9a0', 8)
    c += R(380, 770, 264, 22, WOOD, 10, rx=6) + R(400, 792, 16, 60, WOOD, 7) + R(608, 792, 16, 60, WOOD, 7)
    c += G(560, 768, 0.5, p_cup())
    c += R(810, 680, 70, 80, TERRA, 10) + P('M845 680 Q800 620 820 560 Q845 610 845 680 Q860 600 900 580 Q880 640 845 680 Z', LEAF, 8)
    return room('sage', '#e6dcc8', '#b98a5e', 760, c)


S['Living room'] = living


def restroom():
    c = ''
    for x in range(150, 874, 56):
        c += L(x, 160, x, 740, 3, '#b8cfd3')
    for y in range(190, 740, 56):
        c += L(150, y, 874, y, 3, '#b8cfd3')
    c += R(620, 210, 200, 120, BLUE, 12, rx=16) + L(720, 224, 720, 316, 6, '#ffffff')
    c += C(670, 238, 12, '#ffffff', 0) + P('M652 310 L652 262 Q670 252 688 262 L688 310 Z', '#ffffff', 0)
    c += C(770, 238, 12, '#ffffff', 0) + P('M770 256 L794 300 L746 300 Z', '#ffffff', 0) + R(758, 298, 8, 14, '#ffffff', 0) + R(774, 298, 8, 14, '#ffffff', 0)
    c += R(200, 250, 170, 200, '#dff0f4', 12, rx=80) + F('M230 330 L310 270 L330 280 L240 360 Z', '#fff', 0.7)
    c += R(250, 560, 70, 200, '#ffffff', 12) + P('M180 500 L390 500 Q390 580 285 580 Q180 580 180 500 Z', '#ffffff', 12)
    c += R(276, 470, 18, 34, GREY, 6) + P('M285 470 L285 456 L306 456', 'none', 7)
    c += R(480, 430, 200, 150, '#ffffff', 12, rx=16) + R(566, 450, 30, 12, GREY, 5, rx=4)
    c += P('M500 610 Q500 780 580 800 Q660 780 660 610 Z', '#ffffff', 12) + R(546, 780, 68, 60, '#ffffff', 12)
    c += E(580, 600, 110, 30, '#f4f4f0', 12) + E(580, 600, 76, 16, '#dff0f4', 7)
    c += FR(496, 440, 20, 120, '#9fb3c1', 0.3)
    c += R(760, 480, 60, 16, GREY, 6) + R(770, 496, 40, 70, '#ffffff', 8, rx=6)
    return room('sky', '#dcebec', '#c9ccd0', 740, c)


S['Restroom'] = restroom


def ball(x, y, r=40):
    o = C(x, y, r, '#ffffff', 10) + PG([(x + r * 0.4 * math.cos(math.radians(-90 + i * 72)), y + r * 0.4 * math.sin(math.radians(-90 + i * 72))) for i in range(5)], D, 0)
    for i in range(5):
        a = math.radians(-90 + i * 72)
        o += L(x + r * 0.4 * math.cos(a), y + r * 0.4 * math.sin(a), x + r * 0.85 * math.cos(a), y + r * 0.85 * math.sin(a), 5)
    return o


def lerp(a, b, t):
    return a + (b - a) * t


def sport():
    o = shadow(512, 862, 380, 30)
    tl, tr, br, bl = (280, 380), (744, 380), (874 - 20, 850), (150 + 20, 850)
    o += PG([tl, tr, br, bl], '#7fb05a', 14)
    for i in range(8):
        if i % 2:
            t0, t1 = i / 8, (i + 1) / 8
            y0, y1 = lerp(380, 850, t0), lerp(380, 850, t1)
            o += PG([(lerp(280, 170, t0), y0), (lerp(744, 854, t0), y0), (lerp(744, 854, t1), y1), (lerp(280, 170, t1), y1)], '#6b9e4c', 0)
    wl = '#ffffff'
    o += PG([(300, 400), (724, 400), (824, 830), (200, 830)], 'none', 0, f'stroke="{wl}" stroke-width="7"'.replace('stroke=', 'stroke=', 1)).replace('stroke="none"', '')
    o += f'<line x1="250" y1="590" x2="774" y2="590" stroke="{wl}" stroke-width="7"/>\n'
    o += f'<ellipse cx="512" cy="590" rx="100" ry="40" fill="none" stroke="{wl}" stroke-width="7"/>\n'
    o += f'<polygon points="420,400 604,400 616,450 408,450" fill="none" stroke="{wl}" stroke-width="7"/>\n'
    o += f'<polygon points="360,830 664,830 650,760 374,760" fill="none" stroke="{wl}" stroke-width="7"/>\n'
    o += PG([tl, tr, br, bl], 'none', 14)
    o += R(452, 330, 120, 70, 'none', 0)
    for x in range(462, 572, 16):
        o += L(x, 336, x, 400, 3, '#ffffff')
    for y in range(344, 400, 14):
        o += L(456, y, 568, y, 3, '#ffffff')
    o += P('M452 400 L452 330 L572 330 L572 400', 'none', 0) + TP('M452 400 L452 330 L572 330 L572 400', 8, '#ffffff', 5)
    o += ball(640, 740, 44)
    o += E(640, 790, 40, 8, D, 0, 'opacity="0.2"')
    return svg('sage', o)


S['Sport court_field'] = sport


def pool():
    o = shadow(512, 862, 390, 30)
    o += PG([(250, 360), (774, 360), (864, 850), (160, 850)], '#efe6d4', 14)
    o += PG([(300, 400), (724, 400), (784, 790), (240, 790)], '#6fbad0', 12)
    o += F('M300 400 L724 400 L730 440 L294 440 Z', '#2e211b', 0.15)
    for i in range(1, 4):
        t = i / 4
        xa, xb = lerp(300, 240, 0), lerp(300, 724, t)
        x_top = lerp(300, 724, t)
        x_bot = lerp(240, 784, t)
        for k in range(12):
            u = (k + 0.5) / 12
            o += C(round(lerp(x_top, x_bot, u), 1), round(lerp(400, 790, u), 1), 4 + 4 * u, RED if k % 2 else '#ffffff', 3)
    for (x, y) in ((360, 520), (560, 610), (420, 700), (650, 480)):
        o += P(f'M{x - 40} {y} Q{x - 20} {y - 12} {x} {y} Q{x + 20} {y + 12} {x + 40} {y}', 'none', 6).replace(f'stroke="{D}"', 'stroke="#ffffff"')
    o += f'<ellipse cx="400" cy="640" rx="62" ry="30" fill="none" stroke="{D}" stroke-width="40"/>\n'
    o += f'<ellipse cx="400" cy="640" rx="62" ry="30" fill="none" stroke="#ffffff" stroke-width="26"/>\n'
    o += f'<ellipse cx="400" cy="640" rx="62" ry="30" fill="none" stroke="{RED}" stroke-width="26" stroke-dasharray="36 36"/>\n'
    o += TP('M740 520 L740 460 Q740 430 770 430', 8, GREY, 5) + TP('M800 520 L800 460 Q800 430 830 430', 8, GREY, 5)
    o += L(742, 470, 798, 470, 6) + L(744, 500, 800, 500, 6)
    o += R(460, 300, 104, 28, '#ffffff', 10, rx=8) + R(470, 328, 14, 70, GREY, 6) + R(540, 328, 14, 70, GREY, 6)
    return svg('sky', o)


S['Swimming pool'] = pool


def palm(x, y, s=1.0):
    o = TP('M0 0 Q-10 -120 30 -260', 26, WOOD, 7)
    for d, c in (('M30 -260 Q-40 -300 -110 -240 Q-40 -270 30 -250', LEAF), ('M30 -260 Q100 -310 170 -240 Q100 -270 30 -250', LEAF),
                 ('M30 -260 Q-20 -340 -60 -330 Q0 -320 30 -258', OLIVE), ('M30 -260 Q90 -350 130 -320 Q80 -310 30 -258', OLIVE),
                 ('M30 -260 Q-30 -250 -70 -170 Q-20 -230 30 -254', OLIVE), ('M30 -260 Q100 -240 120 -170 Q90 -230 30 -254', LEAF)):
        o += P(d, c, 9)
    o += C(20, -250, 14, DBROWN, 6) + C(42, -246, 14, DBROWN, 6)
    return G(x, y, s, o)


def beach():
    c = ''
    c += sun(740, 260, 44)
    c += cloud(300, 250, 0.7)
    c += R(150, 380, 724, 110, '#5fa9c0', 0) + L(150, 380, 874, 380, 8)
    c += P('M190 420 Q210 410 230 420 M420 440 Q440 430 460 440 M640 410 Q660 400 680 410', 'none', 5).replace(f'stroke="{D}"', 'stroke="#ffffff"')
    c += P('M150 490 Q260 470 360 494 Q470 516 580 488 Q700 462 874 492 L874 880 L150 880 Z', '#ead3a0', 10)
    c += P('M150 490 Q260 470 360 494 Q470 516 580 488 Q700 462 874 492', 'none', 0).replace('stroke="none"', f'stroke="#ffffff" stroke-width="10"')
    c += palm(770, 800, 1.3)
    c += G(420, 740, 1, PG([(-170, 40), (60, -30), (170, 30), (-60, 110)], TEAL, 10) + PG([(-110, 22), (0, -12), (60, 18), (-50, 54)], '#ffffff', 0, 'opacity="0.35"'))
    c += umbrella(330, 480, 0.75, RED, CREAM)
    c += G(560, 820, 1, P('M-40 -70 L40 -70 L30 0 L-30 0 Z', MUST, 9) + P('M-30 -70 Q0 -110 30 -70', 'none', 6) + L(60, 10, 90, -110, 8) + P('M78 -110 L102 -110 L100 -150 L80 -150 Z', RED, 7))
    c += PG(star_pts(250, 830, 36, 16), TERRA, 8) + P('M660 850 Q670 820 690 850 Z', '#f1b8c4', 6)
    return room('sand', '#e3ecf1', '#ead3a0', 880, c)


def umbrella(cx, cy, s, c1, c2):
    o = L(0, 0, 0, 380, 12)
    n = 6
    w = 400 / n
    for i in range(n):
        o += C(-200 + w * (i + .5), 40, w / 2, c1 if i % 2 == 0 else c2, 9)
    for i in range(n):
        x0, x1 = -200 + w * i, -200 + w * (i + 1)
        o += PG([(0, -100), (x1, 40), (x0, 40)], c1 if i % 2 == 0 else c2, 0)
    o += P('M-200 40 Q-190 -60 0 -100 Q190 -60 200 40 Z', 'none', 12)
    o += C(0, -104, 10, DBROWN, 6)
    return G(cx, cy, s, o)


S['The beach_dry area_'] = beach


def sea():
    c = ''
    c += sun(700, 290, 56) + cloud(300, 260, 0.8)
    c += R(150, 420, 724, 470, '#4f9ab8', 0) + L(150, 420, 874, 420, 8)
    c += R(150, 420, 724, 120, '#6fb4cc', 0)
    for i, y in enumerate((470, 540, 620, 700, 780)):
        for k in range(4):
            x = 190 + k * 180 + (i % 2) * 90
            c += P(f'M{x} {y} Q{x + 25} {y - 20} {x + 50} {y} Q{x + 75} {y - 20} {x + 100} {y}', 'none', 7).replace(f'stroke="{D}"', 'stroke="#ffffff" opacity="0.8"')
    boat = P('M-110 0 L110 0 L80 50 L-80 50 Z', RED, 12) + L(0, 0, 0, -200, 10)
    boat += P('M8 -190 L110 -20 L8 -20 Z', CREAM, 10) + P('M-8 -170 L-90 -20 L-8 -20 Z', '#ffffff', 10)
    c += G(430, 560, 1, boat)
    c += P('M390 616 Q430 604 470 616', 'none', 5).replace(f'stroke="{D}"', 'stroke="#ffffff"')
    for x, y in ((560, 210), (620, 240)):
        c += P(f'M{x - 24} {y} Q{x - 12} {y - 14} {x} {y} Q{x + 12} {y - 14} {x + 24} {y}', 'none', 6)
    c += P('M150 860 Q300 800 480 840 Q600 866 700 880 L150 880 Z', '#ead3a0', 10)
    return room('sky', '#e3ecf1', '#4f9ab8', 420, c)


S['The sea_the beach'] = sea

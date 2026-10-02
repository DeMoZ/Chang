"""Places: shop-front scenes."""
from places_lib import *

S = {}


def shelf(x, y, w):
    return R(x, y, w, 14, WOOD, 8)


def croissant(x, y, s=1):
    o = P('M-40 10 Q-30 -26 0 -28 Q30 -26 40 10 Q20 0 0 2 Q-20 0 -40 10 Z', '#d9a05b', 7)
    o += P('M-16 -24 L-10 0 M0 -28 L0 2 M16 -24 L10 0', 'none', 4)
    return G(x, y, s, o)


def baguette(x, y, rot, s=1):
    o = R(-60, -12, 120, 24, '#d9a05b', 7, rx=12) + P('M-34 -8 L-24 8 M-8 -8 L2 8 M18 -8 L28 8', 'none', 4)
    return G(x, y, s, o, rot)


def bakery():
    wi = shelf(240, 640, 330) + shelf(240, 760, 330)
    wi += G(300, 620, 0.8, p_bread()) + croissant(405, 628, 0.9) + G(505, 620, 0.8, p_bread())
    wi += G(320, 734, 0.8, P('M-44 20 L-40 -14 L40 -14 L44 20 Z', '#e9b7c0', 7) + P('M-40 -14 Q0 -40 40 -14 Z', CREAM, 7) + C(0, -32, 9, RED, 5))
    wi += croissant(430, 742, 0.9) + croissant(510, 742, 0.8)
    wi += baguette(330, 560, -20, 0.9) + baguette(470, 560, 15, 0.9)
    front = G(700, 860, 1, R(-70, -60, 140, 60, WOOD, 10, rx=6) + baguette(-20, -70, -60, 0.8) + baguette(20, -70, -75, 0.8) + R(-70, -60, 140, 30, '#b8845a', 10, rx=6))
    return shop('beige', '#e9c49a', (RED, CREAM), p_bread(), wi, extra_front=front)


S['Bakery shop'] = bakery


def bookstore():
    wi = ''
    cols = [RED, BLUE, MUST, OLIVE, PLUM, TEAL, TERRA]
    for row, y in enumerate((520, 640)):
        x = 250
        i = row * 3
        while x < 550:
            w = 22 + (i * 7) % 16
            h = 90 - (i * 13) % 30
            o = R(x, y + 110 - h, w, h, cols[i % len(cols)], 6)
            wi += o + L(x + 4, y + 110 - h + 14, x + w - 4, y + 110 - h + 14, 4)
            x += w + 4
            i += 1
        wi += shelf(240, y + 110, 330)
    wi += G(405, 780, 0.001, '')
    front = G(700, 860, 1, R(-60, -80, 120, 80, WOOD, 10, rx=4) + G(0, -104, 0.8, p_book()))
    return shop('sage', OLIVE, (BLUE, CREAM), p_book(), wi, extra_front=front, winb=(240, 500, 330, 290))


S['Book store'] = bookstore


def coffee():
    wi = L(320, 500, 320, 550, 6) + P('M290 550 L350 550 L340 580 L300 580 Z', MUST, 6)
    wi += L(490, 500, 490, 550, 6) + P('M460 550 L520 550 L510 580 L470 580 Z', MUST, 6)
    wi += shelf(240, 700, 330)
    wi += R(270, 620, 90, 80, GREY, 8, rx=8) + R(290, 640, 50, 20, D, 0) + L(315, 660, 315, 680, 8)
    wi += G(430, 700, 0.6, p_cup()) + G(510, 700, 0.6, p_cup())
    front = ''
    front += G(330, 862, 1, E(0, -2, 90, 14, D, 0, 'opacity="0.15"') + L(0, -110, 0, -6, 12) + L(-40, -4, 40, -4, 12)
                   + E(0, -114, 80, 16, CREAM, 10) + G(0, -150, 0.6, p_cup()))
    front += G(200, 862, 1, P('M-10 -120 L-10 0 M30 -60 L30 0 M-10 -60 L34 -60', 'none', 10))
    front += G(460, 862, 1, P('M10 -120 L10 0 M-30 -60 L-30 0 M10 -60 L-34 -60', 'none', 10))
    return shop('beige', '#b7825c', (DBROWN, CREAM), p_cup(), wi)


S['Coffee shop'] = coffee


def crate(x, y, fruit, n=4, r=18):
    o = R(x - 70, y - 60, 140, 60, '#c9975f', 10) + L(x - 70, y - 30, x + 70, y - 30, 6)
    for i in range(n):
        o = C(x - 50 + i * (100 / (n - 1)), y - 64, r, fruit, 8) + o
    for i in range(n - 1):
        o = C(x - 34 + i * (100 / (n - 1)), y - 84, r, fruit, 8) + o
    return o


def grocery():
    wi = ''
    for y in (600, 720):
        for i, x in enumerate(range(262, 560, 48)):
            c = [RED, BLUE, OLIVE, MUST, PLUM, TEAL][(i + y // 120) % 6]
            wi += R(x, y - 64, 36, 64, c, 6, rx=5) + R(x + 8, y - 80, 20, 18, GREY, 5, rx=3) + FR(x + 4, y - 44, 28, 14, CREAM, 0.8)
        wi += shelf(240, y, 330)
    front = crate(310, 860, RED) + crate(470, 860, '#e39a4a') + crate(690, 860, OLIVE)
    front += G(690, 790, 0.001, '')
    return shop('sage', MUST, (LEAF, CREAM), p_cart(), wi, extra_front=front, pscale=1.3)


S['Grocery store'] = grocery


def generic_shop():
    wi = shelf(240, 700, 330)
    wi += G(300, 700, 0.9, p_bag()[0:0] + P('M-36 -18 L36 -18 L42 44 L-42 44 Z', BLUE, 7) + P('M-18 -16 Q-18 -46 0 -46 Q18 -46 18 -16', 'none', 7), 0).replace('translate(300 700)', 'translate(300 656)')
    wi += R(370, 610, 80, 90, MUST, 8, rx=4) + L(370, 640, 450, 640, 5)
    wi += G(510, 656, 0.9, p_bag())
    wi += P('M300 520 L300 560 M260 600 L300 560 L340 600 Z', 'none', 6) + P('M262 598 L338 598 L330 640 L270 640 Z', PLUM, 0)
    wi += P('M440 520 L440 560 M400 600 L440 560 L480 600', 'none', 6) + P('M402 598 L478 598 L470 640 L410 640 Z', TEAL, 0)
    wi = shelf(240, 700, 330)
    wi += G(300, 656, 0.9, P('M-36 -18 L36 -18 L42 44 L-42 44 Z', BLUE, 7) + P('M-18 -16 Q-18 -46 0 -46 Q18 -46 18 -16', 'none', 7))
    wi += R(370, 610, 80, 90, MUST, 8, rx=4) + L(370, 640, 450, 640, 5)
    wi += G(510, 656, 0.9, p_bag())
    wi += L(250, 540, 560, 540, 8)
    wi += P('M310 540 L310 556 M270 600 L310 556 L350 600 Z', 'none', 5) + P('M280 570 L340 570 L352 600 L268 600 Z', PLUM, 6) + P('M290 600 L286 690 M330 600 L334 690', 'none', 0)
    wi += P('M470 540 L470 556 M430 600 L470 556 L510 600 Z', 'none', 5) + P('M440 570 L500 570 L512 600 L428 600 Z', TEAL, 6)
    wi += shelf(240, 800, 1)
    front = bush(200, 860, 0.9) + G(760, 860, 1, R(-50, -110, 100, 110, CREAM, 10) + G(0, -56, 0.8, p_bag()))
    return shop('sand', TERRA, (BLUE, CREAM), p_bag(), wi, extra_front=front)


S['_Shop'] = generic_shop


def pharmacy():
    wi = ''
    for y in (600, 720):
        for i, x in enumerate(range(262, 540, 56)):
            if i % 2 == 0:
                wi += R(x, y - 70, 38, 70, CREAM, 6, rx=6) + R(x + 6, y - 86, 26, 18, [TEAL, RED, BLUE][i % 3], 5, rx=3) + P(f'M{x + 19} {y - 50} v24 M{x + 7} {y - 38} h24', 'none', 5)
            else:
                wi += R(x - 6, y - 50, 46, 50, [OLIVE, BLUE, MUST][i % 3], 6, rx=3) + FR(x, y - 38, 34, 12, CREAM, 0.85)
        wi += shelf(240, y, 330)
    ew = G(812, 470, 1, R(-16, -6, 30, 12, DBROWN, 6) + R(10, -56, 100, 100, '#6fa55a', 10, rx=10) + G(60, -6, 0.85, p_cross('#ffffff')))
    ew = R(820, 400, 30, 14, DBROWN, 6) + R(846, 350, 0, 0, 'none', 0)
    return shop('sage', '#e8efe2', (TEAL, CREAM), p_pill(), wi, sign_fill=CREAM,
                extra_wall=R(700, 350, 0, 0, 'none', 0),
                extra_back='', extra_front=G(0, 0, 1, R(96, 480, 110, 110, '#6fa55a', 12, rx=14) + G(151, 535, 0.9, p_cross(CREAM)) + R(196, 520, 20, 28, DBROWN, 8)))


S['Pharmacy'] = pharmacy


def frame(x, y, w, h, inner_fill, inner):
    return R(x, y, w, h, WOOD, 8) + R(x + 10, y + 10, w - 20, h - 20, inner_fill, 6) + inner


def photo():
    wi = frame(260, 520, 150, 110, '#bfe0f0', P('M275 620 L315 570 L345 600 L370 575 L395 620 Z', OLIVE, 5) + C(375, 548, 10, MUST, 4))
    wi += frame(430, 520, 110, 140, '#f3d9c9', C(485, 575, 20, SKIN, 5) + P('M455 650 Q455 605 485 605 Q515 605 515 650 Z', RED, 5) + P('M465 568 Q465 548 485 548 Q505 548 505 568 Q485 558 465 568 Z', HAIR, 0))
    wi += shelf(240, 740, 330)
    wi += G(330, 704, 0.7, p_camera()) + G(470, 710, 0.6, R(-40, -40, 80, 70, '#f6f2ea', 6) + R(-30, -32, 60, 44, TEAL, 4))
    front = G(740, 860, 1, L(-30, -170, -50, 0, 10) + L(30, -170, 50, 0, 10) + L(0, -170, 0, 0, 10) + G(0, -200, 0.8, p_camera()))
    return shop('sky', PLUM, (MUST, CREAM), p_camera(), wi, extra_front=front)


S['Photo shop'] = photo


def mannequin(x, y, s, suit=BLUE):
    o = L(0, 0, 0, -120, 10) + L(-40, 0, 40, 0, 12)
    o += P('M-50 -120 Q-54 -250 -30 -262 L30 -262 Q54 -250 50 -120 Z', suit, 10)
    o += P('M-20 -262 L0 -200 L20 -262 Z', '#ffffff', 7) + P('M-6 -250 L0 -214 L6 -250 Z', RED, 4)
    o += P('M-30 -262 L-6 -190 L-12 -120 M30 -262 L6 -190 L12 -120', 'none', 6)
    o += C(-2, -170, 4, D, 0) + C(-2, -148, 4, D, 0)
    o += R(-12, -290, 24, 30, CREAM, 8) + E(0, -300, 14, 10, WOOD, 6)
    return G(x, y, s, o)


def tailor():
    wi = mannequin(330, 790, 1.0) + mannequin(480, 790, 1.0, PLUM)
    wi += P('M250 520 Q300 560 330 520 Q380 480 420 520 Q470 560 560 520', 'none', 0)
    wi += TP('M250 530 Q290 570 330 530 Q380 490 420 530 Q470 570 560 530', 12, MUST, 5)
    for i in range(8):
        wi += L(262 + i * 38, 530 + (8 if i % 2 else -2), 262 + i * 38, 540 + (8 if i % 2 else -2), 3)
    front = ''
    return shop('rose', BLUE, (PLUM, CREAM), p_spool(), wi, extra_front=front, pscale=1.4)


S['Tailor shop'] = tailor


def salon_chair(x, y, s, c=RED):
    o = R(-8, -60, 16, 60, GREY, 6) + E(0, 0, 40, 10, GREY, 6)
    o += R(-44, -110, 88, 50, c, 8, rx=14) + R(-36, -190, 72, 90, c, 8, rx=18) + R(-56, -120, 20, 40, c, 6, rx=8) + R(36, -120, 20, 40, c, 6, rx=8)
    return G(x, y, s, o)


def hairsalon():
    wi = R(320, 530, 170, 130, '#e8f1f4', 8, rx=60) + F('M350 600 L420 540 L440 546 L360 620 Z', '#fff', 0.6)
    wi += salon_chair(405, 790, 1.0, PLUM)
    wi += G(520, 580, 1, P('M-40 0 Q-40 -50 0 -50 Q40 -50 40 0 Z', GREY, 8) + L(0, 0, 0, 200, 8) + E(0, 204, 26, 8, GREY, 6))
    return shop('rose', '#d9969a', (PLUM, CREAM), p_dryer(), wi, pscale=1.3)


S['Hair salon'] = hairsalon


def barber():
    wi = R(290, 530, 190, 120, '#e8f1f4', 8, rx=10) + F('M310 600 L380 540 L400 544 L320 620 Z', '#fff', 0.6)
    wi += salon_chair(385, 790, 1.0, RED)
    pole = R(546, 480, 48, 24, GREY, 8, rx=6) + R(546, 736, 48, 24, GREY, 8, rx=6)
    pole += '<defs><clipPath id="pole"><rect x="552" y="504" width="36" height="232"/></clipPath></defs>\n'
    pole += '<g clip-path="url(#pole)">' + R(552, 504, 36, 232, '#ffffff', 0)
    for i in range(-2, 10):
        y = 504 + i * 34
        pole += PG([(552, y), (588, y - 24), (588, y - 10), (552, y + 14)], RED if i % 2 == 0 else BLUE, 0)
    pole += '</g>' + R(552, 504, 36, 232, 'none', 8) + FR(558, 504, 8, 232, '#fff', 0.35)
    pole += C(570, 470, 16, GREY, 8)
    return shop('sky', CREAM, (RED, CREAM), p_mustache(), wi, winb=(240, 500, 290, 300), extra_front=pole, pscale=1.4)


S['Barber'] = barber


def massage():
    wi = R(260, 700, 290, 26, WOOD, 8, rx=6) + L(280, 726, 280, 790, 10) + L(530, 726, 530, 790, 10)
    wi += R(270, 680, 270, 26, CREAM, 8, rx=10)
    wi += C(300, 660, 26, SKIN2, 8) + P('M270 646 Q300 624 326 650 Q300 640 276 660 Z', HAIR, 0)
    wi += P('M322 682 Q330 636 380 640 L520 646 Q540 650 536 682 Z', '#e8a0b0', 8)
    wi += bust(440, 646, 0.7, OLIVE, SKIN)
    wi += G(510, 560, 0.8, p_lotus())
    wi += G(300, 560, 1, L(0, 30, 0, -30, 6, LEAF) + C(-10, -30, 12, '#e8c0e0', 5) + C(12, -16, 10, '#e8c0e0', 5))
    return shop('sage', '#b98a5e', (OLIVE, CREAM), p_lotus(), wi, pscale=1.4, roof='#7a4e33')


S['Massage shop'] = massage


def nailsalon():
    wi = ''
    cols = ['#c8577e', RED, PLUM, TEAL, MUST, '#e8a0b0']
    for row, y in enumerate((620, 750)):
        for i in range(5):
            x = 275 + i * 60
            c = cols[(i + row * 2) % len(cols)]
            wi += R(x - 18, y - 60, 36, 60, c, 7, rx=8) + R(x - 8, y - 96, 16, 38, HAIR, 6, rx=4) + FR(x - 12, y - 52, 8, 40, '#fff', 0.4)
        wi += shelf(240, y, 330)
    return shop('rose', '#e3a5b4', (PLUM, CREAM), p_nails(), wi, pscale=1.3)


S['Nail salon'] = nailsalon


def restaurant():
    wi = L(320, 500, 320, 540, 6) + P('M290 540 L350 540 L340 568 L300 568 Z', MUST, 6)
    wi += L(490, 500, 490, 540, 6) + P('M460 540 L520 540 L510 568 L470 568 Z', MUST, 6)
    for x in (320, 490):
        wi += P(f'M{x - 70} 700 L{x + 70} 700 L{x + 80} 760 L{x - 80} 760 Z', '#ffffff', 8)
        wi += PG([(x - 70, 700), (x - 50, 700), (x - 46, 760), (x - 66, 760)], RED, 0)
        wi += PG([(x - 10, 700), (x + 10, 700), (x + 12, 760), (x - 12, 760)], RED, 0)
        wi += PG([(x + 50, 700), (x + 70, 700), (x + 78, 760), (x + 56, 760)], RED, 0)
        wi += P(f'M{x - 70} 700 L{x + 70} 700 L{x + 80} 760 L{x - 80} 760 Z', 'none', 8)
        wi += E(x, 690, 34, 10, CREAM, 6) + P(f'M{x - 20} 686 Q{x} 660 {x + 20} 686 Z', TERRA, 5)
    front = G(740, 862, 1, L(-40, -10, 0, -170, 10) + L(40, -10, 0, -170, 10) + R(-50, -170, 100, 110, '#3b3a3a', 10, rx=6) + G(0, -115, 0.7, p_forkknife()))
    front = G(760, 862, 1, L(-44, 0, -24, -160, 10) + L(44, 0, 24, -160, 10) + R(-56, -170, 112, 120, '#3b3a3a', 10, rx=6)
              + G(0, -112, 0.8, p_forkknife()))
    return shop('beige', RED, (OLIVE, CREAM), p_forkknife(), wi, extra_front=front, doorb=(620, 510, 160, 350), pscale=1.3)


S['Restaurant'] = restaurant


def dental():
    wi = P('M280 780 L300 700 L420 700 L500 640 L520 660 L440 730 L440 780 Z', TEAL, 8)
    wi += R(270, 700, 60, 90, TEAL, 8, rx=10)
    wi += P('M300 700 L290 620', 'none', 0)
    wi += TP('M480 560 Q520 540 520 500', 8, GREY, 5) + E(460, 568, 30, 16, MUST, 7)
    wi += R(270, 780, 200, 14, GREY, 6)
    wi = R(270, 730, 190, 34, TEAL, 8, rx=14) + P('M270 740 L250 640 Q248 620 268 620 L290 620 L306 730 Z', TEAL, 8)
    wi += P('M450 730 L520 760 L530 790 L460 764 Z', TEAL, 8) + L(360, 764, 360, 800, 12) + L(320, 800, 400, 800, 12)
    wi += TP('M470 520 Q500 520 500 600', 8, GREY, 5) + E(460, 530, 34, 16, MUST, 7) + FR(436, 522, 30, 6, '#fff', 0.5)
    wi += G(520, 700, 0.9, p_tooth())
    return shop('sky', '#dde9ee', (TEAL, CREAM), p_tooth(), wi, pscale=1.5)


S['Dental clinic'] = dental

"""Clothes / Colors / Boss-Chief-Colleague generator. Reuses helpers from gen_people.py (exec of its helper part only)."""
import math
import os

ROOT = os.path.dirname(os.path.abspath(__file__))
_src = open(os.path.join(ROOT, "gen_people.py")).read().split("\nout = {}")[0]
exec(_src)

out = {}
SH = lambda cx, cy, rx, ry=None: f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry or rx * 0.16:.0f}" fill="{OL}" opacity="0.15"/>'
HL = lambda d, w=16, op=0.3: f'<path d="{d}" fill="none" stroke="#fff" stroke-width="{w}" opacity="{op}" stroke-linecap="round" stroke-linejoin="round"/>'
DK = lambda d, op=0.14: f'<path d="{d}" fill="{OL}" opacity="{op}"/>'


def P_(d, fill, w=14, extra=""):
    return f'<path d="{d}" fill="{fill}" stroke="{OL}" stroke-width="{w}" stroke-linejoin="round" stroke-linecap="round" {extra}/>'


def L_(d, w=8, color=OL, op=1.0, dash=None):
    da = f' stroke-dasharray="{dash}"' if dash else ""
    o = f' opacity="{op}"' if op < 1 else ""
    return f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"{da}{o}/>'


def hanger(x=512, y=250, w=150):
    """hook top at y-80, bar at y."""
    return (L_(f"M{x} {y - 40} Q{x} {y - 62} {x + 18} {y - 72} Q{x + 34} {y - 84} {x + 22} {y - 102} Q{x + 6} {y - 118} {x - 12} {y - 100}", 11, "#8a8580")
            + L_(f"M{x} {y - 40} Q{x} {y - 62} {x + 18} {y - 72} Q{x + 34} {y - 84} {x + 22} {y - 102} Q{x + 6} {y - 118} {x - 12} {y - 100}", 4, "#d9d3cc")
            + P_(f"M{x} {y - 44} L{x + w} {y + 6} L{x - w} {y + 6} Z", "#9a6a45", 10))


# ---------------- garments (drawn around x=512) ----------------
TEE = "M440 262 Q512 314 584 262 L680 290 L772 384 L716 440 L652 402 L656 762 Q512 778 368 762 L372 402 L308 440 L252 384 L344 290 Z"


def tee(color="#4f9a9a"):
    s = P_(TEE, color)
    s += DK("M600 420 L652 402 L656 762 Q620 768 590 770 Z", 0.15)
    s += HL("M410 420 L406 720")
    s += L_("M440 262 Q512 314 584 262", 10)
    s += L_("M452 270 Q512 322 572 270", 7, OL, 0.5)
    s += L_("M318 430 L262 376", 7, OL, 0.45) + L_("M706 430 L762 376", 7, OL, 0.45)
    return s


def polo(color="#6f9bb0"):
    s = P_(TEE, color)
    s += DK("M600 420 L652 402 L656 762 Q620 768 590 770 Z", 0.15)
    s += HL("M410 430 L406 720")
    # ribbed sleeve cuffs
    s += P_("M252 384 L308 440 L326 422 L270 366 Z", color, 9) + L_("M262 388 L302 428", 5, OL, 0.4)
    s += P_("M772 384 L716 440 L698 422 L754 366 Z", color, 9) + L_("M762 388 L722 428", 5, OL, 0.4)
    # placket + buttons
    s += P_("M488 290 L536 290 L536 420 L488 420 Z", color, 9)
    s += "".join(f'<circle cx="512" cy="{y}" r="9" fill="#f6ecd8" stroke="{OL}" stroke-width="5"/>' for y in (320, 356, 392))
    # collar
    s += P_("M440 262 L420 250 L412 262 L470 352 L512 292 Z", "#f6ecd8", 10)
    s += P_("M584 262 L604 250 L612 262 L554 352 L512 292 Z", "#f6ecd8", 10)
    # small logo
    s += f'<circle cx="590" cy="440" r="13" fill="#c8574b" stroke="{OL}" stroke-width="5"/>'
    return s


SHIRT = ("M438 258 L586 258 L688 292 Q742 322 762 420 L802 642 L740 658 L702 472 L664 446 L664 782 Q512 798 360 782 "
         "L360 446 L322 472 L284 658 L222 642 L262 420 Q282 322 336 292 Z")


def shirt(color="#8fb4cf"):
    s = P_(SHIRT, color)
    s += DK("M612 440 L664 446 L664 782 Q630 790 604 792 Z", 0.14)
    s += HL("M300 440 L262 630", 14, 0.25) + HL("M410 440 L406 740")
    # cuffs
    s += P_("M222 642 L284 658 L276 700 L214 684 Z", "#f6ecd8", 10)
    s += P_("M802 642 L740 658 L748 700 L810 684 Z", "#f6ecd8", 10)
    # placket + buttons
    s += L_("M512 300 L512 788", 8)
    s += L_("M530 300 L530 788", 6, OL, 0.35)
    s += "".join(f'<circle cx="521" cy="{y}" r="9" fill="#f6ecd8" stroke="{OL}" stroke-width="5"/>' for y in (360, 450, 540, 630, 720))
    # chest pocket
    s += P_("M570 430 L640 430 L640 510 Q605 524 570 510 Z", color, 9) + L_("M570 450 L640 450", 6, OL, 0.5)
    # collar
    s += P_("M438 258 L414 244 L402 270 L470 350 L512 300 Z", "#f6ecd8", 10)
    s += P_("M586 258 L610 244 L622 270 L554 350 L512 300 Z", "#f6ecd8", 10)
    return s


def sweater(color="#d9825b", band="#f6ecd8", zig="#5d82a8"):
    body = ("M430 262 Q512 300 594 262 L690 290 Q746 318 766 420 L806 652 L744 666 L704 480 L666 452 L670 772 Q512 790 354 772 "
            "L358 452 L320 480 L280 666 L218 652 L258 420 Q278 318 334 290 Z")
    s = P_(body, color)
    s += DK("M614 450 L666 452 L670 772 Q640 780 610 782 Z", 0.14)
    # pattern band
    s += f'<clipPath id="swc"><path d="{body}"/></clipPath>'
    zz = "M200 470 " + " ".join(f"L{200 + i * 30} {470 + (24 if i % 2 else 0)}" for i in range(1, 22))
    s += (f'<g clip-path="url(#swc)"><rect x="200" y="420" width="640" height="110" fill="{band}"/>'
          + L_(zz, 12, zig) + "".join(f'<circle cx="{230 + i * 60}" cy="440" r="7" fill="{zig}"/><circle cx="{230 + i * 60}" cy="508" r="7" fill="#c8574b"/>' for i in range(11))
          + L_("M200 420 L840 420", 9) + L_("M200 530 L840 530", 9) + f'</g>')
    s += f'<path d="{body}" fill="none" stroke="{OL}" stroke-width="14" stroke-linejoin="round"/>'
    # ribbed neck, cuffs, hem
    s += P_("M430 262 Q512 300 594 262 Q586 244 574 240 Q512 272 450 240 Q438 244 430 262 Z", color, 10)
    s += P_("M354 772 Q512 790 670 772 L672 820 Q512 838 352 820 Z", color, 12)
    s += "".join(L_(f"M{x} {782 + abs(x - 512) * -0.02} L{x} {822 + abs(x - 512) * -0.02}", 5, OL, 0.35) for x in range(380, 660, 28))
    s += P_("M218 652 L280 666 L270 716 L208 702 Z", color, 10) + L_("M230 664 L222 704 M250 668 L242 708", 5, OL, 0.35)
    s += P_("M806 652 L744 666 L754 716 L816 702 Z", color, 10) + L_("M794 664 L802 704 M774 668 L782 708", 5, OL, 0.35)
    s += HL("M300 450 L262 630", 14, 0.25)
    return s


def dress(color="#e8a0a8", trim="#f6ecd8"):
    s = hanger(512, 262, 140)
    s += P_("M446 250 Q512 296 578 250 L610 262 Q614 330 596 410 L428 410 Q410 330 414 262 Z", color)  # bodice
    s += P_("M414 262 L380 266 Q372 300 390 330 L420 320 Z", color, 11) + P_("M610 262 L644 266 Q652 300 634 330 L604 320 Z", color, 11)
    s += P_("M428 410 L596 410 Q700 600 760 800 Q512 850 264 800 Q324 600 428 410 Z", color)  # skirt
    s += DK("M596 410 Q700 600 760 800 Q700 812 650 818 Q620 600 560 420 Z", 0.13)
    s += L_("M470 440 Q430 620 400 820 M512 440 L512 830 M554 440 Q594 620 624 820", 7, OL, 0.35)
    s += P_("M264 800 Q512 850 760 800 L770 836 Q512 890 254 836 Z", trim, 10)
    s += "".join(f'<circle cx="{x}" cy="{842 - 30 * math.cos((x - 512) / 260 * 1.2) + 0}" r="7" fill="{color}"/>' for x in range(300, 740, 44))
    s += P_("M424 402 L600 402 L604 432 L420 432 Z", "#c8574b", 10)  # waist ribbon
    s += P_("M512 416 L462 388 L466 448 Z", "#c8574b", 9) + P_("M512 416 L562 388 L558 448 Z", "#c8574b", 9)
    s += f'<circle cx="512" cy="416" r="14" fill="#c8574b" stroke="{OL}" stroke-width="8"/>'
    s += HL("M442 290 Q432 340 440 390") + HL("M400 560 Q350 680 320 780", 16, 0.25)
    return s


def skirt(color="#5d82a8"):
    s = P_("M372 300 L652 300 L652 360 L372 360 Z", color)
    s += P_("M372 360 L652 360 L780 700 Q512 760 244 700 Z", color)
    for i, x in enumerate(range(0, 9)):
        t = x / 8
        xt = 372 + t * 280
        xb = 244 + t * 536
        yb = 700 + 60 * math.sin(math.pi * t) * 0.9
        if 0 < x < 8:
            s += L_(f"M{xt:.0f} 364 L{xb:.0f} {yb:.0f}", 7, OL, 0.55)
        if x % 2 == 1 and x < 8:
            xb2 = 244 + (x + 1) / 8 * 536
            s += f'<path d="M{xt:.0f} 364 L{372 + (x + 1) / 8 * 280:.0f} 364 L{xb2:.0f} {700 + 54 * math.sin(math.pi * (x + 1) / 8):.0f} L{xb:.0f} {yb:.0f} Z" fill="{OL}" opacity="0.12"/>'
    s += f'<path d="M372 360 L652 360 L780 700 Q512 760 244 700 Z" fill="none" stroke="{OL}" stroke-width="14" stroke-linejoin="round"/>'
    s += f'<circle cx="620" cy="330" r="10" fill="#f6ecd8" stroke="{OL}" stroke-width="5"/>'
    s += P_("M512 330 L470 306 L472 354 Z", "#e8a0a8", 8) + P_("M512 330 L554 306 L552 354 Z", "#e8a0a8", 8)
    s += HL("M360 420 L300 660", 16, 0.28)
    return s


def trousers_shape(top=250, bot=830, wl=368, wr=656, hem=150, crotch=420):
    return f"M{wl} {top} L{wr} {top} L{wr + 30} {bot} L{wr + 30 - hem} {bot} L512 {crotch} L{wl - 30 + hem} {bot} L{wl - 30} {bot} Z"


def pants_casual(color="#d8b48a"):
    """Casual chinos: elastic drawstring waist, relaxed tapered legs, rolled cuffs."""
    body = "M364 262 L660 262 Q700 460 676 760 L548 760 Q536 560 512 440 Q488 560 476 760 L348 760 Q324 460 364 262 Z"
    s = P_(body, color)
    s += DK("M600 300 L660 262 Q700 460 676 760 L630 760 Q650 500 600 300 Z", 0.13)
    # elastic waistband
    s += P_("M358 236 L666 236 L662 290 L362 290 Z", color, 12)
    s += "".join(L_(f"M{x} 244 L{x + 4} 284", 5, OL, 0.35) for x in range(380, 650, 22))
    s += L_("M500 290 Q490 340 470 360 M524 290 Q534 340 556 356", 7) + f'<circle cx="470" cy="362" r="8" fill="#f6ecd8" stroke="{OL}" stroke-width="5"/><circle cx="556" cy="358" r="8" fill="#f6ecd8" stroke="{OL}" stroke-width="5"/>'
    # side pockets
    s += L_("M384 300 Q420 330 420 380", 7, OL, 0.6) + L_("M640 300 Q604 330 604 380", 7, OL, 0.6)
    # knee wrinkles
    s += L_("M380 560 Q410 552 440 562 M590 566 Q620 556 650 566", 6, OL, 0.35)
    # rolled cuffs
    s += P_("M344 752 L480 752 L482 812 L342 812 Z", "#c49a6c", 12) + P_("M544 752 L680 752 L682 812 L542 812 Z", "#c49a6c", 12)
    s += HL("M380 320 Q360 480 372 720", 16, 0.28)
    return s


def trousers_formal(color="#3e4f70"):
    body = trousers_shape(268, 850, 372, 652, 140, 420)
    s = P_(body, color)
    s += DK("M600 268 L652 268 L682 850 L640 850 Z", 0.18)
    # belt + buckle + loops
    s += P_("M366 236 L658 236 L658 282 L366 282 Z", "#3b2a22", 12)
    s += f'<rect x="486" y="228" width="52" height="62" rx="8" fill="#e0b04f" stroke="{OL}" stroke-width="9"/><rect x="500" y="244" width="24" height="30" rx="4" fill="#3b2a22"/>'
    s += "".join(f'<rect x="{x - 9}" y="226" width="18" height="66" rx="4" fill="{color}" stroke="{OL}" stroke-width="7"/>' for x in (410, 614))
    # fly + slanted pockets
    s += L_("M512 282 L512 410", 7, OL, 0.7) + L_("M512 282 Q540 330 530 398", 6, OL, 0.45)
    s += L_("M384 290 L430 380", 7, OL, 0.7) + L_("M640 290 L594 380", 7, OL, 0.7)
    # sharp creases (light lines)
    s += L_("M438 300 L402 842", 6, "#fff", 0.5) + L_("M586 300 L622 842", 6, "#fff", 0.5)
    s += HL("M388 320 L362 800", 14, 0.2)
    return s


def jeans(color="#4f79a8"):
    body = trousers_shape(262, 840, 372, 652, 146, 430)
    s = P_(body, color)
    s += DK("M600 262 L652 262 L682 840 L636 840 Z", 0.16)
    s += P_("M366 232 L658 232 L658 280 L366 280 Z", color, 12)
    ST = "#e0a24f"
    s += L_("M372 244 L652 244 M372 270 L652 270", 4, ST, 1, "10 8")
    s += "".join(f'<rect x="{x - 9}" y="224" width="18" height="64" rx="4" fill="{color}" stroke="{OL}" stroke-width="7"/>' for x in (410, 470, 614))
    s += f'<circle cx="540" cy="256" r="13" fill="#d9c09a" stroke="{OL}" stroke-width="6"/>'
    # front pockets (J curves) + coin pocket + fly J-stitch
    s += L_("M378 290 Q450 300 450 370", 8) + L_("M392 290 Q440 304 442 364", 4, ST, 1, "10 8")
    s += L_("M646 290 Q574 300 574 370", 8) + L_("M632 290 Q584 304 582 364", 4, ST, 1, "10 8")
    s += P_("M590 294 L628 294 L630 330 L592 330 Z", color, 5) + L_("M594 300 L626 300", 4, ST, 1, "8 6")
    s += L_("M500 282 L500 380 Q500 404 520 410", 4, ST, 1, "10 8") + L_("M512 280 L512 420", 7, OL, 0.8)
    s += f'<circle cx="450" cy="302" r="6" fill="#d9c09a" stroke="{OL}" stroke-width="4"/><circle cx="574" cy="302" r="6" fill="#d9c09a" stroke="{OL}" stroke-width="4"/>'
    # side seams + hem stitch
    s += L_("M352 400 L344 820 M672 400 L680 820", 4, ST, 1, "10 8")
    s += L_("M344 816 L488 816 M536 816 L680 816", 4, ST, 1, "10 8")
    # fading
    s += f'<ellipse cx="420" cy="560" rx="34" ry="90" fill="#fff" opacity="0.18"/><ellipse cx="604" cy="560" rx="34" ry="90" fill="#fff" opacity="0.18"/>'
    s += L_("M396 480 Q420 470 444 482 M580 486 Q604 474 628 486", 5, OL, 0.3)
    return s


def shorts(color="#a79e6a"):
    body = "M356 300 L668 300 L710 610 L540 626 L512 470 L484 626 L314 610 Z"
    s = P_(body, color)
    s += DK("M620 300 L668 300 L710 610 L664 614 Z", 0.15)
    s += P_("M350 262 L674 262 L672 308 L352 308 Z", "#5c3d2e", 12)
    s += f'<rect x="490" y="254" width="44" height="60" rx="8" fill="#d9c09a" stroke="{OL}" stroke-width="8"/>'
    s += "".join(f'<rect x="{x - 9}" y="252" width="18" height="64" rx="4" fill="{color}" stroke="{OL}" stroke-width="7"/>' for x in (400, 624))
    s += L_("M512 312 L512 470", 7, OL, 0.7)
    s += L_("M376 318 Q430 330 436 390", 8) + L_("M648 318 Q594 330 588 390", 8)
    # cargo-ish side pocket
    s += P_("M334 440 L400 440 L398 520 L330 516 Z", color, 8) + L_("M334 458 L400 458", 5, OL, 0.5)
    # rolled cuffs
    s += P_("M314 610 L484 626 L480 676 L308 660 Z", "#c2b98a", 12) + P_("M710 610 L540 626 L544 676 L716 660 Z", "#c2b98a", 12)
    s += HL("M372 340 L346 580", 14, 0.25)
    return s


def tank_top(color="#e8a0a8"):
    body = "M400 250 L444 250 Q512 350 580 250 L624 250 Q622 330 662 400 L670 700 Q512 720 354 700 L362 400 Q402 330 400 250 Z"
    s = P_(body, color)
    s += DK("M620 330 Q622 360 662 400 L670 700 Q640 708 612 710 Z", 0.13)
    s += L_("M444 250 Q512 350 580 250", 9)
    s += L_("M420 262 Q418 330 380 400 M604 262 Q606 330 644 400", 6, OL, 0.35)
    s += L_("M364 660 Q512 676 664 660", 6, OL, 0.4)
    s += "".join(f'<circle cx="{x}" cy="{y}" r="11" fill="#fbf6ec" opacity="0.8"/>' for x, y in ((440, 470), (560, 440), (500, 560), (610, 580), (420, 620), (560, 650)))
    s += HL("M388 420 L382 660", 14, 0.25)
    return s


def at(svg_str, cx, cy, s, ox=512, oy=512, op=1.0):
    o = f' opacity="{op}"' if op < 1 else ""
    return f'<g transform="translate({cx} {cy}) scale({s}) translate({-ox} {-oy})"{o}>{svg_str}</g>'


GROUND = lambda y=880, rx=260: SH(512, y, rx, 30)

out["Clothes/T_Shirt"] = svg("sky", GROUND(830, 220) + tee("#4f9a9a"))
out["Clothes/Polo shirt"] = svg("sage", GROUND(830, 220) + polo("#6f9bb0"))
out["Clothes/Shirt"] = svg("rose", GROUND(850, 240) + shirt("#8fb4cf"))
out["Clothes/Sweater"] = svg("sky", GROUND(866, 240) + sweater())
out["Clothes/Dress"] = svg("rose", GROUND(900, 250) + at(dress(), 512, 500, 0.92, 512, 520))
out["Clothes/Skirt"] = svg("beige", GROUND(790, 240) + at(skirt(), 512, 520, 1.12, 512, 520))
out["Clothes/Pants"] = svg("sage", GROUND(836, 200) + at(pants_casual(), 512, 510, 1.05, 512, 520))
out["Clothes/Trousers"] = svg("beige", GROUND(868, 200) + at(trousers_formal(), 512, 520, 0.98, 512, 540))
out["Clothes/Jeans"] = svg("sand", GROUND(862, 200) + at(jeans(), 512, 520, 0.98, 512, 536))
out["Clothes/Short pants"] = svg("sky", GROUND(760, 220) + at(shorts(), 512, 520, 1.15, 512, 460))

# Top (clothes): outfit with the top highlighted, bottom faded
b = halo(512, 390, 330) + GROUND(900, 190)
b += at(trousers_formal("#5c3d2e"), 512, 700, 0.62, 512, 540, op=0.3)
b += at(tank_top("#e8a0a8"), 512, 390, 0.95, 512, 470)
b += '<g transform="translate(790 400) rotate(90)">' + arrow_v(0, -80, 80, "#e0b04f") + "</g>"
b += star(230, 300, 24) + star(270, 470, 16)
out["Clothes/Top_clothes_"] = svg("rose", b)

# Clothes: rack with several garments
b = SH(512, 870, 330, 30)
rail_y = 240
b += L_(f"M220 {rail_y} L220 850 M804 {rail_y} L804 850", 30) + L_(f"M220 {rail_y} L220 850 M804 {rail_y} L804 850", 14, "#9a6a45")
b += L_("M170 850 L270 850 M754 850 L854 850", 30) + L_("M170 850 L270 850 M754 850 L854 850", 14, "#9a6a45")
b += L_(f"M190 {rail_y} L834 {rail_y}", 30) + L_(f"M190 {rail_y} L834 {rail_y}", 14, "#c9c3bb")
items = [(322, sweater("#7fa05a", "#f6ecd8", "#c8574b"), 0.47), (446, shirt("#c8574b"), 0.45), (576, dress("#e0b04f"), 0.5),
         (712, hanger(512, 262, 140) + '<g transform="translate(0 40)">' + jeans() + '</g>', 0.5)]
for x, g, sc in items:
    if "hanger" not in g[:10] and not g.startswith("<path d=\"M512 222"):
        pass
    body = g if g is items[2][1] else hanger(512, 262, 140) + g
    if g is items[3][1]:
        body = g
    b += at(body, x, rail_y + 6, sc, 512, 160)
# boxes of shoes at the bottom
b += "<g transform=\"translate(140 0)\">" + P_("M330 800 Q338 770 372 770 L398 784 Q432 788 440 812 L440 836 L330 836 Z", "#8a5a86", 12) + L_("M330 818 L440 818", 7, OL, 0.5) + "</g>"
b += P_("M600 790 Q610 760 650 760 L680 776 Q720 780 730 810 L730 836 L600 836 Z", "#8a5a86", 12) + L_("M600 818 L730 818", 7, OL, 0.5)
out["Clothes/Clothes"] = svg("beige", b)


# ---------------- Colors ----------------
def smooth_closed(points):
    n = len(points)
    d = f"M{points[0][0]:.0f} {points[0][1]:.0f}"
    for i in range(n):
        p0, p1, p2, p3 = points[i - 1], points[i], points[(i + 1) % n], points[(i + 2) % n]
        c1 = (p1[0] + (p2[0] - p0[0]) / 6, p1[1] + (p2[1] - p0[1]) / 6)
        c2 = (p2[0] - (p3[0] - p1[0]) / 6, p2[1] - (p3[1] - p1[1]) / 6)
        d += f" C{c1[0]:.0f} {c1[1]:.0f} {c2[0]:.0f} {c2[1]:.0f} {p2[0]:.0f} {p2[1]:.0f}"
    return d + " Z"


def splat_path(cx, cy, r, seed=1, n=18, spike=0.16):
    pts_ = []
    for i in range(n):
        a = 2 * math.pi * i / n + seed * 0.3
        k = 1 + 0.1 * math.sin(3 * a + seed) + 0.07 * math.sin(5 * a + 2 * seed)
        k *= (1 + spike * 0.8) if i % 2 == 0 else (1 - spike * 0.5)
        if i % 6 == 0:
            k *= 1 + spike * 0.8
        pts_.append((cx + r * k * math.cos(a), cy + r * k * 0.9 * math.sin(a)))
    return smooth_closed(pts_)


def mix(c, t, other):
    c = c.lstrip("#"); other = other.lstrip("#")
    r = [int(c[i:i + 2], 16) for i in (0, 2, 4)]
    o = [int(other[i:i + 2], 16) for i in (0, 2, 4)]
    return "#" + "".join(f"{round(a + (b - a) * t):02x}" for a, b in zip(r, o))


def splat(cx, cy, r, color, seed=1, drops=True, hl=True):
    s = f'<ellipse cx="{cx + 10}" cy="{cy + r * 0.95:.0f}" rx="{r * 0.9:.0f}" ry="{r * 0.12:.0f}" fill="{OL}" opacity="0.1"/>'
    s += P_(splat_path(cx, cy, r, seed), color, 14)
    if hl:
        s += f'<path d="{splat_path(cx - r * 0.18, cy - r * 0.2, r * 0.42, seed + 2, 14, 0.15)}" fill="#fff" opacity="{0.28 if color not in ("#fbf8f2", "#2a2320") else (0 if color == "#fbf8f2" else 0.12)}"/>'
        s += f'<path d="{splat_path(cx + r * 0.12, cy + r * 0.15, r * 0.6, seed + 3, 14, 0.12)}" fill="{OL}" opacity="0.1"/>'
    if drops:
        for a, dist, rr in ((0.4, 1.45, 0.09), (1.9, 1.4, 0.07), (3.6, 1.5, 0.1), (4.6, 1.38, 0.06), (5.5, 1.5, 0.05)):
            x = cx + r * dist * math.cos(a + seed)
            y = cy + r * dist * 0.9 * math.sin(a + seed)
            s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r * rr:.0f}" fill="{color}" stroke="{OL}" stroke-width="9"/>'
    return s


def brush(tipx, tipy, rot, color, s=1.0, handle="#d8b48a"):
    g = f'<g transform="translate({tipx} {tipy}) rotate({rot}) scale({s})">'
    g += P_("M-24 -236 L24 -236 L16 -520 Q0 -548 -16 -520 Z", handle, 12)
    g += HL("M-8 -260 L-6 -500", 8, 0.35)
    g += f'<rect x="-44" y="-246" width="88" height="96" rx="10" fill="#c9c3bb" stroke="{OL}" stroke-width="12"/>'
    g += L_("M-44 -220 L44 -220 M-44 -196 L44 -196", 6, OL, 0.35) + HL("M-26 -236 L-26 -160", 10, 0.5)
    g += P_("M-42 -150 L42 -150 Q52 -70 6 4 Q-2 10 -8 2 Q-52 -70 -42 -150 Z", "#e6d3b0", 12)
    g += f'<path d="M-46 -100 Q0 -92 46 -100 Q46 -60 6 4 Q-2 10 -8 2 Q-46 -60 -46 -100 Z" fill="{color}" stroke="{OL}" stroke-width="12" stroke-linejoin="round"/>'
    g += L_("M-18 -146 L-14 -100 M14 -146 L10 -100", 5, OL, 0.35)
    g += "</g>"
    return g


COLORS = {
    "Red": ("#c8423a", "sage", 1),
    "Orange": ("#e07a38", "sky", 2),
    "Yellow": ("#ecc443", "sky", 3),
    "Green": ("#5f9a4e", "rose", 4),
    "Blue": ("#4a80c4", "sand", 5),
    "Dark blue": ("#253b73", "sand", 6),
    "Purple": ("#7d4e9a", "sage", 7),
    "Pink": ("#ea8cab", "sage", 8),
    "Brown": ("#7a4e30", "sky", 9),
    "Black": ("#2a2320", "sand", 10),
    "White": ("#fbf8f2", "rose", 11),
    "Grey": ("#9a9794", "sand", 12),
}
for name, (c, bg, seed) in COLORS.items():
    b = splat(480, 560, 235, c, seed)
    handle = "#d8b48a"
    b += brush(560, 540, 36, c, 0.95, handle)
    out[f"Colors/{name}"] = svg(bg, b)

# Color: palette with many colours + brush
pal = "M300 330 Q420 230 600 250 Q800 280 830 470 Q850 620 720 700 Q640 750 620 680 Q600 620 540 650 Q470 690 500 760 Q520 820 420 810 Q250 790 210 620 Q180 440 300 330 Z"
b = SH(520, 850, 300, 34) + P_(pal, "#d9a86c", 14)
b += DK("M720 700 Q850 620 830 470 Q820 600 700 670 Z", 0.18) + HL("M300 360 Q400 280 540 276", 16, 0.35)
b += f'<ellipse cx="610" cy="580" rx="46" ry="38" fill="{mix("#2e211b", 0, "#2e211b")}" opacity="0.001"/>'
b += f'<ellipse cx="400" cy="640" rx="44" ry="38" fill="url(#bg)" stroke="{OL}" stroke-width="12"/>'  # thumb hole
wells = [("#c8423a", 330, 470), ("#e07a38", 400, 360), ("#ecc443", 520, 330), ("#5f9a4e", 640, 350), ("#4a80c4", 740, 440),
         ("#7d4e9a", 740, 570), ("#ea8cab", 540, 470), ("#fbf8f2", 300, 590)]
for col, x, y in wells:
    b += P_(splat_path(x, y, 44, x % 7, 12, 0.18), col, 11) + f'<circle cx="{x - 14}" cy="{y - 14}" r="10" fill="#fff" opacity="0.35"/>'
b += brush(610, 600, 30, "#4a80c4", 0.8)
out["Colors/Color"] = svg("beige", b)

# Dark / Light: three hues, target shade row highlighted, other shade faded
BASE = ["#c8423a", "#5f9a4e", "#4a80c4"]
for key, bg in (("Dark", "sand"), ("Light", "sky")):
    darks = [mix(c, 0.5, "#1a1210") for c in BASE]
    lights = [mix(c, 0.6, "#ffffff") for c in BASE]
    tgt, oth = (darks, lights) if key == "Dark" else (lights, darks)
    b = halo(512, 420, 380)
    for i, c in enumerate(oth):
        b += f'<g opacity="0.33">' + splat(262 + i * 250, 770, 80, c, i + 3, drops=False) + "</g>"
    for i, c in enumerate(tgt):
        b += splat(262 + i * 250, 420, 118, c, i + 1, drops=False)
    b += sparkles(512, 230, 330) if False else star(150, 250, 22) + star(880, 260, 18)
    # arrow from faded row to highlighted row
    b += arrow_v(512, 690, 560, "#e0b04f", 22) if False else ""
    out[f"Colors/{key}"] = svg(bg, b)


# ---------------- Boss / Chief / Colleague ----------------
SUIT = "#3e4f70"


def suit_top(tie="#c8574b", suit=SUIT, shirt_c="#fbf8f2"):
    return (f'<path d="M-40 -388 L40 -388 L0 -290 Z" fill="{shirt_c}" stroke="{OL}" stroke-width="9" stroke-linejoin="round"/>'
            f'<path d="M-10 -376 L10 -376 L14 -364 L22 -290 L0 -262 L-22 -290 L-14 -364 Z" fill="{tie}" stroke="{OL}" stroke-width="7" stroke-linejoin="round"/>'
            f'<path d="M-40 -388 L0 -290 L-30 -300 L-60 -340 Z" fill="{suit}" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/>'
            f'<path d="M40 -388 L0 -290 L30 -300 L60 -340 Z" fill="{suit}" stroke="{OL}" stroke-width="8" stroke-linejoin="round"/>'
            f'<path d="M-40 -388 L-26 -370 L-50 -352 M40 -388 L26 -370 L50 -352" fill="none" stroke="{OL}" stroke-width="6" opacity="0.5"/>'
            f'<rect x="-76" y="-300" width="34" height="10" rx="3" fill="#fbf8f2" stroke="{OL}" stroke-width="5"/>'
            f'<circle cx="0" cy="-236" r="6" fill="{OL}"/>')


def badge(color="#4f9a9a"):
    return (f'<path d="M-30 -386 L-40 -300 M30 -386 L40 -300" stroke="{color}" stroke-width="8" fill="none"/>'
            f'<rect x="-44" y="-304" width="44" height="54" rx="6" fill="#fbf8f2" stroke="{OL}" stroke-width="7" transform="translate(22 0)"/>'
            f'<rect x="-14" y="-294" width="28" height="16" rx="3" fill="{color}"/>')


def desk(x0, x1, top, bottom, color="#9a6a45"):
    s = SH((x0 + x1) / 2, bottom + 6, (x1 - x0) / 2 + 30, 26)
    s += P_(f"M{x0} {top} L{x1} {top} L{x1} {top + 40} L{x0} {top + 40} Z", mix(color, 0.2, "#ffffff"), 14)
    s += P_(f"M{x0 + 20} {top + 40} L{x1 - 20} {top + 40} L{x1 - 20} {bottom} L{x0 + 20} {bottom} Z", color, 14)
    s += DK(f"M{x0 + 20} {top + 40} L{x1 - 20} {top + 40} L{x1 - 20} {top + 64} L{x0 + 20} {top + 64} Z", 0.2)
    return s


def laptop_back(cx, by, w=200, h=140, rot=0):
    return (f'<g transform="rotate({rot} {cx} {by})">' + P_(f"M{cx - w / 2} {by} L{cx - w / 2 + 8} {by - h} L{cx + w / 2 - 8} {by - h} L{cx + w / 2} {by} Z", "#c9c3bb", 12)
            + f'<circle cx="{cx}" cy="{by - h / 2}" r="16" fill="#fbf8f2" opacity="0.7"/>'
            + P_(f"M{cx - w / 2 - 14} {by} L{cx + w / 2 + 14} {by} L{cx + w / 2 + 6} {by + 14} L{cx - w / 2 - 6} {by + 14} Z", "#9a9794", 9) + "</g>")


def chart_frame(x, y, w, h):
    s = P_(f"M{x} {y} L{x + w} {y} L{x + w} {y + h} L{x} {y + h} Z", "#fbf8f2", 12)
    bw = w / 6
    for i, hh in enumerate((0.3, 0.5, 0.45, 0.75)):
        bx = x + bw * (0.8 + i * 1.2)
        s += f'<rect x="{bx:.0f}" y="{y + h - 20 - (h - 50) * hh:.0f}" width="{bw * 0.8:.0f}" height="{(h - 50) * hh:.0f}" fill="{("#7fa05a", "#4f9a9a", "#e0b04f", "#c8574b")[i]}" stroke="{OL}" stroke-width="6"/>'
    s += L_(f"M{x + 20} {y + h - 40} L{x + w * 0.45} {y + h * 0.45} L{x + w * 0.6} {y + h * 0.55} L{x + w - 26} {y + 26}", 9, "#c8574b")
    return s


# Boss: at a big desk, suit & tie, executive chair, chart on wall
b = chart_frame(660, 160, 210, 160)
b += P_("M340 330 Q340 250 420 246 L604 246 Q684 250 684 330 L690 700 L334 700 Z", "#5c3d2e", 14)  # chair back
b += HL("M372 300 L372 640", 16, 0.18)
p, _ = person(512, 900, 1.0, hair="neat", top=SUIT, sleeve="long", torso="plain", skin=SKIN2, arms=("outlow", "outlow"),
              extra_top=suit_top("#c8574b"), glasses=False, mustache=True, legs_hidden=True, shadow=False)
b += p
b += desk(170, 854, 640, 860, "#7a4e30")
b += f'<circle cx="{512 - 190}" cy="648" r="26" fill="{SKIN2}" stroke="{OL}" stroke-width="10"/><circle cx="{512 + 190}" cy="648" r="26" fill="{SKIN2}" stroke="{OL}" stroke-width="10"/>'
# nameplate, papers, pen cup, phone
b += P_("M360 612 L470 612 L480 644 L350 644 Z", "#e0b04f", 10) + L_("M378 628 L452 628", 6, OL, 0.35)
b += P_("M220 614 L300 614 L306 640 L214 640 Z", "#fbf8f2", 9) + P_("M226 600 L296 600 L300 614 L220 614 Z", "#f6ecd8", 8)
b += P_("M740 576 L788 576 L784 640 L744 640 Z", "#4f9a9a", 10) + L_("M752 576 L744 540 M766 576 L770 530 M778 576 L792 548", 8)
b += P_("M600 490 L620 470 L636 486 L616 506 Z", "#e0b04f", 0) if False else ""
out["FamilyAndPeople/Boss"] = svg("sky", b)

# Chief: the head/leader in front, team faded behind
b = ""
team = [(230, "bob", "#d9825b", SKIN, "skirt", "#5c3d2e"), (372, "short", "#7fa05a", SKIN2, "pants", "#5c3d2e"),
        (652, "ponytail", "#8a5a86", SKIN, "skirt", "#3b2a22"), (794, "spiky", "#e0b04f", SKIN3, "pants", "#5d82a8")]
for x, hs, tc, sk, bt, bc in team:
    p, _ = person(x, 760, 0.7, hair=hs, top=tc, skin=sk, bottom=bt, bc=bc, torso="collar", arms=("down", "down"),
                  extra_top=badge("#4f9a9a"), opacity=0.38)
    b += p
b += halo(512, 560, 330)
p, _ = person(512, 900, 1.02, hair="neat", top=SUIT, sleeve="long", torso="plain", skin=SKIN2, bottom="pants", bc=SUIT,
              arms=("hip", "up"), extra_top=suit_top("#e0b04f"), shoes="#3b2a22")
b += p
# star above raised hand
b += star(660, 300, 42) + star(740, 240, 18) + star(580, 250, 14)
out["FamilyAndPeople/Chief"] = svg("sand", b)

# Colleague: two coworkers side by side at desks with laptops
b = ""
b += P_("M130 170 L894 170 L894 430 L130 430 Z", "#cfe0ea", 12)  # window
b += L_("M512 170 L512 430 M130 300 L894 300", 10) + HL("M170 200 L260 200", 14, 0.5)
b += P_("M200 430 L250 330 L290 430 Z", "#bcccd6", 0) if False else ""
p1, _ = person(320, 900, 1.0, legs_hidden=True, shadow=False, hair="bob", top="#d9825b", torso="vneck", sleeve="long", arms=("down", "down"), extra_top=badge("#4f9a9a"))
p2, _ = person(704, 900, 1.0, legs_hidden=True, shadow=False, hair="neat", top="#8fb4cf", torso="collar", collar="#fbf8f2", skin=SKIN2, sleeve="short",
               arms=("down", "wave"), extra_top=badge("#4f9a9a"))
b += p1 + p2
b += desk(120, 904, 660, 870, "#9a6a45")
b += laptop_back(360, 656, 170, 108) + laptop_back(664, 656, 170, 108)
b += P_("M140 600 L200 600 L194 656 L146 656 Z", "#fbf8f2", 10) + L_("M200 612 Q222 616 220 632 Q218 646 198 646", 8)
b += P_("M810 620 L870 620 L862 656 L818 656 Z", "#d9825b", 10) + P_("M840 620 Q800 560 812 520 Q840 560 840 620 Q860 560 890 540 Q880 590 840 620 Z", "#5f8f4e", 9)
out["FamilyAndPeople/Colleague"] = svg("sky", b)


for k, v in out.items():
    path = os.path.join(ROOT, "svg", k + ".svg")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(v)
print(len(out), "written")

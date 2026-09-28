import math, random
from common import *
random.seed(7)
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def hl(d,w=16,op=0.3): return f'<path d="{d}" fill="none" stroke="#ffffff" stroke-width="{w}" opacity="{op}"/>'
def dk(d,w=16,op=0.15): return f'<path d="{d}" fill="none" stroke="#2e211b" stroke-width="{w}" opacity="{op}"/>'
def sl(d,w,col,o=12): return f'<path d="{d}" fill="none" stroke-width="{w+o}"/><path d="{d}" fill="none" stroke="{col}" stroke-width="{w}"/>'
def leaf(x,y,rot,s=1,col="#5f8f4e"):
    return f'<g transform="translate({x} {y}) rotate({rot}) scale({s})"><path d="M0 0 Q40 -45 110 -30 Q60 20 0 0Z" fill="{col}" stroke-width="{10/s:.0f}"/><path d="M8 -3 Q50 -18 100 -28" fill="none" stroke-width="{6/s:.0f}" opacity="0.5"/></g>'
def plate(x,y,rx,ry,col="#f6ecd8",inner="#e8dcc6"):
    return f'{sh(x,y+ry*0.55,rx+10,ry*0.6)}<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="{col}" stroke-width="16"/><ellipse cx="{x}" cy="{y-ry*0.05:.0f}" rx="{rx*0.76:.0f}" ry="{ry*0.7:.0f}" fill="{inner}" stroke-width="8"/>'
def board(x,y,w,h):
    return (f'{sh(x,y+h/2+10,w/2+20,30)}'
            f'<rect x="{x+w/2-10}" y="{y-35}" width="120" height="70" rx="35" fill="#9a6a45" stroke-width="14"/><circle cx="{x+w/2+70}" cy="{y}" r="14" fill="#c9ab93" stroke-width="8"/>'
            f'<rect x="{x-w/2}" y="{y-h/2}" width="{w}" height="{h}" rx="50" fill="#c38a58" stroke-width="16"/>'
            f'<rect x="{x-w/2+25}" y="{y-h/2+25}" width="{w-50}" height="{h-50}" rx="35" fill="none" stroke-width="6" opacity="0.2"/>'
            f'{hl(f"M{x-w/2+30} {y+h/2-60} L{x-w/2+30} {y-h/2+60}",14,0.3)}')

# ---------- animal badges ----------
def badge(x,y,kind,r=95):
    s=f'<circle cx="{x}" cy="{y}" r="{r}" fill="#f6ecd8" stroke-width="12"/><circle cx="{x}" cy="{y}" r="{r-16}" fill="none" stroke-width="5" opacity="0.25"/>'
    g=f'<g transform="translate({x} {y}) scale({r/95:.3f})">'
    if kind=="cow":
        g+=('<path d="M-38 -30 Q-80 -40 -78 -80 Q-60 -55 -32 -48Z" fill="#f6ecd8" stroke-width="7"/><path d="M38 -30 Q80 -40 78 -80 Q60 -55 32 -48Z" fill="#f6ecd8" stroke-width="7"/>'
            '<ellipse cx="-58" cy="-18" rx="24" ry="13" transform="rotate(20 -58 -18)" fill="#9a6a45" stroke-width="7"/><ellipse cx="58" cy="-18" rx="24" ry="13" transform="rotate(-20 58 -18)" fill="#9a6a45" stroke-width="7"/>'
            '<path d="M-42 -40 Q0 -62 42 -40 Q52 10 36 38 L-36 38 Q-52 10 -42 -40Z" fill="#9a6a45" stroke-width="8"/>'
            '<ellipse cx="0" cy="42" rx="44" ry="28" fill="#e8b2a0" stroke-width="8"/>'
            '<circle cx="-15" cy="42" r="6" fill="#2e211b" stroke="none"/><circle cx="15" cy="42" r="6" fill="#2e211b" stroke="none"/>'
            '<circle cx="-20" cy="-8" r="7" fill="#2e211b" stroke="none"/><circle cx="20" cy="-8" r="7" fill="#2e211b" stroke="none"/>'
            '<path d="M-14 -44 Q0 -30 14 -44" fill="#f6ecd8" stroke-width="5"/>')
    elif kind=="pig":
        g+=('<path d="M-50 -30 L-58 -70 L-20 -52Z" fill="#e8a39a" stroke-width="8"/><path d="M50 -30 L58 -70 L20 -52Z" fill="#e8a39a" stroke-width="8"/>'
            '<circle cx="0" cy="0" r="58" fill="#f0b8ae" stroke-width="8"/>'
            '<ellipse cx="0" cy="18" rx="28" ry="20" fill="#e38f86" stroke-width="7"/>'
            '<ellipse cx="-9" cy="18" rx="5" ry="8" fill="#2e211b" stroke="none"/><ellipse cx="9" cy="18" rx="5" ry="8" fill="#2e211b" stroke="none"/>'
            '<circle cx="-22" cy="-16" r="7" fill="#2e211b" stroke="none"/><circle cx="22" cy="-16" r="7" fill="#2e211b" stroke="none"/>'
            '<circle cx="-36" cy="10" r="9" fill="#e8907f" opacity="0.5" stroke="none"/><circle cx="36" cy="10" r="9" fill="#e8907f" opacity="0.5" stroke="none"/>')
    elif kind=="chicken":
        g+=('<path d="M-22 -48 Q-30 -78 -8 -74 Q-2 -96 16 -80 Q34 -88 32 -62 Q46 -56 30 -44Z" fill="#c8574b" stroke-width="7"/>'
            '<circle cx="0" cy="0" r="52" fill="#ffffff" stroke-width="8"/>'
            '<path d="M-10 20 L10 20 L2 52 Q-6 56 -8 44Z" fill="#c8574b" stroke-width="6"/>'
            '<path d="M-18 6 L18 6 L0 30Z" fill="#e0a04f" stroke-width="7"/>'
            '<circle cx="-20" cy="-12" r="7" fill="#2e211b" stroke="none"/><circle cx="20" cy="-12" r="7" fill="#2e211b" stroke="none"/>')
    return s+g+'</g>'

# ---------- Beef: raw steak on board + cow badge ----------
save("Food","Beef","rose",f'''
{board(470,620,560,330)}
<path d="M250 610 C240 500 360 440 470 470 C560 420 700 450 700 560 C720 680 600 740 470 720 C340 750 255 700 250 610Z" fill="#f3dcc8" stroke-width="16"/>
<path d="M290 610 C285 525 380 480 470 505 C555 465 665 490 665 565 C680 660 590 700 475 688 C360 712 295 670 290 610Z" fill="#b8453d" stroke-width="8"/>
<g fill="none" stroke="#f3dcc8" stroke-width="9" opacity="0.9"><path d="M340 580 Q380 560 420 590 Q450 610 490 590"/><path d="M520 540 Q560 560 600 540"/><path d="M380 650 Q430 640 470 660 Q520 670 560 640"/><path d="M560 610 Q600 600 630 620"/></g>
<path d="M430 600 Q470 520 560 530" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.2"/>
<path d="M300 570 C310 520 360 490 420 490" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.5"/>
<path d="M650 360 L700 330" stroke-width="0"/>
{badge(740,300,"cow",105)}
''')

# ---------- Boiled egg ----------
def egg_half(x,y,rot,s=1):
    return (f'<g transform="translate({x} {y}) rotate({rot}) scale({s})">'
            f'<ellipse cx="0" cy="0" rx="120" ry="88" fill="#fbf4e6" stroke-width="{14/s:.0f}"/>'
            f'<circle cx="8" cy="4" r="52" fill="#f2b33c" stroke-width="{9/s:.0f}"/>'
            f'<circle cx="8" cy="4" r="30" fill="#f6c95a" stroke="none"/>'
            f'<path d="M-95 -20 Q-85 -60 -40 -72" fill="none" stroke="#ffffff" stroke-width="{14/s:.0f}"/></g>')
save("Food","Boiled egg","sand",f'''
{sh(512,760,330,45)}
{leaf(250,700,-20,2.2,"#7fa05a")}{leaf(760,720,200,2.0,"#5f8f4e")}
<ellipse cx="560" cy="500" rx="120" ry="160" transform="rotate(20 560 500)" fill="#fbf4e6" stroke-width="16"/>
{hl("M490 420 Q500 380 540 360",18,0.8)}
{dk("M660 470 Q670 560 610 630",20,0.08)}
{egg_half(400,650,-8)}{egg_half(640,680,6,0.95)}
''')

# ---------- Cabbage ----------
save("Food","Cabbage","sage",f'''
{sh(512,820,300,40)}
<path d="M230 600 Q180 470 280 400 Q260 300 380 290 Q430 200 530 240 Q640 210 680 310 Q800 320 790 440 Q860 540 780 640 Q760 760 620 780 Q512 830 400 780 Q260 770 230 600Z" fill="#7fa05a" stroke-width="16"/>
<path d="M260 560 Q240 460 330 430 M720 400 Q800 470 760 580 M420 300 Q480 260 560 280" fill="none" stroke-width="8" opacity="0.35"/>
<circle cx="512" cy="560" r="210" fill="#b7d38a" stroke-width="14"/>
<path d="M330 520 Q400 360 560 360 Q690 380 720 520 Q650 440 540 440 Q420 440 330 520Z" fill="#a4c774" stroke-width="10"/>
<path d="M512 770 Q480 640 520 450 M512 700 Q430 640 380 580 M515 640 Q600 590 660 560 M505 560 Q450 520 420 480 M520 520 Q580 490 620 470" fill="none" stroke="#eef4d8" stroke-width="10"/>
<path d="M512 770 Q480 640 520 450" fill="none" stroke-width="4" opacity="0.3"/>
{hl("M340 620 Q340 500 420 440",18,0.35)}
{dk("M690 620 Q650 720 560 760",18,0.12)}
''')

# ---------- Carrot ----------
save("Food","Carrot","sand",f'''
{sh(512,840,260,30)}
<g transform="rotate(-35 512 540)">
<path d="M512 330 Q470 230 420 180" fill="none" stroke-width="30"/><path d="M512 330 Q470 230 420 180" fill="none" stroke="#5f8f4e" stroke-width="14"/>
<path d="M512 330 Q515 220 512 150" fill="none" stroke-width="30"/><path d="M512 330 Q515 220 512 150" fill="none" stroke="#7fa05a" stroke-width="14"/>
<path d="M512 330 Q560 230 610 190" fill="none" stroke-width="30"/><path d="M512 330 Q560 230 610 190" fill="none" stroke="#5f8f4e" stroke-width="14"/>
{leaf(420,190,-120,1.1,"#5f8f4e")}{leaf(512,160,-80,1.1,"#7fa05a")}{leaf(606,200,-50,1.1,"#5f8f4e")}{leaf(470,250,-150,0.8,"#7fa05a")}{leaf(560,250,-20,0.8,"#7fa05a")}
<path d="M420 350 Q512 300 604 350 Q600 520 540 800 Q512 880 484 800 Q424 520 420 350Z" fill="#e0823f" stroke-width="16"/>
<path d="M440 440 L480 445 M560 400 L595 395 M450 540 L490 548 M540 600 L575 590 M470 690 L500 695 M520 750 L545 742" stroke-width="8" opacity="0.6"/>
{hl("M455 380 Q450 500 480 700",18,0.35)}
{dk("M580 380 Q575 520 530 760",16,0.15)}
</g>
''')

# ---------- Chicken: raw drumsticks + chicken badge ----------
def drum(x,y,rot,s=1):
    return (f'<g transform="translate({x} {y}) rotate({rot}) scale({s})">'
            f'<path d="M60 -18 L200 -14 L200 14 L60 18Z" fill="#f6ecd8" stroke-width="{14/s:.0f}"/>'
            f'<circle cx="215" cy="-22" r="26" fill="#f6ecd8" stroke-width="{12/s:.0f}"/><circle cx="215" cy="22" r="26" fill="#f6ecd8" stroke-width="{12/s:.0f}"/><path d="M200 -14 L200 14" stroke="#f6ecd8" stroke-width="20"/>'
            f'<path d="M-150 0 C-150 -95 -40 -115 40 -50 Q80 -25 90 0 Q80 25 40 50 C-40 115 -150 95 -150 0Z" fill="#f2c7ae" stroke-width="{16/s:.0f}"/>'
            f'<path d="M-120 -20 C-110 -70 -50 -85 0 -60" fill="none" stroke="#ffffff" stroke-width="{16/s:.0f}" opacity="0.45"/>'
            f'<path d="M-100 50 Q-30 70 30 40" fill="none" stroke="#d99a86" stroke-width="{12/s:.0f}" opacity="0.7"/>'
            f'<g fill="#e3a791" stroke="none"><circle cx="-80" cy="10" r="6"/><circle cx="-40" cy="-20" r="6"/><circle cx="-20" cy="25" r="6"/><circle cx="-100" cy="-35" r="5"/><circle cx="20" cy="-5" r="5"/></g></g>')
save("Food","Chicken","sage",f'''
{plate(490,650,330,150)}
{drum(420,610,-15)}{drum(560,690,10,0.95)}
{leaf(280,690,160,0.9,"#7fa05a")}
{badge(760,300,"chicken",105)}
''')

# ---------- Chili ----------
save("Food","Chili","rose",f'''
{sh(512,720,300,36)}
{chili(250,330,-35,1.05)}
{chili(430,270,-18,1.15,"#7fa05a")}
{chili(620,300,-2,1.05,"#c8574b")}
''')

# ---------- Chinese kale (kana) ----------
def kale_stalk(x0,y0,x1,y1,lw=1,col="#5f8f4e"):
    mx,my=(x0+x1)/2,(y0+y1)/2
    ang=math.degrees(math.atan2(y1-y0,x1-x0))
    return (sl(f"M{x0} {y0} Q{mx+10} {my} {x1} {y1}",26,"#c9dca0")+
            f'<g transform="translate({x1} {y1}) rotate({ang+90})"><path d="M0 40 C-110 20 -130 -130 -60 -200 C-30 -230 30 -230 60 -200 C130 -130 110 20 0 40Z" transform="scale({lw})" fill="{col}" stroke-width="{14/lw:.0f}"/>'
            f'<path d="M0 40 L0 -180 M0 -40 L-50 -90 M0 -40 L50 -90 M0 -110 L-40 -150 M0 -110 L40 -150" transform="scale({lw})" fill="none" stroke="#c9dca0" stroke-width="{8/lw:.0f}"/>'
            f'<path d="M-70 -60 Q-80 -150 -30 -190" transform="scale({lw})" fill="none" stroke="#ffffff" stroke-width="{10/lw:.0f}" opacity="0.25"/></g>')
save("Food","Chinese kale","sage",f'''
{sh(512,850,220,30)}
{kale_stalk(505,830,300,380,0.95,"#4f7f47")}
{kale_stalk(519,830,730,380,0.95,"#4f7f47")}
{kale_stalk(500,830,400,300,1.0,"#5f8f4e")}
{kale_stalk(524,830,630,300,1.0,"#5f8f4e")}
{kale_stalk(512,830,512,260,1.05,"#6a9a55")}
<rect x="455" y="690" width="114" height="44" rx="14" fill="#c8574b" stroke-width="12"/>
''')

# ---------- Crab ----------
def legs(side):
    s=""
    for i,(dy,ang) in enumerate([(-10,-20),(25,0),(60,20),(90,40)]):
        x0=512+side*150; y0=560+dy
        x1=x0+side*110; y1=y0+ (dy-10)*0.6 -30
        x2=x1+side*60; y2=y1+110
        s+=sl(f"M{x0} {y0} L{x1} {y1} L{x2} {y2}",24,"#d9674b",14)
    return s
def claw(side):
    x=512+side*230
    return (sl(f"M{512+side*120} 500 Q{512+side*200} 440 {x} 380",34,"#d9674b",14)+
            f'<g transform="translate({x} 340) scale({side} 1)">'
            f'<path d="M-40 40 C-80 0 -60 -80 10 -90 C60 -95 90 -60 80 -30 L20 -30 L60 10 C40 50 -10 60 -40 40Z" fill="#d9674b" stroke-width="14"/>'
            f'<path d="M-45 10 C-50 -30 -30 -60 5 -70" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.35"/></g>')
save("Food","Crab","sky",f'''
{sh(512,760,300,36)}
{legs(-1)}{legs(1)}
{claw(-1)}{claw(1)}
<path d="M470 470 L455 400 M554 470 L569 400" stroke-width="14"/>
<circle cx="455" cy="392" r="22" fill="#ffffff" stroke-width="10"/><circle cx="569" cy="392" r="22" fill="#ffffff" stroke-width="10"/>
<circle cx="458" cy="394" r="10" fill="#2e211b" stroke="none"/><circle cx="566" cy="394" r="10" fill="#2e211b" stroke="none"/>
<path d="M330 540 C330 460 420 440 512 440 C604 440 694 460 694 540 C694 640 610 690 512 690 C414 690 330 640 330 540Z" fill="#d9674b" stroke-width="16"/>
<path d="M370 520 Q400 470 480 462" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.4"/>
<path d="M480 590 Q512 610 544 590" fill="none" stroke-width="9"/>
<circle cx="440" cy="560" r="16" fill="#e8907f" opacity="0.6" stroke="none"/><circle cx="584" cy="560" r="16" fill="#e8907f" opacity="0.6" stroke="none"/>
<g fill="#f0a07f" stroke="none"><circle cx="420" cy="620" r="8"/><circle cx="600" cy="625" r="8"/><circle cx="512" cy="500" r="9"/><circle cx="640" cy="510" r="7"/></g>
''')

# ---------- Crispy pork (moo krob) ----------
def mk(x,y,w=110,h=120,rot=0):
    top=y-h/2
    bub=''.join(f'<circle cx="{x-w/2+14+i*((w-28)/4):.0f}" cy="{top+14+(i%2)*8:.0f}" r="7" fill="#f3c46a" stroke="none"/>' for i in range(5))
    return (f'<g transform="rotate({rot} {x} {y})">'
            f'<rect x="{x-w/2}" y="{top}" width="{w}" height="{h}" rx="16" fill="#e8a38e" stroke-width="12"/>'
            f'<rect x="{x-w/2}" y="{top+h*0.45:.0f}" width="{w}" height="{h*0.18:.0f}" fill="#f6ecd8" stroke-width="6"/>'
            f'<rect x="{x-w/2}" y="{top+h*0.2:.0f}" width="{w}" height="{h*0.1:.0f}" fill="#f6ecd8" stroke="none"/>'
            f'<path d="M{x-w/2} {top+30} L{x-w/2} {top+16} Q{x-w/2} {top} {x-w/2+16} {top} L{x+w/2-16} {top} Q{x+w/2} {top} {x+w/2} {top+16} L{x+w/2} {top+30}Z" fill="#c9782f" stroke-width="12"/><path d="M{x-w/2+10} {top+34} L{x+w/2-10} {top+34}" stroke="#a85f25" stroke-width="6" opacity="0.6"/>'
            f'{bub}'
            f'<rect x="{x-w/2}" y="{top}" width="{w}" height="{h}" rx="16" fill="none" stroke-width="12"/></g>')
save("Food","Crispy pork","beige",f'''
{plate(490,650,360,175)}
<g transform="translate(495 650) scale(1.22) translate(-495 -650)">
{mk(330,610,105,125,-8)}{mk(440,600,105,125,-3)}{mk(550,600,105,125,3)}{mk(660,615,105,125,8)}
{mk(390,690,105,120,-4)}{mk(510,700,105,120,1)}{mk(625,695,105,120,5)}
</g>
<path d="M300 530 L780 530" stroke-width="0"/>
{leaf(790,700,-10,0.9,"#7fa05a")}
<ellipse cx="780" cy="420" rx="95" ry="40" fill="#f6ecd8" stroke-width="12"/>
<path d="M685 420 Q690 500 780 505 Q870 500 875 420" fill="#f6ecd8" stroke-width="12"/>
<ellipse cx="780" cy="425" rx="72" ry="26" fill="#6b3a26" stroke-width="7"/>
<g fill="#c8574b" stroke="none"><circle cx="760" cy="425" r="6"/><circle cx="795" cy="420" r="6"/><circle cx="780" cy="432" r="5"/></g>
''')

# ---------- Cucumber ----------
def cslice(x,y,r=58):
    seeds=''.join(f'<ellipse cx="{x+math.cos(a)*r*0.35:.0f}" cy="{y+math.sin(a)*r*0.35:.0f}" rx="6" ry="3" transform="rotate({math.degrees(a):.0f} {x+math.cos(a)*r*0.35:.0f} {y+math.sin(a)*r*0.35:.0f})" fill="#f6ecd8" stroke="none"/>' for a in [i*math.pi/3 for i in range(6)])
    return f'<circle cx="{x}" cy="{y}" r="{r}" fill="#4f7f47" stroke-width="12"/><circle cx="{x}" cy="{y}" r="{r-12}" fill="#d6e8a8" stroke="none"/><circle cx="{x}" cy="{y}" r="{r*0.5:.0f}" fill="#bcd68a" stroke="none"/>{seeds}'
save("Food","Cucumber","sage",f'''
{sh(512,790,320,36)}
<g transform="rotate(-35 480 540)">
<rect x="200" y="455" width="560" height="170" rx="85" fill="#5f8f4e" stroke-width="16"/>
<ellipse cx="200" cy="540" rx="14" ry="30" fill="#7fa05a" stroke-width="8"/>
{hl("M280 500 L680 500",18,0.35)}
{dk("M280 590 L680 590",16,0.15)}
<g fill="#a4c774" stroke="none"><circle cx="330" cy="530" r="7"/><circle cx="420" cy="560" r="7"/><circle cx="510" cy="525" r="7"/><circle cx="600" cy="565" r="7"/><circle cx="680" cy="535" r="7"/><circle cx="370" cy="580" r="6"/><circle cx="560" cy="580" r="6"/></g>
<ellipse cx="760" cy="540" rx="30" ry="85" fill="#d6e8a8" stroke-width="14"/>
</g>
{cslice(650,720)}{cslice(770,650,54)}{cslice(740,780,50)}
''')

# ---------- Fish ----------
scales=''.join(f'<path d="M{x} {y} q18 18 0 36" fill="none" stroke-width="6" opacity="0.3"/>' for x in range(420,640,50) for y in (470,520,570) )
save("Food","Fish","sky",f'''
{sh(512,780,320,34)}
<path d="M680 520 L840 410 Q815 520 840 640Z" fill="#d9825b" stroke-width="16"/>
<path d="M430 410 Q500 300 620 380 L600 420Z" fill="#d9825b" stroke-width="12"/>
<path d="M500 640 Q540 720 610 690 L590 630Z" fill="#d9825b" stroke-width="12"/>
<path d="M200 520 C260 400 420 380 560 400 C640 410 700 470 700 520 C700 570 640 630 560 640 C420 660 260 640 200 520Z" fill="#e8a38e" stroke-width="16"/>
<path d="M220 540 C300 620 450 640 600 610 C650 600 690 560 698 530 C650 580 560 600 450 600 C340 600 260 580 220 540Z" fill="#f6ecd8" stroke="none"/>
{scales}
<path d="M340 430 Q370 500 340 610" fill="none" stroke-width="10"/>
<circle cx="275" cy="490" r="26" fill="#ffffff" stroke-width="9"/><circle cx="270" cy="492" r="12" fill="#2e211b" stroke="none"/>
<path d="M205 540 Q225 548 240 540" fill="none" stroke-width="8"/>
<path d="M420 530 Q450 560 480 540 L470 510Z" fill="#d9825b" stroke-width="9"/>
{hl("M380 425 Q480 405 580 420",16,0.45)}
''')

# ---------- Fish sauce: tall glass bottle, amber, fish label ----------
def bottle(x,col,cap,label,tall=1.0,glass="#e8e2cf"):
    top=int(220+ (1-tall)*200)
    return (f'{sh(x,830,150,28)}'
            f'<path d="M{x-40} {top+60} L{x-40} {top+150} Q{x-140} {top+220} {x-140} {top+320} L{x-140} 800 Q{x-140} 830 {x-110} 830 L{x+110} 830 Q{x+140} 830 {x+140} 800 L{x+140} {top+320} Q{x+140} {top+220} {x+40} {top+150} L{x+40} {top+60}Z" fill="{glass}" stroke-width="16"/>'
            f'<path d="M{x-126} {top+330} Q{x} {top+310} {x+126} {top+330} L{x+126} 800 Q{x+126} 816 {x+110} 816 L{x-110} 816 Q{x-126} 816 {x-126} 800Z" fill="{col}" stroke-width="8"/>'
            f'<rect x="{x-50}" y="{top}" width="100" height="70" rx="14" fill="{cap}" stroke-width="14"/>'
            f'<path d="M{x-30} {top+15} L{x-30} {top+55} M{x} {top+15} L{x} {top+55} M{x+30} {top+15} L{x+30} {top+55}" stroke-width="6" opacity="0.3"/>'
            f'<rect x="{x-110}" y="{top+400}" width="220" height="210" rx="20" fill="#f6ecd8" stroke-width="12"/>'
            f'{label}'
            f'{hl(f"M{x-105} {top+340} L{x-105} 780",18,0.4)}'
            f'{hl(f"M{x-22} {top+80} L{x-22} {top+140}",10,0.5)}')
def fishicon(cx,cy,s=1,col="#d9825b"):
    return f'<g transform="translate({cx} {cy}) scale({s})"><path d="M40 0 L80 -30 L80 30Z" fill="{col}" stroke-width="8"/><path d="M-70 0 C-40 -45 20 -45 50 0 C20 45 -40 45 -70 0Z" fill="{col}" stroke-width="9"/><circle cx="-40" cy="-6" r="7" fill="#2e211b" stroke="none"/></g>'
save("Food","Fish sauce","sand",
     bottle(512,"#c47a2c","#5f8f4e",fishicon(512,725,1.1)+'<path d="M420 650 L604 650 M420 800 L604 800" stroke="#c8574b" stroke-width="10"/>',1.0,"#ecd3a4"))

# ---------- Fried egg ----------
save("Food","Fried egg","beige",f'''
{plate(512,580,330,230,"#5d82a8","#6f94ba")}
<path d="M300 560 C280 440 400 380 480 400 C560 350 700 380 720 470 C780 540 720 680 620 690 C540 760 400 740 360 680 C290 660 280 610 300 560Z" fill="#c98a4b" stroke-width="14"/>
<path d="M330 560 C315 460 410 415 482 432 C560 390 680 410 692 485 C740 545 700 650 612 660 C540 720 410 705 378 655 C320 640 315 600 330 560Z" fill="#fbf6ea" stroke="none"/>
<circle cx="520" cy="540" r="92" fill="#f2a93c" stroke-width="14"/>
<circle cx="495" cy="515" r="26" fill="#ffffff" opacity="0.55" stroke="none"/>
{hl("M360 600 Q370 500 440 460",14,0.6)}
''')

# ---------- Fried rice ----------
rice=''.join(f'<ellipse cx="{x}" cy="{y}" rx="11" ry="6" transform="rotate({r} {x} {y})" fill="#fbe8b0" stroke="none"/>' for x,y,r in [(random.randint(360,670),random.randint(450,600),random.randint(-40,40)) for _ in range(70)] if (x-512)**2/170**2+(y-600)**2/150**2<1)
bits=''.join(f'<circle cx="{random.randint(380,650)}" cy="{random.randint(480,590)}" r="11" fill="{c}" stroke-width="6"/>' for c in ["#7fa05a","#7fa05a","#d9825b","#d9825b","#7fa05a","#d9825b","#7fa05a"])
eggs=''.join(f'<path d="M{x} {y} q15 -18 32 -4 q-6 22 -32 4Z" fill="#f2c64f" stroke-width="6"/>' for x,y in [(420,520),(560,470),(600,560),(480,580)])
save("Food","Fried rice","sand",f'''
{plate(512,660,350,160)}
{steam([440,580],340,0.4)}
<path d="M300 650 Q300 440 512 430 Q724 440 724 650 Q512 700 300 650Z" fill="#e8b85f" stroke-width="14"/>
{rice}{bits}{eggs}
{hl("M340 600 Q350 490 440 455",14,0.45)}
<g transform="rotate(-15 770 650)"><path d="M690 650 A80 80 0 0 0 850 650Z" fill="#8fbf5a" stroke-width="12"/><path d="M710 656 A60 60 0 0 0 830 656Z" fill="#d6e8a8" stroke="none"/></g>
<g transform="rotate(10 260 690)"><ellipse cx="260" cy="690" rx="46" ry="30" fill="#5f8f4e" stroke-width="10"/><ellipse cx="260" cy="690" rx="34" ry="20" fill="#d6e8a8" stroke="none"/></g>
<path d="M650 360 L800 250" stroke-width="30"/><path d="M650 360 L800 250" stroke="#dcd6ca" stroke-width="14"/>
<ellipse cx="610" cy="395" rx="60" ry="38" transform="rotate(-36 610 395)" fill="#dcd6ca" stroke-width="12"/>
''')

# ---------- Garlic ----------
save("Food","Garlic","beige",f'''
{sh(512,800,280,34)}
<path d="M512 240 Q500 190 520 170" fill="none" stroke-width="32"/><path d="M512 240 Q500 190 520 170" fill="none" stroke="#e8dcc6" stroke-width="16"/>
<path d="M512 250 C440 330 280 400 280 580 C280 720 400 790 512 790 C624 790 744 720 744 580 C744 400 584 330 512 250Z" fill="#fbf6ea" stroke-width="16"/>
<path d="M512 270 C450 360 390 470 400 780 M512 270 C574 360 634 470 624 780 M512 270 C470 400 470 600 500 788" fill="none" stroke-width="8" opacity="0.4"/>
<path d="M340 500 Q380 560 390 700 M690 520 Q650 580 650 700" fill="none" stroke="#b58aa8" stroke-width="12" opacity="0.6"/>
<path d="M440 790 Q512 810 584 790" fill="none" stroke-width="0"/>
<g stroke-width="5"><path d="M470 790 L460 815 M500 792 L500 820 M530 792 L540 818 M560 790 L575 812" fill="none"/></g>
{hl("M320 600 Q320 470 420 380",18,0.8)}
<g transform="rotate(25 760 740)"><path d="M700 780 C700 700 740 660 760 640 C790 670 820 710 815 780 Q760 800 700 780Z" fill="#fbf6ea" stroke-width="12"/><path d="M730 760 Q735 700 760 660" fill="none" stroke="#b58aa8" stroke-width="8" opacity="0.6"/></g>
''')

# ---------- Green curry: clay bowl with handles ----------
def basil(x,y,rot,s=1): return leaf(x,y,rot,s,"#4f8a45")
eggpl=''.join(f'<circle cx="{x}" cy="{y}" r="26" fill="#d6e8a8" stroke-width="8"/><circle cx="{x}" cy="{y}" r="12" fill="none" stroke="#7fa05a" stroke-width="5"/>' for x,y in [(420,500),(600,515),(510,470)])
chk=''.join(f'<rect x="{x-24}" y="{y-18}" width="48" height="36" rx="12" transform="rotate({r} {x} {y})" fill="#f6ecd8" stroke-width="8"/>' for x,y,r in [(360,520,-10),(470,530,15),(660,490,-20),(560,470,10)])
save("Food","Green curry","sage",f'''
{sh(512,820,320,36)}
{steam([440,512,584],330,0.45)}
<path d="M200 500 Q140 480 140 530 Q140 570 210 570" fill="none" stroke-width="44"/><path d="M200 500 Q140 480 140 530 Q140 570 210 570" fill="none" stroke="#9a6a45" stroke-width="22"/>
<path d="M824 500 Q884 480 884 530 Q884 570 814 570" fill="none" stroke-width="44"/><path d="M824 500 Q884 480 884 530 Q884 570 814 570" fill="none" stroke="#9a6a45" stroke-width="22"/>
<path d="M392 760 L380 810 L644 810 L632 760Z" fill="#7a4f33" stroke-width="12"/>
<path d="M200 500 Q210 780 512 790 Q814 780 824 500Z" fill="#9a6a45" stroke-width="16"/>
{hl("M250 580 Q280 720 380 760",18,0.3)}
{dk("M770 580 Q740 720 640 760",18,0.15)}
<ellipse cx="512" cy="500" rx="312" ry="75" fill="#7a4f33" stroke-width="16"/>
<ellipse cx="512" cy="505" rx="275" ry="55" fill="#a8c46a" stroke-width="8"/>
<path d="M300 505 Q400 480 460 500 Q540 520 620 495 Q690 480 720 505" fill="none" stroke="#f6ecd8" stroke-width="8" opacity="0.5"/>
{eggpl}{chk}
<g stroke-width="7"><path d="M400 470 L440 455" stroke="#c8574b" stroke-width="12"/><path d="M620 540 L660 530" stroke="#c8574b" stroke-width="12"/><path d="M530 540 L560 552" stroke="#c8574b" stroke-width="12"/></g>
{basil(480,500,-40,0.6)}{basil(560,500,-150,0.6)}{basil(520,505,-100,0.55)}
{basil(210,800,-20,1.1)}{basil(250,830,-60,0.9)}{basil(800,810,200,1.1)}
''')

# ---------- Light soy sauce: dark bottle + saucer ----------
pod=('<g transform="translate(440 715) rotate(-15)"><path d="M-80 10 C-60 -40 60 -40 80 -10 C60 30 -60 40 -80 10Z" fill="#7fa05a" stroke-width="9"/>'
     '<circle cx="-40" cy="0" r="17" fill="#c9dca0" stroke-width="6"/><circle cx="0" cy="-5" r="17" fill="#c9dca0" stroke-width="6"/><circle cx="40" cy="-8" r="16" fill="#c9dca0" stroke-width="6"/></g>')
save("Food","Light soy sauce","beige",
     bottle(440,"#6b3a26","#c8574b",pod+'<path d="M348 650 L532 650 M348 800 L532 800" stroke="#5f8f4e" stroke-width="10"/>',0.85,"#d8c2a6")+f'''
{sh(730,830,130,20)}
<ellipse cx="730" cy="740" rx="120" ry="40" fill="#f6ecd8" stroke-width="12"/>
<path d="M610 740 Q615 820 730 825 Q845 820 850 740" fill="#f6ecd8" stroke-width="12"/>
<ellipse cx="730" cy="744" rx="95" ry="28" fill="#8a4f33" stroke-width="7"/>
<ellipse cx="700" cy="738" rx="30" ry="7" fill="#ffffff" opacity="0.35" stroke="none"/>
{hl("M630 770 Q650 805 700 815",10,0.4)}''')

# ---------- Lime ----------
segs=''.join(f'<path d="M0 0 L{math.cos(a)*108:.0f} {math.sin(a)*108:.0f} A108 108 0 0 1 {math.cos(a+math.pi/4)*108:.0f} {math.sin(a+math.pi/4)*108:.0f}Z" fill="#cfe38f" stroke="#eef4d0" stroke-width="10"/>' for a in [i*math.pi/4+0.2 for i in range(8)])
save("Food","Lime","sage",f'''
{sh(512,790,320,38)}
{leaf(560,380,-50,1.6,"#5f8f4e")}{leaf(620,420,-10,1.3,"#4f7f47")}
<circle cx="600" cy="560" r="190" fill="#7fae4a" stroke-width="16"/>
<g fill="#6a9a3c" stroke="none"><circle cx="560" cy="480" r="6"/><circle cx="660" cy="520" r="6"/><circle cx="700" cy="620" r="6"/><circle cx="620" cy="680" r="6"/><circle cx="540" cy="600" r="6"/></g>
{hl("M470 520 Q480 440 550 400",20,0.35)}
{dk("M760 600 Q730 700 640 740",20,0.15)}
<g transform="translate(390 600) rotate(-10)">
<circle cx="0" cy="0" r="150" fill="#7fae4a" stroke-width="16"/>
<circle cx="0" cy="0" r="128" fill="#f2f6dc" stroke="none"/>
<g>{segs}</g>
<circle cx="0" cy="0" r="16" fill="#eef4d0" stroke="none"/>
<circle cx="0" cy="0" r="150" fill="none" stroke-width="16"/>
<path d="M-100 -60 Q-70 -105 -20 -115" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.6"/>
</g>
''')

# ---------- Long bean ----------
beans=""
cols=["#5f8f4e","#7fa05a","#6f9a4a","#5f8f4e","#7fa05a"]
for i,k in enumerate([-2,-1,0,1,2]):
    d=(f"M{250+k*55} {820-abs(k)*10} C{300+k*40} {700} {420+k*14} {620} {470} {540+k*8} "
       f"C{520-k*10} {460+k*14} {600+k*20} {420+k*30} {640+k*20} {300+k*50} "
       f"C{660+k*20} {240+k*50} {720+k*25} {200+k*40} {780+k*15} {180+k*55}")
    beans+=sl(d,24,cols[i],14)+f'<path d="{d}" fill="none" stroke="#ffffff" stroke-width="6" opacity="0.3" stroke-dasharray="26 22"/>'
save("Food","Long bean","sand",f'''
{sh(440,860,240,28)}
<g transform="translate(512 540) scale(0.88) translate(-500 -480)">
{beans}
<g transform="rotate(-50 470 545)"><rect x="390" y="522" width="160" height="46" rx="14" fill="#c8574b" stroke-width="12"/></g>
</g>
''')

# ---------- Meat: assortment on board ----------
save("Food","Meat","rose",f'''
{board(470,600,600,380)}
<path d="M230 520 C220 440 320 400 400 420 C470 390 560 420 560 500 C575 590 480 640 400 625 C300 650 240 600 230 520Z" fill="#f3dcc8" stroke-width="14"/>
<path d="M262 520 C255 455 335 425 400 445 C465 418 535 440 535 505 C548 575 470 610 402 600 C320 620 268 580 262 520Z" fill="#b8453d" stroke-width="7"/>
<g fill="none" stroke="#f3dcc8" stroke-width="8"><path d="M300 500 Q340 480 380 505 Q410 525 450 505"/><path d="M340 560 Q390 550 430 575 Q470 585 500 560"/></g>
<g transform="rotate(-25 640 480)">
<path d="M560 480 C560 420 620 410 680 440 Q720 460 730 480 Q720 500 680 520 C620 550 560 540 560 480Z" fill="#f2c7ae" stroke-width="13"/>
<path d="M728 470 L810 470 L810 490 L728 490Z" fill="#f6ecd8" stroke-width="10"/><circle cx="820" cy="462" r="18" fill="#f6ecd8" stroke-width="9"/><circle cx="820" cy="498" r="18" fill="#f6ecd8" stroke-width="9"/><path d="M808 470 L808 490" stroke="#f6ecd8" stroke-width="14"/>
{hl("M585 470 Q600 440 650 440",12,0.45)}
</g>
<g transform="rotate(8 520 700)">
<path d="M330 690 Q390 660 450 690 Q510 720 570 690 Q630 660 690 690 L690 730 Q630 700 570 730 Q510 760 450 730 Q390 700 330 730Z" fill="#d86a5e" stroke-width="12"/>
<path d="M330 705 Q390 675 450 705 Q510 735 570 705 Q630 675 690 705" fill="none" stroke="#f3dcc8" stroke-width="10"/>
</g>
<g transform="translate(-60 20)"><rect x="590" y="560" width="150" height="70" rx="35" fill="#a8463d" stroke-width="12"/><rect x="720" y="580" width="110" height="64" rx="32" transform="rotate(20 775 612)" fill="#a8463d" stroke-width="12"/>
<g fill="#f3dcc8" stroke="none"><circle cx="630" cy="590" r="6"/><circle cx="680" cy="605" r="6"/><circle cx="710" cy="585" r="5"/><circle cx="770" cy="610" r="6"/></g>
{hl("M615 580 L715 580",8,0.35)}</g>
''')

# ---------- Minced pork ----------
curls=''.join(f'<path d="M{x} {y} q{random.choice([-1,1])*10} -14 {random.choice([-1,1])*20} -2 q8 10 18 0" fill="none" stroke="{random.choice(["#b8574b","#f6dcd0","#c9685c"])}" stroke-width="7"/>' for x,y in [(random.randint(330,660),random.randint(470,640)) for _ in range(120)] if ((x-500)/190)**2+((y-640)/180)**2<0.85)
save("Food","Minced pork","rose",f'''
{plate(500,650,340,160)}
<path d="M290 640 Q280 520 380 480 Q450 420 540 440 Q650 450 690 540 Q720 620 710 650 Q500 710 290 640Z" fill="#e39a88" stroke-width="14"/>
{curls}
{hl("M330 600 Q340 520 410 490",14,0.35)}
{leaf(700,700,-20,1.0,"#7fa05a")}{leaf(280,700,190,0.9,"#5f8f4e")}
{badge(760,300,"pig",105)}
''')

# ---------- Morning glory (pak boong) ----------
def mg(x0,y0,x1,y1,col):
    ang=math.degrees(math.atan2(y1-y0,x1-x0))
    return (sl(f"M{x0} {y0} Q{(x0+x1)/2+15} {(y0+y1)/2} {x1} {y1}",24,"#a8c77a",12)+
            f'<g transform="translate({x1} {y1}) rotate({ang+90})"><path d="M0 0 C-15 -8 -38 -12 -52 -4 C-48 -30 -38 -50 -34 -70 C-32 -120 -15 -180 0 -220 C15 -180 32 -120 34 -70 C38 -50 48 -30 52 -4 C38 -12 15 -8 0 0Z" fill="{col}" stroke-width="12"/>'
            f'<path d="M0 -5 L0 -195" fill="none" stroke="#c9dca0" stroke-width="7"/><path d="M-30 -70 Q-25 -130 -5 -170" fill="none" stroke="#ffffff" stroke-width="8" opacity="0.25"/></g>')
save("Food","Morning glory","sage",f'''
{sh(512,850,220,30)}
{mg(500,840,280,450,"#4f7f47")}{mg(524,840,750,460,"#4f7f47")}
{mg(505,840,370,360,"#5f8f4e")}{mg(519,840,650,350,"#5f8f4e")}
{mg(512,840,505,320,"#6a9a55")}
{mg(508,840,420,450,"#6a9a55")}{mg(516,840,610,460,"#6a9a55")}
<rect x="455" y="700" width="114" height="44" rx="14" fill="#c8574b" stroke-width="12"/>
''')

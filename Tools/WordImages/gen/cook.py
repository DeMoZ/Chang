from common import *
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def hl(d,w=16,op=0.3): return f'<path d="{d}" fill="none" stroke="#ffffff" stroke-width="{w}" opacity="{op}"/>'
def gasflame(x,y,s=1): return f'<path transform="translate({x} {y}) scale({s})" d="M0 0 C-30 0 -40 -30 -25 -55 C-20 -40 -10 -40 -8 -50 C-12 -75 0 -90 5 -105 C15 -80 35 -65 30 -35 C35 -40 38 -50 36 -58 C55 -30 45 0 0 0Z" fill="#5d82a8" stroke-width="{9/s:.0f}"/><path transform="translate({x} {y}) scale({s})" d="M2 -8 C-15 -8 -18 -25 -10 -40 C0 -30 10 -45 8 -60 C20 -40 25 -25 20 -15 C15 -8 10 -8 2 -8Z" fill="#a9c4dc" stroke="none"/>'
def bubbles(pts): return ''.join(f'<circle cx="{x}" cy="{y}" r="{r}" fill="#ffffff" fill-opacity="0.6" stroke-width="7"/>' for x,y,r in pts)
def leaf(x,y,rot,s=1,col="#5f8f4e"): return f'<path transform="translate({x} {y}) rotate({rot}) scale({s})" d="M0 0 Q35 -40 90 -25 Q50 15 0 0Z" fill="{col}" stroke-width="{9/s:.0f}"/>'
def shrimp(x,y,rot,s=1):
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({s})">
<path d="M-60 -10 C-60 -70 40 -80 70 -20 C85 10 70 45 40 55" fill="none" stroke-width="58"/>
<path d="M-60 -10 C-60 -70 40 -80 70 -20 C85 10 70 45 40 55" fill="none" stroke="#e8875f" stroke-width="36"/>
<path d="M-20 -60 L-10 -35 M15 -62 L15 -35 M45 -48 L35 -25 M68 -18 L45 -8" stroke-width="6" opacity="0.5"/>
<path d="M40 55 L10 85 L60 90Z" fill="#e8875f" stroke-width="9"/>
<circle cx="-62" cy="-28" r="6" fill="#2e211b" stroke="none"/>
<path d="M-75 -15 Q-120 -40 -130 -90 M-70 -25 Q-100 -60 -95 -110" fill="none" stroke-width="5"/>
</g>'''

# Bake: oven with glowing window and loaf/cake
save("HowToCook","Bake","beige",f'''
{sh(512,835,320,35)}
<rect x="250" y="800" width="60" height="30" rx="8" fill="#5c3d2e" stroke-width="10"/><rect x="714" y="800" width="60" height="30" rx="8" fill="#5c3d2e" stroke-width="10"/>
<rect x="220" y="240" width="584" height="570" rx="40" fill="#c8574b" stroke-width="16"/>
{hl("M250 300 L250 760",18,0.3)}
<rect x="220" y="240" width="584" height="110" rx="40" fill="#a8463d" stroke-width="16"/>
<circle cx="310" cy="295" r="26" fill="#f6ecd8" stroke-width="10"/><path d="M310 295 L310 275" stroke-width="8"/>
<circle cx="400" cy="295" r="26" fill="#f6ecd8" stroke-width="10"/><path d="M400 295 L418 283" stroke-width="8"/>
<rect x="540" y="275" width="200" height="40" rx="12" fill="#3b2a22" stroke-width="10"/>
<circle cx="600" cy="295" r="6" fill="#e0b04f" stroke="none"/><circle cx="640" cy="295" r="6" fill="#e0b04f" stroke="none"/><circle cx="680" cy="295" r="6" fill="#e0b04f" stroke="none"/>
<rect x="300" y="380" width="424" height="40" rx="20" fill="#5c3d2e" stroke-width="12"/>
<rect x="280" y="450" width="464" height="300" rx="30" fill="url(#glow)" stroke-width="14"/>
<path d="M300 690 L724 690" stroke-width="10" opacity="0.5"/>
<path d="M400 690 Q390 580 512 570 Q634 580 624 690Z" fill="#c98a4b" stroke-width="12"/>
<path d="M440 610 L470 590 M500 600 L530 580 M560 610 L590 590" stroke-width="8" opacity="0.6"/>
{hl("M420 650 Q420 600 480 590",12,0.35)}
{hl("M300 480 L380 480",10,0.5)}
''',defs='<linearGradient id="glow" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#f5d08a"/><stop offset="1" stop-color="#e0904f"/></linearGradient>')

# Boil: pot on flame with bubbles & steam
save("HowToCook","Boil","sky",f'''
{sh(512,875,260,24)}
{gasflame(420,870,0.8)}{gasflame(512,870,0.9)}{gasflame(604,870,0.8)}
{steam([420,512,604],300)}
<path d="M230 470 Q190 470 190 510 Q190 540 240 540" fill="none" stroke-width="40"/><path d="M230 470 Q190 470 190 510 Q190 540 240 540" fill="none" stroke="#5c3d2e" stroke-width="18"/>
<path d="M794 470 Q834 470 834 510 Q834 540 784 540" fill="none" stroke-width="40"/><path d="M794 470 Q834 470 834 510 Q834 540 784 540" fill="none" stroke="#5c3d2e" stroke-width="18"/>
<path d="M250 440 L270 740 Q280 780 512 780 Q744 780 754 740 L774 440Z" fill="#8ea3b3" stroke-width="16"/>
{hl("M290 480 L305 730",20,0.35)}
<path d="M270 600 Q512 640 754 600" fill="none" stroke-width="8" opacity="0.2"/>
<ellipse cx="512" cy="440" rx="262" ry="50" fill="#6f889a" stroke-width="16"/>
<ellipse cx="512" cy="445" rx="225" ry="34" fill="#9fc6d6" stroke-width="8"/>
{bubbles([(420,440,20),(480,430,14),(560,445,24),(630,435,16),(360,450,12),(520,395,16),(600,380,11),(450,380,12),(690,450,12)])}
''')

# Deep fry: wok of golden oil with spring rolls and bubbles
rolls=''.join(f'<g transform="rotate({r} {x} {y})"><rect x="{x-90}" y="{y-32}" width="180" height="64" rx="32" fill="#d9a04f" stroke-width="12"/><path d="M{x-40} {y-30} L{x-60} {y+30} M{x+10} {y-30} L{x-10} {y+30} M{x+60} {y-30} L{x+40} {y+30}" stroke-width="6" opacity="0.4"/><path d="M{x-70} {y-15} L{x+60} {y-15}" stroke="#ffffff" stroke-width="10" opacity="0.35"/></g>' for x,y,r in [(420,470,-15),(600,480,12),(510,420,3)])
save("HowToCook","Deep fry","sand",f'''
{sh(512,810,300,30)}
{gasflame(420,810,0.9)}{gasflame(512,810,1)}{gasflame(604,810,0.9)}
<path d="M160 490 L90 450" stroke-width="44"/><path d="M160 490 L90 450" stroke="#9a6a45" stroke-width="24"/>
<path d="M864 490 L934 450" stroke-width="44"/><path d="M864 490 L934 450" stroke="#9a6a45" stroke-width="24"/>
<path d="M170 480 Q200 740 512 750 Q824 740 854 480Z" fill="#4a3a33" stroke-width="16"/>
{hl("M230 540 Q270 680 380 720",18,0.2)}
<ellipse cx="512" cy="480" rx="345" ry="70" fill="#3b2a22" stroke-width="16"/>
<ellipse cx="512" cy="490" rx="305" ry="52" fill="#e8b94f" stroke-width="8"/>
{rolls}
{bubbles([(300,500,14),(340,480,10),(720,495,16),(690,515,10),(480,520,12),(560,525,9),(650,470,8),(370,515,9)])}
<g fill="none" stroke-width="10" opacity="0.45"><path d="M380 360 q-20 -30 0 -60 q20 -30 0 -60"/><path d="M650 360 q-20 -30 0 -60 q20 -30 0 -60"/></g>
''')

# Grill: skewers on a grill with flames
def skewer(y,x0=260,x1=770,cols=("#b5603c","#7fa05a","#b5603c","#e0b04f","#b5603c")):
    s=f'<path d="M{x0-40} {y} L{x1+40} {y}" stroke-width="18"/><path d="M{x0-40} {y} L{x1+40} {y}" stroke="#d9b98a" stroke-width="8"/>'
    step=(x1-x0)/(len(cols)-1)
    for i,c in enumerate(cols):
        x=x0+i*step
        s+=f'<rect x="{x-45}" y="{y-40}" width="90" height="80" rx="22" fill="{c}" stroke-width="12"/><path d="M{x-25} {y-25} L{x+5} {y+25} M{x+5} {y-25} L{x+30} {y+15}" stroke-width="7" opacity="0.4"/>'
    return s
save("HowToCook","Grill","rose",f'''
{sh(512,850,300,30)}
<path d="M360 700 L310 840 M664 700 L714 840" stroke-width="16"/>
<path d="M240 380 L784 380 L834 570 L190 570Z" fill="url(#coal)" stroke-width="16"/>
{flame(320,560,0.35)}{flame(430,560,0.45,-5)}{flame(620,565,0.4,6)}{flame(730,560,0.35)}
<g stroke="#8e8e8e" stroke-width="10">{''.join(f'<path d="M{250+i*60} 385 L{205+i*69} 565"/>' for i in range(10))}</g>
<path d="M190 570 L834 570 L790 720 Q512 750 234 720Z" fill="#3b2a22" stroke-width="16"/>
{hl("M240 600 L270 700",14,0.2)}
{skewer(440,300,720)}
{skewer(520,290,740,("#b5603c","#c8574b","#b5603c","#7fa05a","#b5603c"))}
<g fill="none" stroke-width="10" opacity="0.4"><path d="M420 330 q-20 -30 0 -60 q20 -30 0 -60"/><path d="M620 330 q-20 -30 0 -60 q20 -30 0 -60"/></g>
''',defs='<linearGradient id="coal" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#5c3d2e"/><stop offset="1" stop-color="#d9825b"/></linearGradient>')

# Soft boil
save("HowToCook","Soft boil","sand",f'''
{sh(512,840,260,30)}
<path d="M318 400 C300 520 315 610 512 640 C709 610 724 520 706 400 Z" fill="#fbf4e6" stroke-width="16"/>
<ellipse cx="512" cy="400" rx="194" ry="46" fill="#fbf4e6" stroke-width="14"/>
<ellipse cx="512" cy="402" rx="130" ry="30" fill="#f0a93c" stroke-width="10"/>
<path d="M590 420 Q640 430 650 470 Q660 520 640 540 Q620 555 612 520 Q606 470 570 428Z" fill="#f0a93c" stroke-width="10"/>
<ellipse cx="480" cy="394" rx="44" ry="10" fill="#ffffff" opacity="0.55" stroke="none"/>
{hl("M345 450 Q340 500 355 540",16,0.8)}
<path d="M392 740 L372 830 L652 830 L632 740Z" fill="#c16a45" stroke-width="14"/>
<path d="M320 555 Q330 770 512 770 Q694 770 704 555 Z" fill="#d9825b" stroke-width="16"/>
<path d="M320 555 Q512 590 704 555" fill="none" stroke-width="14"/>
{hl("M355 600 Q370 700 430 740",16,0.3)}
<path d="M790 830 L770 560" stroke-width="30"/><path d="M790 830 L770 560" stroke="#b9b2a6" stroke-width="14"/>
<ellipse cx="767" cy="520" rx="36" ry="58" fill="#b9b2a6" stroke-width="12"/>
<path d="M200 300 Q210 250 260 240" fill="none" stroke-width="0"/>
''')

# Stir fry: tilted wok, veggies tossed in arc
veg=(f'<circle cx="380" cy="300" r="34" fill="#d9825b" stroke-width="10"/><circle cx="380" cy="300" r="14" fill="none" stroke-width="5" opacity="0.5"/>'
     f'<path d="M480 230 Q520 200 560 230 Q540 270 500 265Z" fill="#7fa05a" stroke-width="10"/>'
     f'<circle cx="640" cy="300" r="30" fill="#c8574b" stroke-width="10"/>'
     f'<g transform="rotate(20 720 380)"><path d="M700 390 L740 390 L735 430 L705 430Z" fill="#8fae5e" stroke-width="8"/><circle cx="700" cy="375" r="22" fill="#5f8f4e" stroke-width="8"/><circle cx="730" cy="365" r="24" fill="#5f8f4e" stroke-width="8"/><circle cx="745" cy="390" r="18" fill="#5f8f4e" stroke-width="8"/></g>'
     f'<rect x="300" y="380" width="70" height="30" rx="10" transform="rotate(-30 335 395)" fill="#e0b04f" stroke-width="9"/>')
save("HowToCook","Stir fry","sage",f'''
{sh(512,820,300,30)}
{gasflame(430,810,1)}{gasflame(530,810,1.1)}{gasflame(630,810,1)}
<g fill="none" stroke-width="10" opacity="0.4"><path d="M300 440 Q320 280 470 220"/><path d="M760 470 Q760 330 680 270"/></g>
{veg}
<g transform="rotate(-8 512 560)">
<path d="M170 560 L60 520" stroke-width="44"/><path d="M170 560 L60 520" stroke="#3b2a22" stroke-width="24"/>
<path d="M180 540 Q210 760 512 770 Q814 760 844 540Z" fill="#4a3a33" stroke-width="16"/>
{hl("M240 590 Q280 710 390 740",18,0.2)}
<ellipse cx="512" cy="540" rx="335" ry="60" fill="#3b2a22" stroke-width="16"/>
<path d="M300 545 Q360 510 420 545 Q470 515 520 548 Q580 510 640 545 Q690 520 730 548" fill="none" stroke="#7fa05a" stroke-width="22"/>
<circle cx="400" cy="545" r="20" fill="#d9825b" stroke-width="8"/><circle cx="600" cy="540" r="18" fill="#c8574b" stroke-width="8"/>
</g>
''')

# Toast: toaster with two slices popping
def slice_(x,y,rot): return f'<g transform="rotate({rot} {x} {y})"><path d="M{x-80} {y+100} L{x-80} {y-40} Q{x-110} {y-60} {x-100} {y-90} Q{x-80} {y-130} {x} {y-120} Q{x+80} {y-130} {x+100} {y-90} Q{x+110} {y-60} {x+80} {y-40} L{x+80} {y+100}Z" fill="#c98a4b" stroke-width="14"/><path d="M{x-58} {y+100} L{x-58} {y-30} Q{x-85} {y-55} {x-75} {y-80} Q{x-60} {y-105} {x} {y-98} Q{x+60} {y-105} {x+75} {y-80} Q{x+85} {y-55} {x+58} {y-30} L{x+58} {y+100}Z" fill="#f2d9a6" stroke="none"/></g>'
save("HowToCook","Toast","beige",f'''
{sh(512,820,300,30)}
{slice_(420,420,-8)}{slice_(610,410,6)}
<path d="M230 520 Q230 470 290 470 L734 470 Q794 470 794 520 L794 760 Q794 800 750 800 L274 800 Q230 800 230 760Z" fill="#5d82a8" stroke-width="16"/>
{hl("M260 530 L260 760",18,0.3)}
<rect x="330" y="455" width="170" height="30" rx="15" fill="#2e211b" stroke-width="8"/><rect x="530" y="455" width="170" height="30" rx="15" fill="#2e211b" stroke-width="8"/>
<rect x="794" y="560" width="40" height="30" rx="10" fill="#e0b04f" stroke-width="10"/>
<path d="M300 640 L724 640" stroke-width="8" opacity="0.3"/>
<circle cx="512" cy="720" r="26" fill="#f6ecd8" stroke-width="10"/>
{sparkle(260,300,0.8)}{sparkle(790,320,0.9)}
''')

# Curry: bowl of Thai green curry
save("HowToCook","Curry","sage",f'''
{sh(512,820,300,35)}
{steam([430,512,594],320)}
<path d="M392 760 L380 810 L644 810 L632 760Z" fill="#9a6a45" stroke-width="12"/>
<path d="M200 500 Q210 780 512 790 Q814 780 824 500Z" fill="#f6ecd8" stroke-width="16"/>
<path d="M210 560 Q512 610 814 560" fill="none" stroke="#5d82a8" stroke-width="14"/>
{hl("M250 580 Q280 720 380 760",18,0.4)}
<ellipse cx="512" cy="500" rx="312" ry="75" fill="#f6ecd8" stroke-width="16"/>
<ellipse cx="512" cy="505" rx="280" ry="58" fill="#b2b85a" stroke-width="8"/>
<path d="M330 500 Q400 470 470 505 Q540 535 610 500 Q660 480 700 505" fill="none" stroke="#f6ecd8" stroke-width="12" opacity="0.7"/>
<rect x="380" y="470" width="60" height="45" rx="12" fill="#f3e0c0" stroke-width="9"/>
<rect x="570" y="505" width="55" height="42" rx="12" fill="#f3e0c0" stroke-width="9"/>
<circle cx="500" cy="530" r="26" fill="#7fa05a" stroke-width="9"/><circle cx="500" cy="530" r="10" fill="#e7ecdc" stroke-width="5"/>
<circle cx="660" cy="480" r="22" fill="#7fa05a" stroke-width="9"/><circle cx="660" cy="480" r="8" fill="#e7ecdc" stroke-width="5"/>
<path d="M320 520 L360 505" stroke="#c8574b" stroke-width="16"/><path d="M700 525 L735 512" stroke="#c8574b" stroke-width="16"/>
{leaf(460,470,-20,0.6)}{leaf(540,470,10,0.55)}{leaf(600,540,-10,0.5)}
''')

# Spicy sour salad: som tam plate
strands=''.join(f'<path d="M{x} {y} q{dx} {dy} {dx*2} 0" fill="none" stroke-width="18"/><path d="M{x} {y} q{dx} {dy} {dx*2} 0" fill="none" stroke="#cfe0a0" stroke-width="8"/>' for x,y,dx,dy in [(330,560,60,-40),(380,600,70,-30),(450,540,60,-50),(520,590,60,-40),(560,540,70,-30),(420,640,60,-25),(600,620,50,-40),(350,620,40,-30),(640,570,40,-40),(480,500,60,-30)])
save("HowToCook","Spicy sour salad","sand",f'''
{sh(512,760,330,50)}
<ellipse cx="512" cy="600" rx="340" ry="150" fill="#f6ecd8" stroke-width="16"/>
<ellipse cx="512" cy="590" rx="260" ry="105" fill="#e8dcc6" stroke-width="8"/>
{strands}
<path d="M300 560 L370 480 L400 580Z" fill="#c8574b" stroke-width="10"/><path d="M620 520 L700 480 L690 590Z" fill="#c8574b" stroke-width="10"/>
<path d="M330 520 L380 490" fill="none" stroke="#f6ecd8" stroke-width="6" opacity="0"/>
<path d="M440 660 L620 690" stroke-width="26"/><path d="M440 660 L620 690" stroke="#7fa05a" stroke-width="12"/>
<path d="M470 620 L650 640" stroke-width="26"/><path d="M470 620 L650 640" stroke="#7fa05a" stroke-width="12"/>
<g transform="translate(560 470) rotate(20)"><path d="M0 0 C30 -5 60 10 70 45 C50 30 25 25 5 15Z" fill="#c8574b" stroke-width="9"/></g>
<g transform="translate(400 470) rotate(-40)"><path d="M0 0 C30 -5 60 10 70 45 C50 30 25 25 5 15Z" fill="#c8574b" stroke-width="9"/></g>
<g fill="#d9a86a" stroke-width="6"><ellipse cx="500" cy="560" rx="12" ry="9"/><ellipse cx="540" cy="620" rx="12" ry="9"/><ellipse cx="420" cy="580" rx="12" ry="9"/><ellipse cx="610" cy="580" rx="12" ry="9"/></g>
<path d="M720 620 A80 80 0 0 1 800 540 L720 540Z" fill="#8fbf5a" stroke-width="12" transform="rotate(20 740 580)"/>
<path d="M735 610 A60 60 0 0 1 795 550 L735 550Z" fill="#d6e8a8" stroke="none" transform="rotate(20 740 580)"/>
''')

# Spicy sour soup: tom yum bowl with shrimp, lemongrass, chili
save("HowToCook","Spicy sour soup","rose",f'''
{sh(512,830,300,35)}
{steam([400,512,624],300)}
<path d="M392 770 L380 820 L644 820 L632 770Z" fill="#8a5a3f" stroke-width="12"/>
<path d="M190 480 Q200 790 512 800 Q824 790 834 480Z" fill="#b5603c" stroke-width="16"/>
{hl("M240 560 Q270 720 380 770",18,0.3)}
<ellipse cx="512" cy="480" rx="322" ry="78" fill="#b5603c" stroke-width="16"/>
<ellipse cx="512" cy="485" rx="288" ry="60" fill="#e0763f" stroke-width="8"/>
<ellipse cx="450" cy="470" rx="80" ry="12" fill="#e0b04f" opacity="0.6" stroke="none"/>
<path d="M300 470 L430 360" stroke-width="30"/><path d="M300 470 L430 360" stroke="#d7df9a" stroke-width="14"/>
<path d="M330 500 L480 410" stroke-width="30"/><path d="M330 500 L480 410" stroke="#d7df9a" stroke-width="14"/>
{shrimp(560,470,-10,0.75)}
{shrimp(700,480,20,0.6)}
<g fill="#f3e6d6" stroke-width="9"><path d="M420 500 Q420 470 450 470 Q480 470 480 500Z"/><path d="M620 520 Q620 495 645 495 Q670 495 670 520Z"/></g>
{leaf(470,520,-10,0.6,"#4f7a40")}{leaf(360,510,20,0.5,"#4f7a40")}
<path d="M760 460 L800 440" stroke="#c8574b" stroke-width="18"/><path d="M270 500 L300 490" stroke="#c8574b" stroke-width="16"/>
''')

# Month: calendar page
cells=''
d=1
for r in range(5):
    for c in range(7):
        idx=r*7+c-2
        x=265+c*82; y=455+r*76
        if 1<=idx+1<=31:
            n=idx+1
            if n==15:
                cells+=f'<circle cx="{x}" cy="{y-12}" r="32" fill="#c8574b" stroke-width="8"/>'
                col="#f6ecd8"
            else: col="#5c3d2e"
            cells+=f'<text x="{x}" y="{y}" text-anchor="middle" font-family="Arial, Helvetica, sans-serif" font-weight="bold" font-size="34" fill="{col}" stroke="none">{n}</text>'
save("Months","Month","sky",f'''
{sh(512,850,320,30)}
<rect x="210" y="230" width="604" height="600" rx="40" fill="#e2cfb3" stroke-width="16" transform="translate(14 10)"/>
<rect x="210" y="230" width="604" height="600" rx="40" fill="#fbf6ea" stroke-width="16"/>
<path d="M210 370 L210 270 Q210 230 250 230 L774 230 Q814 230 814 270 L814 370Z" fill="#c8574b" stroke-width="16"/>
{hl("M250 270 L770 270",12,0.3)}
<g stroke-width="6" opacity="0.18">{''.join(f'<path d="M230 {y} L794 {y}"/>' for y in range(468,800,76))}</g>
{''.join(f'<rect x="{x-9}" y="{386}" width="18" height="18" rx="4" fill="#5d82a8" stroke="none"/>' for x in range(265,265+7*82,82))}
{cells}
<rect x="300" y="190" width="30" height="100" rx="15" fill="#8e8e8e" stroke-width="12"/>
<rect x="497" y="190" width="30" height="100" rx="15" fill="#8e8e8e" stroke-width="12"/>
<rect x="694" y="190" width="30" height="100" rx="15" fill="#8e8e8e" stroke-width="12"/>
''')
import salad

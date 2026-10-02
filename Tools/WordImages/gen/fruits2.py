import math
from common import *
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def leaf(x,y,rot,s=1,col="#5f8f4e"):
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({s})"><path d="M0 0 Q50 -60 150 -40 Q90 30 0 0Z" fill="{col}" stroke-width="{11/s:.0f}"/>
<path d="M10 -3 Q70 -25 135 -38" fill="none" stroke-width="{7/s:.0f}" opacity="0.5"/></g>'''
def hi(d,w=16,op=0.35): return f'<path d="{d}" fill="none" stroke="#ffffff" stroke-width="{w}" opacity="{op}"/>'

# Banana: a single big banana plus a smaller one behind
def banana(x,y,col,rot=0,s=1):
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({s})">
<path d="M-10 -20 L-30 -60 L0 -70 L15 -25Z" fill="#7a5a3a" stroke-width="{10/s:.0f}"/>
<path d="M-15 -20 C-15 170 160 270 345 200 C370 190 362 158 335 160 C200 175 100 100 40 -30 Z" fill="{col}" stroke-width="{14/s:.0f}"/>
<path d="M20 10 C50 120 150 185 270 180" fill="none" stroke="#ffffff" stroke-width="{12/s:.0f}" opacity="0.35"/>
<path d="M5 30 C40 150 150 215 280 205" fill="none" stroke-width="{7/s:.0f}" opacity="0.35"/>
<path d="M335 160 L372 150 L378 178 L350 196Z" fill="#5c3d2e" stroke-width="{9/s:.0f}"/>
</g>'''
save("Fruits","Banana","sand",f'''
{sh(512,800,300,34)}
{banana(330,260,"#d9a93f",-4,1.45)}
{banana(270,240,"#e8bf4f",-14,1.55)}
''')

# Coconut: whole brown coconut + cracked half with white flesh
fib=''.join(f'<path d="M{330+i*28} {330+abs(i-5)*14} q{-10+i*2} 150 {-20+i*4} 300" fill="none" stroke-width="6" opacity="0.3"/>' for i in range(11))
save("Fruits","Coconut","sage",f'''
{sh(520,800,310,36)}
<ellipse cx="420" cy="500" rx="210" ry="220" fill="#8a5a3a" stroke-width="16"/>
{fib}
<path d="M300 400 Q330 330 400 310" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.25"/>
<circle cx="380" cy="360" r="14" fill="#4a3024" stroke-width="6"/><circle cx="430" cy="350" r="14" fill="#4a3024" stroke-width="6"/><circle cx="405" cy="395" r="14" fill="#4a3024" stroke-width="6"/>
<path d="M430 790 Q420 600 560 540 Q700 500 800 560 Q860 620 830 720 Q790 810 640 810 Q500 815 430 790Z" fill="#7a4e32" stroke-width="16"/>
<ellipse cx="640" cy="610" rx="190" ry="95" transform="rotate(-8 640 610)" fill="#7a4e32" stroke-width="14"/>
<ellipse cx="640" cy="612" rx="162" ry="76" transform="rotate(-8 640 612)" fill="#fbf6ea" stroke-width="10"/>
<ellipse cx="640" cy="618" rx="118" ry="50" transform="rotate(-8 640 618)" fill="#e9e2cf" stroke-width="8"/>
<path d="M560 606 Q600 585 650 584" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.8"/>
<path d="M470 740 Q520 790 640 790" fill="none" stroke="#2e211b" stroke-width="8" opacity="0.3"/>
''')

# Durian: spiky fruit + opened section with yellow pods
def spikes(cx,cy,rx,ry,n,L,rot=0):
    out=[]
    for i in range(n):
        a=2*math.pi*i/n
        b1,b2=a-math.pi/n*0.8,a+math.pi/n*0.8
        p=lambda t,r=1: (cx+rx*r*math.cos(t),cy+ry*r*math.sin(t))
        x1,y1=p(b1,0.97);x2,y2=p(b2,0.97);xt,yt=cx+(rx+L)*math.cos(a),cy+(ry+L)*math.sin(a)
        out.append(f'<path d="M{x1:.0f} {y1:.0f} L{xt:.0f} {yt:.0f} L{x2:.0f} {y2:.0f}Z" fill="#8a9a3c" stroke-width="9"/>')
    return f'<g transform="rotate({rot} {cx} {cy})">'+''.join(out)+'</g>'
inner=''.join(f'<path d="M{x-16} {y+12} L{x} {y-18} L{x+16} {y+12}Z" fill="#6f8030" stroke-width="7"/>' for x,y in [(330,420),(400,380),(470,400),(300,500),(370,470),(440,460),(510,470),(340,570),(410,550),(480,540),(380,640),(450,625),(550,560),(540,420)])
save("Fruits","Durian","sand",f'''
{sh(520,820,320,36)}
{spikes(430,520,230,270,26,48,-15)}
<ellipse cx="430" cy="520" rx="230" ry="270" transform="rotate(-15 430 520)" fill="#9aaa44" stroke-width="16"/>
{inner}
<path d="M300 380 Q340 300 420 280" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.3"/>
<path d="M455 255 L470 180 Q480 160 500 170" fill="none" stroke="#6b4a33" stroke-width="22"/>
{spikes(660,640,170,120,18,34,0)}
<ellipse cx="660" cy="640" rx="170" ry="120" fill="#9aaa44" stroke-width="16"/>
<ellipse cx="660" cy="630" rx="140" ry="88" fill="#f6ecd8" stroke-width="10"/>
<ellipse cx="605" cy="632" rx="70" ry="52" fill="#ecc24f" stroke-width="10"/>
<ellipse cx="715" cy="628" rx="70" ry="52" fill="#e6b845" stroke-width="10"/>
<path d="M565 615 Q585 595 615 595" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.5"/>
<path d="M680 610 Q700 592 730 592" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.5"/>
''')

# Guava: whole light green guava + cut half, pale pink flesh
seeds=''.join(f'<ellipse cx="{x}" cy="{y}" rx="9" ry="6" fill="#f3dfb8" stroke-width="4"/>' for x,y in [(640,560),(680,600),(700,655),(680,700),(630,720),(590,690),(575,640),(600,595),(640,640),(655,600),(620,670),(660,680)])
save("Fruits","Guava","rose",f'''
{sh(520,815,310,36)}
<path d="M420 250 Q560 250 590 400 Q620 560 520 680 Q430 770 330 700 Q220 620 240 440 Q260 260 420 250Z" fill="#a8c46a" stroke-width="16"/>
<path d="M300 400 Q320 320 390 295" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.35"/>
<g fill="#7fa05a" stroke="none" opacity="0.6"><circle cx="380" cy="450" r="7"/><circle cx="450" cy="520" r="6"/><circle cx="340" cy="580" r="6"/><circle cx="480" cy="390" r="6"/></g>
<path d="M420 255 L415 210" fill="none" stroke-width="12"/>
{leaf(418,220,-35,1)}
<ellipse cx="640" cy="640" rx="190" ry="170" fill="#a8c46a" stroke-width="16"/>
<ellipse cx="640" cy="640" rx="160" ry="142" fill="#f4c7c0" stroke-width="10"/>
<ellipse cx="640" cy="645" rx="95" ry="92" fill="#e98f95" stroke-width="8"/>
{seeds}
<path d="M520 580 Q540 530 590 510" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.5"/>
''')

# Mango: one ripe mango with leaves
save("Fruits","Mango","sky",f'''
{sh(512,820,280,34)}
<g transform="rotate(-40 512 520)">
<path d="M470 230 Q610 220 650 380 Q690 560 630 720 Q570 840 450 820 Q330 800 340 680 Q350 590 390 520 Q420 460 400 380 Q390 250 470 230Z" fill="url(#mg)" stroke-width="16"/>
<path d="M430 300 Q455 260 500 258" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.4"/>
<path d="M600 420 Q640 560 600 700" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.3"/>
<path d="M380 700 Q400 790 470 800" fill="none" stroke="#2e211b" stroke-width="18" opacity="0.12"/>
<path d="M470 232 L470 175" fill="none" stroke="#6b4a33" stroke-width="16"/>
{leaf(470,185,-10,1.3)}
</g>
''',defs='<linearGradient id="mg" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#9ab25a"/><stop offset="0.35" stop-color="#e8bf4a"/><stop offset="0.8" stop-color="#e8983f"/><stop offset="1" stop-color="#d9764b"/></linearGradient>')

# Orange: whole orange + half with segments
segs=''.join(f'<path d="M650 640 L{650+118*math.cos(2*math.pi*i/10):.0f} {640+118*math.sin(2*math.pi*i/10):.0f}" fill="none" stroke="#f6ecd8" stroke-width="10"/>' for i in range(10))
save("Fruits","Orange","sage",f'''
{sh(520,820,310,36)}
<circle cx="420" cy="480" r="220" fill="#e58a3c" stroke-width="16"/>
<path d="M290 380 Q320 310 400 290" fill="none" stroke="#ffffff" stroke-width="20" opacity="0.35"/>
<g fill="#c46a2a" stroke="none" opacity="0.5"><circle cx="360" cy="500" r="6"/><circle cx="450" cy="560" r="6"/><circle cx="500" cy="420" r="6"/><circle cx="330" cy="600" r="6"/><circle cx="420" cy="400" r="6"/></g>
<path d="M430 262 L440 220" fill="none" stroke="#6b4a33" stroke-width="14"/>
{leaf(438,228,-30,1.1)}
<circle cx="650" cy="640" r="170" fill="#e58a3c" stroke-width="16"/>
<circle cx="650" cy="640" r="145" fill="#f6ecd8" stroke-width="8"/>
<circle cx="650" cy="640" r="125" fill="#f0a24a" stroke-width="8"/>
{segs}
<circle cx="650" cy="640" r="16" fill="#f6ecd8" stroke="none"/>
<path d="M550 580 Q570 540 610 525" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.5"/>
''')

# Papaya: whole papaya + half with orange flesh and black seeds
pseeds=''.join(f'<circle cx="{x}" cy="{y}" r="11" fill="#2e211b" stroke="none"/>' for x,y in [(640,560),(665,590),(625,605),(655,630),(630,650),(670,665),(640,690),(660,720),(625,735),(650,760)])
save("Fruits","Papaya","beige",f'''
{sh(520,830,320,36)}
<path d="M400 200 Q520 210 540 400 Q560 560 520 700 Q480 820 380 800 Q270 780 260 620 Q250 460 300 330 Q330 210 400 200Z" fill="#7fa05a" stroke-width="16"/>
<path d="M400 200 Q520 210 540 400 Q560 560 520 700 Q490 790 430 800 Q480 600 450 420 Q430 260 400 200Z" fill="#e0b04f" stroke="none" opacity="0.75"/>
<path d="M400 200 Q520 210 540 400 Q560 560 520 700 Q480 820 380 800 Q270 780 260 620 Q250 460 300 330 Q330 210 400 200Z" fill="none" stroke-width="16"/>
<path d="M300 420 Q310 320 360 260" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.35"/>
<path d="M398 205 L400 170" fill="none" stroke="#6b4a33" stroke-width="16"/>
<path d="M650 420 Q760 430 780 600 Q800 780 660 820 Q530 830 510 660 Q500 450 650 420Z" fill="#7fa05a" stroke-width="16"/>
<path d="M650 448 Q740 455 755 600 Q770 760 660 792 Q555 800 540 660 Q530 480 650 448Z" fill="#e8874a" stroke-width="10"/>
<path d="M648 520 Q700 525 705 640 Q710 760 650 775 Q595 770 595 650 Q595 525 648 520Z" fill="#f2b25a" stroke-width="8"/>
{pseeds}
<path d="M570 520 Q590 475 640 468" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.5"/>
''')

# Pineapple
cross=''.join(f'<path d="M{200+i*60} 380 L{200+i*60+320} 900" fill="none" stroke-width="8" opacity="0.45"/><path d="M{824-i*60} 380 L{824-i*60-320} 900" fill="none" stroke-width="8" opacity="0.45"/>' for i in range(-4,11))
eyes=''.join(f'<path d="M{x-10} {y+6} Q{x} {y-10} {x+10} {y+6}" fill="none" stroke="#9a6a2a" stroke-width="7"/>' for x,y in [(452,470),(572,470),(512,530),(392,530),(632,530),(452,590),(572,590),(512,650),(392,650),(632,650),(452,710),(572,710),(512,770)])
crown=''.join(f'<path transform="rotate({r} 512 400)" d="M492 400 Q480 300 512 {h} Q544 300 532 400Z" fill="{c}" stroke-width="11"/>' for r,h,c in [(-50,200,"#7fa05a"),(50,200,"#7fa05a"),(-30,160,"#5f8f4e"),(30,160,"#5f8f4e"),(-12,140,"#7fa05a"),(12,140,"#7fa05a"),(0,120,"#5f8f4e")])
save("Fruits","Pineapple","sky",f'''
<defs><clipPath id="pc"><ellipse cx="512" cy="620" rx="190" ry="240"/></clipPath></defs>
{sh(512,860,230,30)}
{crown}
<ellipse cx="512" cy="620" rx="190" ry="240" fill="#e0a43f" stroke-width="16"/>
<g clip-path="url(#pc)">{cross}
<path d="M600 400 Q720 600 610 850 L720 850 L720 400Z" fill="#2e211b" opacity="0.12" stroke="none"/></g>
{eyes}
<path d="M390 520 Q400 440 450 410" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.35"/>
<ellipse cx="512" cy="620" rx="190" ry="240" fill="none" stroke-width="16"/>
''')

# Watermelon: whole striped melon + slice
stripes=''.join(f'<path d="M{x} 280 Q{x+ (x-400)*0.35} 480 {x} 690" fill="none" stroke="#3f6a3a" stroke-width="30"/>' for x in [260,330,400,470,540])
wseeds=''.join(f'<ellipse cx="{x}" cy="{y}" rx="8" ry="13" fill="#2e211b" stroke="none" transform="rotate({r} {x} {y})"/>' for x,y,r in [(560,560,-10),(640,570,-5),(720,560,5),(800,570,10),(600,620,-8),(680,625,0),(760,620,8),(640,680,-5),(720,680,5),(680,735,0)])
save("Fruits","Watermelon","rose",f'''
<defs><clipPath id="wc"><ellipse cx="400" cy="480" rx="230" ry="210"/></clipPath></defs>
{sh(520,820,330,36)}
<ellipse cx="400" cy="480" rx="230" ry="210" fill="#7fa05a" stroke-width="16"/>
<g clip-path="url(#wc)">{stripes}</g>
<path d="M240 400 Q270 320 350 290" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.35"/>
<ellipse cx="400" cy="480" rx="230" ry="210" fill="none" stroke-width="16"/>
<path d="M400 272 Q395 245 410 230" fill="none" stroke="#6b4a33" stroke-width="12"/>
<path d="M480 500 L870 500 Q860 640 770 720 Q680 800 580 760 Q480 700 480 500Z" transform="translate(0 30)" fill="#5f8f4e" stroke-width="16"/>
<path d="M500 530 L850 530 Q840 650 760 715 Q680 770 590 740 Q500 690 500 530Z" fill="#f6ecd8" stroke="none"/>
<path d="M515 530 L835 530 Q825 640 752 700 Q680 752 598 726 Q515 680 515 530Z" fill="#e0625a" stroke-width="10"/>
<path d="M480 530 L870 530" fill="none" stroke-width="16"/>
{wseeds}
<path d="M540 550 L700 550" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.4"/>
''')

import math, random, sys
from common import *
random.seed(7)
K="#2e211b"
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def hl(d,w=16,op=0.3): return f'<path d="{d}" fill="none" stroke="#ffffff" stroke-width="{w}" opacity="{op}"/>'
def dk(d,w=16,op=0.15): return f'<path d="{d}" fill="none" stroke="#2e211b" stroke-width="{w}" opacity="{op}"/>'
def leaf(x,y,rot,s=1,col="#5f8f4e"): return f'<path transform="translate({x} {y}) rotate({rot}) scale({s})" d="M0 0 Q35 -40 90 -25 Q50 15 0 0Z" fill="{col}" stroke-width="{9/s:.0f}"/><path transform="translate({x} {y}) rotate({rot}) scale({s})" d="M8 -3 Q45 -18 80 -24" fill="none" stroke-width="{5/s:.0f}" opacity="0.4"/>'
def tube(d,w,col,o=12): return f'<path d="{d}" fill="none" stroke-width="{w+2*o}"/><path d="{d}" fill="none" stroke="{col}" stroke-width="{w}"/>'
def bez(p0,p1,p2,p3,t):
    u=1-t
    return tuple(u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d for a,b,c,d in zip(p0,p1,p2,p3))
def plate(cx,cy,rx,ry,col="#f6ecd8",inner="#e8dcc6",band=None):
    s=sh(cx,cy+ry*0.35,rx+10,ry*0.55)
    s+=f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{col}" stroke-width="16"/>'
    if band: s+=f'<ellipse cx="{cx}" cy="{cy}" rx="{rx-22}" ry="{ry-14}" fill="none" stroke="{band}" stroke-width="10"/>'
    s+=f'<ellipse cx="{cx}" cy="{cy-8}" rx="{rx*0.76:.0f}" ry="{ry*0.7:.0f}" fill="{inner}" stroke-width="8"/>'
    return s
def shrimp_small(x,y,rot,s=1):
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({s})">
<path d="M-60 -10 C-60 -70 40 -80 70 -20 C85 10 70 45 40 55" fill="none" stroke-width="58"/>
<path d="M-60 -10 C-60 -70 40 -80 70 -20 C85 10 70 45 40 55" fill="none" stroke="#e8875f" stroke-width="36"/>
<path d="M-20 -60 L-10 -35 M15 -62 L15 -35 M45 -48 L35 -25 M68 -18 L45 -8" stroke-width="6" opacity="0.5"/>
<path d="M40 55 L10 85 L60 90Z" fill="#e8875f" stroke-width="9"/>
<circle cx="-62" cy="-28" r="6" fill="#2e211b" stroke="none"/>
</g>'''
def lime_wedge(x,y,rot,s=1):
    return f'<g transform="translate({x} {y}) rotate({rot}) scale({s})"><path d="M-80 0 A80 80 0 0 0 80 0Z" fill="#8fbf5a" stroke-width="{12/s:.0f}"/><path d="M-60 6 A60 60 0 0 0 60 6Z" fill="#d6e8a8" stroke="none"/><path d="M0 6 L0 60 M0 6 L-40 44 M0 6 L40 44" stroke="#8fbf5a" stroke-width="{5/s:.0f}"/></g>'
def chili_small(x,y,rot,s=1,col="#c8574b"):
    return f'<g transform="translate({x} {y}) rotate({rot}) scale({s})"><path d="M0 0 C40 -10 110 10 150 70 C110 55 50 40 5 25Z" fill="{col}" stroke-width="{10/s:.0f}"/><path d="M0 12 C-15 8 -25 -5 -35 -15" fill="none" stroke-width="{24/s:.0f}"/><path d="M0 12 C-15 8 -25 -5 -35 -15" fill="none" stroke="#5f8f4e" stroke-width="{10/s:.0f}"/></g>'
def cube(x,y,s,top="#ffffff",l="#ece6dc",r="#d8d0c4",sw=10):
    a=s*0.87;h=s*0.5
    return (f'<path d="M{x} {y-h} L{x+a} {y} L{x} {y+h} L{x-a} {y}Z" fill="{top}" stroke-width="{sw}"/>'
            f'<path d="M{x-a} {y} L{x} {y+h} L{x} {y+h+s} L{x-a} {y+s}Z" fill="{l}" stroke-width="{sw}"/>'
            f'<path d="M{x} {y+h} L{x+a} {y} L{x+a} {y+s} L{x} {y+h+s}Z" fill="{r}" stroke-width="{sw}"/>')
def dots(pts,r,col,op=1,sw=0):
    st=f'stroke-width="{sw}"' if sw else 'stroke="none"'
    return ''.join(f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r}" fill="{col}" opacity="{op}" {st}/>' for x,y in pts)
def rnd_in_ellipse(cx,cy,rx,ry,n):
    out=[]
    while len(out)<n:
        x=random.uniform(-1,1);y=random.uniform(-1,1)
        if x*x+y*y<1: out.append((cx+x*rx,cy+y*ry))
    return out

def bsave(k,bg,body,sc=1.18,dy=-30):
    save('Food',k,bg,f'<g transform="translate(512 {610+dy}) scale({sc}) translate(-512 -610)">{body}</g>')
# ---------------- Mushroom
def mushroom(tx,ty,s,cap="#b07a55"):
    gills=''.join(f'<path d="M{445+i*38} {568} L{445+i*48} {592}" stroke-width="6" opacity="0.35"/>' for i in range(-4,5))
    return f'''<g transform="translate({tx} {ty}) scale({s})">
<ellipse cx="445" cy="560" rx="222" ry="42" fill="#ecd9b8" stroke-width="{14/s:.0f}"/>
{gills}
<path d="M385 790 Q372 660 395 575 L495 575 Q520 660 508 790 Q446 808 385 790Z" fill="#f3e6cf" stroke-width="{14/s:.0f}"/>
<path d="M410 600 Q400 690 410 770" fill="none" stroke="#2e211b" stroke-width="12" opacity="0.1"/>
<path d="M480 600 Q492 690 485 775" fill="none" stroke="#2e211b" stroke-width="14" opacity="0.15"/>
<path d="M223 560 C220 400 320 312 445 312 C570 312 670 400 667 560 Q445 522 223 560Z" fill="{cap}" stroke-width="{15/s:.0f}"/>
<path d="M600 360 C650 410 665 480 660 545 Q620 540 590 538 C610 470 610 410 600 360Z" fill="#2e211b" opacity="0.15" stroke="none"/>
{hl("M280 480 C290 400 350 350 430 340",22,0.35)}
<ellipse cx="520" cy="400" rx="26" ry="16" fill="#ffffff" opacity="0.25" stroke="none"/>
<ellipse cx="360" cy="470" rx="18" ry="12" fill="#ffffff" opacity="0.25" stroke="none"/>
</g>'''
save("Food","Mushroom","sage",f'''
{sh(512,800,330,40)}
{mushroom(430,310,0.6,"#a8744f")}
{mushroom(-60,-10,1,"#b07a55")}
{leaf(250,800,-30,0.9,"#7fa05a")}{leaf(760,815,200,0.8,"#5f8f4e")}
''')

# ---------------- Noodle soup
noodles=''.join(tube(f"M{x} {y} q30 -22 60 0 t60 0 t60 0",10,"#f0d58a",5) for x,y in [(330,500),(380,520),(450,490),(520,515),(560,495),(410,470),(600,470)])
save("Food","Noodle soup","sage",f'''
{sh(512,830,300,35)}
{steam([420,512,604],300)}
<path d="M392 770 L380 820 L644 820 L632 770Z" fill="#9a6a45" stroke-width="12"/>
<path d="M200 500 Q210 790 512 800 Q814 790 824 500Z" fill="#f6ecd8" stroke-width="16"/>
<path d="M214 580 Q512 630 810 580" fill="none" stroke="#c8574b" stroke-width="16"/>
<path d="M250 640 Q380 670 512 672 Q644 670 774 640" fill="none" stroke="#c8574b" stroke-width="6" stroke-dasharray="20 22" opacity="0.8"/>
{hl("M250 600 Q280 730 380 770",18,0.4)}
<ellipse cx="512" cy="500" rx="312" ry="78" fill="#f6ecd8" stroke-width="16"/>
<ellipse cx="512" cy="506" rx="282" ry="60" fill="#d9924e" stroke-width="8"/>
{noodles}
<g fill="#f6ecd8" stroke-width="9"><circle cx="400" cy="505" r="28"/><circle cx="450" cy="530" r="24"/><circle cx="640" cy="515" r="26"/></g>
<g fill="#e3a08e" stroke-width="9"><path d="M520 480 Q560 455 610 470 Q600 505 540 510Z"/><path d="M300 505 Q330 480 370 490 Q370 520 320 525Z"/></g>
<path d="M540 482 Q570 475 598 480 M318 505 Q340 497 358 500" fill="none" stroke="#f6ecd8" stroke-width="6"/>
<g stroke-width="7" fill="#7fa05a"><circle cx="500" cy="490" r="10"/><circle cx="700" cy="500" r="10"/><circle cx="580" cy="530" r="9"/><circle cx="350" cy="530" r="9"/><circle cx="470" cy="470" r="8"/></g>
{leaf(660,470,-30,0.55,"#5f8f4e")}{leaf(420,540,-170,0.5,"#5f8f4e")}
<path d="M590 520 L770 250" stroke-width="30"/><path d="M590 520 L770 250" stroke="#9a6a45" stroke-width="12"/>
<path d="M625 525 L805 275" stroke-width="30"/><path d="M625 525 L805 275" stroke="#9a6a45" stroke-width="12"/>
''')

# ---------------- Omelette
omel_pts=[(420,560),(480,600),(560,560),(530,620),(390,620),(610,600)]
bsave("Omelette","sky",f'''
{plate(512,610,350,165,band="#5d82a8")}
<path d="M270 610 C255 520 330 450 430 465 C490 415 610 425 660 480 C720 470 740 560 700 620 C690 690 560 720 470 700 C360 710 280 680 270 610Z" fill="#d9953f" stroke-width="15"/>
<path d="M300 600 C295 530 350 485 430 495 C490 450 600 460 640 505 C690 505 700 570 670 620 C650 670 560 690 470 675 C370 685 305 660 300 600Z" fill="#eebd55" stroke="none"/>
<path d="M360 560 C380 520 430 510 470 520 C500 505 550 510 570 540 C560 580 500 590 450 585 C400 595 360 590 360 560Z" fill="#f6d97e" stroke="none"/>
{''.join(f'<ellipse cx="{x}" cy="{y}" rx="22" ry="12" fill="#c98a3b" opacity="0.45" stroke="none"/>' for x,y in omel_pts)}
{hl("M320 560 C330 510 380 490 430 495",14,0.45)}
<g fill="#7fa05a" stroke-width="6"><circle cx="450" cy="545" r="9"/><circle cx="530" cy="590" r="8"/><circle cx="600" cy="530" r="8"/><circle cx="400" cy="600" r="7"/></g>
<path d="M650 560 Q700 470 790 500 Q800 580 730 640 Q680 640 650 560Z" fill="#7fa05a" stroke-width="12"/>
<path d="M670 580 Q720 520 780 510" fill="none" stroke-width="6" opacity="0.4"/>
<g transform="rotate(-15 730 620)"><circle cx="730" cy="620" r="46" fill="#c8574b" stroke-width="11"/><circle cx="730" cy="620" r="30" fill="#e8907f" stroke="none"/><circle cx="718" cy="610" r="6" fill="#f6ecd8" stroke="none"/><circle cx="742" cy="630" r="6" fill="#f6ecd8" stroke="none"/><circle cx="725" cy="635" r="6" fill="#f6ecd8" stroke="none"/></g>
<circle cx="660" cy="665" r="40" fill="#8fbf5a" stroke-width="11"/><circle cx="660" cy="665" r="27" fill="#e2efc0" stroke="none"/>{dots([(652,658),(668,660),(660,675)],5,"#8fbf5a")}
''')

# ---------------- Onion
save("Food","Onion","sand",f'''
{sh(512,810,220,32)}
<g fill="none" stroke-width="30"><path d="M500 360 Q470 250 420 170"/><path d="M512 355 Q515 240 540 150"/><path d="M522 360 Q570 270 630 200"/></g>
<g fill="none" stroke="#7fa05a" stroke-width="14"><path d="M500 360 Q470 250 420 170"/><path d="M512 355 Q515 240 540 150"/><path d="M522 360 Q570 270 630 200"/></g>
<path d="M512 330 C610 380 730 480 708 625 C690 745 600 795 512 795 C424 795 334 745 316 625 C294 480 414 380 512 330Z" fill="#a8577a" stroke-width="16"/>
<path d="M512 350 C450 420 420 520 430 640 C440 730 470 780 500 792" fill="none" stroke-width="7" opacity="0.35"/>
<path d="M512 350 C574 420 604 520 594 640 C584 730 554 780 524 792" fill="none" stroke-width="7" opacity="0.35"/>
<path d="M512 350 C380 430 350 560 380 690" fill="none" stroke-width="7" opacity="0.25"/>
<path d="M512 350 C644 430 674 560 644 690" fill="none" stroke-width="7" opacity="0.25"/>
<path d="M600 420 C680 490 700 580 680 660 C660 730 610 770 560 785 C640 700 650 560 600 420Z" fill="#2e211b" opacity="0.15" stroke="none"/>
{hl("M370 560 C375 480 420 430 470 400",24,0.35)}
<path d="M480 340 Q512 300 544 340" fill="#e8c9a0" stroke-width="10"/>
<g fill="none" stroke-width="8"><path d="M490 795 Q470 830 450 845"/><path d="M512 798 L512 850"/><path d="M534 795 Q555 830 575 845"/><path d="M500 798 Q490 830 480 855"/></g>
''')

# ---------------- Papaya salad (mortar krok)
strands=''
for i in range(34):
    x=random.uniform(330,690); y=random.uniform(400,500)
    top=505-(1-((x-512)/200)**2)*130
    if y<top+10: continue
    dx=random.uniform(-40,40); col=random.choice(["#8fae5e","#8fae5e","#8fae5e","#e0904f"])
    strands+=f'<path d="M{x:.0f} {y:.0f} q{dx/2:.0f} -14 {dx:.0f} {random.uniform(-10,10):.0f}" fill="none" stroke="{col}" stroke-width="7"/>'
def tomw(x,y,r): return f'<g transform="rotate({r} {x} {y})"><path d="M{x-40} {y} A40 40 0 0 0 {x+40} {y}Z" fill="#c8574b" stroke-width="10"/><path d="M{x-26} {y+4} A26 26 0 0 0 {x+26} {y+4}Z" fill="#e8907f" stroke="none"/></g>'
save("Food","Papaya salad_Central Thai style_","beige",f'''
{sh(512,815,300,38)}
<path d="M700 470 L815 235" stroke-width="64"/><path d="M700 470 L815 235" stroke="#b58255" stroke-width="40"/>
{hl("M712 440 L810 245",10,0.3)}
<path d="M395 780 L385 815 L639 815 L629 780Z" fill="#8a4f30" stroke-width="12"/>
<path d="M262 505 Q275 765 512 790 Q749 765 762 505Z" fill="#b8683e" stroke-width="16"/>
{hl("M300 560 Q320 700 410 750",20,0.25)}
{dk("M700 560 Q690 690 610 750",22,0.15)}
<ellipse cx="512" cy="505" rx="252" ry="58" fill="#9a5433" stroke-width="16"/>
<path d="M300 510 Q330 385 512 370 Q694 385 724 510 Q512 548 300 510Z" fill="#dbe7a8" stroke-width="13"/>
{strands}
{hl("M340 480 Q360 420 430 395",12,0.5)}
{tomw(420,470,-20)}{tomw(600,450,25)}{tomw(520,505,0)}
{tube("M370 420 L480 400",12,"#6f9a4a",8)}{tube("M560 490 L665 470",12,"#6f9a4a",8)}
<g fill="#d9a86a" stroke-width="6"><ellipse cx="480" cy="445" rx="13" ry="10"/><ellipse cx="560" cy="420" rx="13" ry="10"/><ellipse cx="640" cy="495" rx="13" ry="10"/><ellipse cx="380" cy="495" rx="13" ry="10"/><ellipse cx="512" cy="400" rx="12" ry="9"/></g>
<g fill="none" stroke="#e8875f" stroke-width="9"><path d="M450 490 q12 -16 24 0"/><path d="M590 510 q12 -16 24 0"/><path d="M660 440 q12 -16 24 0"/></g>
{lime_wedge(215,760,-25,0.85)}
{chili_small(735,760,-10,0.9)}{chili_small(770,715,15,0.75,"#7fa05a")}
''')

# ---------------- Pepper (grinder + peppercorns)
pcs=[(640,790),(690,805),(740,785),(670,760),(720,752),(770,810),(610,815),(820,780)]
corns=''.join(f'<circle cx="{x}" cy="{y}" r="17" fill="#3b2a22" stroke-width="8"/><circle cx="{x-5}" cy="{y-5}" r="5" fill="#ffffff" opacity="0.35" stroke="none"/>' for x,y in pcs)
save("Food","Pepper","beige",f'''
{sh(512,818,320,36)}
<circle cx="440" cy="205" r="30" fill="#5c3d2e" stroke-width="12"/>
<path d="M350 330 Q350 245 440 238 Q530 245 530 330Z" fill="#5c3d2e" stroke-width="15"/>
{hl("M375 310 Q380 265 425 255",12,0.3)}
<rect x="340" y="325" width="200" height="38" rx="10" fill="#b9b2a6" stroke-width="13"/>
<path d="M360 363 Q330 480 375 570 Q330 680 352 800 L528 800 Q550 680 505 570 Q550 480 520 363Z" fill="#6b4634" stroke-width="16"/>
{hl("M385 390 Q365 480 400 565 Q362 670 380 780",20,0.3)}
{dk("M495 390 Q515 480 480 570 Q520 680 505 780",22,0.2)}
<rect x="340" y="790" width="200" height="30" rx="10" fill="#b9b2a6" stroke-width="13"/>
<ellipse cx="700" cy="822" rx="110" ry="14" fill="#3b2a22" opacity="0.4" stroke="none"/>
{corns}
{dots([(560,810),(575,825),(590,812),(548,822)],5,"#3b2a22",0.8)}
''')

# ---------------- Pork (pig)
save("Food","Pork","rose",f'''
{sh(530,815,290,34)}
<path d="M770 540 C820 520 830 470 800 460 C775 455 770 495 800 500 C830 505 845 480 840 455" fill="none" stroke-width="12"/>
<g fill="#e89a92" stroke-width="13"><rect x="410" y="680" width="62" height="125" rx="24"/><rect x="630" y="680" width="62" height="125" rx="24"/></g>
<ellipse cx="560" cy="580" rx="245" ry="170" fill="#f1b1a6" stroke-width="16"/>
<g fill="#f1b1a6" stroke-width="13"><rect x="350" y="690" width="66" height="125" rx="26"/><rect x="570" y="690" width="66" height="125" rx="26"/></g>
<g fill="#5c3d2e" stroke-width="10"><path d="M352 790 L414 790 L414 805 Q414 815 400 815 L366 815 Q352 815 352 805Z"/><path d="M572 790 L634 790 L634 805 Q634 815 620 815 L586 815 Q572 815 572 805Z"/></g>
{hl("M430 470 Q520 420 640 430",22,0.35)}
{dk("M760 600 Q740 700 640 740",24,0.12)}
<path d="M260 330 L250 220 L340 280Z" fill="#f1b1a6" stroke-width="13"/><path d="M270 315 L264 250 L320 285Z" fill="#e38c86" stroke="none"/>
<path d="M430 290 L480 190 L500 300Z" fill="#f1b1a6" stroke-width="13"/><path d="M445 290 L475 225 L487 295Z" fill="#e38c86" stroke="none"/>
<circle cx="370" cy="450" r="150" fill="#f1b1a6" stroke-width="16"/>
<ellipse cx="340" cy="505" rx="72" ry="52" fill="#e38c86" stroke-width="13"/>
<ellipse cx="318" cy="505" rx="12" ry="18" fill="#8a3b33" stroke="none"/><ellipse cx="362" cy="505" rx="12" ry="18" fill="#8a3b33" stroke="none"/>
<circle cx="300" cy="410" r="14" fill="#2e211b"/><circle cx="420" cy="410" r="14" fill="#2e211b"/>
<ellipse cx="260" cy="470" rx="26" ry="16" fill="#e8907f" opacity="0.6" stroke="none"/><ellipse cx="445" cy="470" rx="28" ry="17" fill="#e8907f" opacity="0.6" stroke="none"/>
<path d="M320 575 Q345 592 370 575" fill="none" stroke-width="9"/>
''')

# ---------------- Potato
def potato(x,y,rx,ry,rot):
    eyes=''.join(f'<path d="M{x+dx-10} {y+dy} q10 -8 20 0" fill="none" stroke-width="7" opacity="0.5"/>' for dx,dy in [(-rx*0.4,-ry*0.3),(rx*0.3,-ry*0.1),(-rx*0.1,ry*0.4),(rx*0.5,ry*0.35)])
    return f'''<g transform="rotate({rot} {x} {y})">
<path d="M{x-rx} {y} C{x-rx} {y-ry*0.8} {x-rx*0.5} {y-ry} {x} {y-ry*0.95} C{x+rx*0.6} {y-ry*1.05} {x+rx} {y-ry*0.6} {x+rx} {y} C{x+rx*1.02} {y+ry*0.7} {x+rx*0.5} {y+ry} {x-rx*0.05} {y+ry} C{x-rx*0.6} {y+ry} {x-rx*1.02} {y+ry*0.6} {x-rx} {y}Z" fill="#c9a06c" stroke-width="15"/>
<path d="M{x+rx*0.3} {y-ry*0.8} C{x+rx*0.95} {y-ry*0.5} {x+rx} {y+ry*0.5} {x+rx*0.3} {y+ry*0.9} C{x+rx*0.7} {y+ry*0.3} {x+rx*0.7} {y-ry*0.3} {x+rx*0.3} {y-ry*0.8}Z" fill="#2e211b" opacity="0.15" stroke="none"/>
{hl(f"M{x-rx*0.75} {y+ry*0.1} C{x-rx*0.75} {y-ry*0.5} {x-rx*0.4} {y-ry*0.75} {x-rx*0.05} {y-ry*0.78}",18,0.35)}
{eyes}
{dots([(x-rx*0.2,y+ry*0.1),(x+rx*0.1,y-ry*0.5),(x-rx*0.55,y+ry*0.3),(x+rx*0.55,y-ry*0.3)],4,"#7a5638",0.6)}
</g>'''
save("Food","Potato","beige",f'''
{sh(512,790,330,40)}
{potato(400,540,160,120,-25)}
{potato(640,560,150,112,20)}
<g transform="rotate(-10 470 720)">
<ellipse cx="470" cy="720" rx="130" ry="80" fill="#b58a58" stroke-width="15"/>
<ellipse cx="470" cy="712" rx="112" ry="64" fill="#f3dc8a" stroke="none"/>
<ellipse cx="455" cy="705" rx="70" ry="32" fill="#f8eab0" stroke="none"/>
{dots([(430,700),(500,730),(480,690),(520,705)],3,"#d9b860",0.8)}
</g>
''')

# ---------------- Salt (white shaker + pile)
holes=''.join(f'<circle cx="{x}" cy="{y}" r="7" fill="#5c5650" stroke="none"/>' for x,y in [(400,262),(440,255),(480,262),(420,285),(460,285)])
salt_grains=dots(rnd_in_ellipse(440,640,110,110,40),4,"#c9cdd2",0.8)
crystals=''.join(f'<rect x="{x}" y="{y}" width="14" height="14" rx="2" transform="rotate({r} {x+7} {y+7})" fill="#ffffff" stroke-width="5"/>' for x,y,r in [(560,790,20),(600,806,-15),(835,790,30),(800,808,10),(700,700,25),(655,735,-30)])
save("Food","Salt","sky",f'''
{sh(512,820,330,36)}
<path d="M340 360 Q350 300 440 290 Q530 300 540 360Z" fill="#b9b2a6" stroke-width="14"/>
<path d="M340 360 L340 310 Q340 225 440 222 Q540 225 540 310 L540 360Z" fill="#c9c3b8" stroke-width="15"/>
{holes}
{hl("M365 330 Q365 260 420 245",12,0.45)}
<rect x="330" y="350" width="220" height="40" rx="12" fill="#a8a196" stroke-width="13"/>
<path d="M345 390 L325 760 Q325 810 380 810 L500 810 Q555 810 555 760 L535 390Z" fill="#eef4f7" stroke-width="16"/>
<path d="M338 520 L326 760 Q326 806 380 806 L500 806 Q554 806 554 760 L542 520 Q440 540 338 520Z" fill="#ffffff" stroke="none"/>
<path d="M338 520 Q440 540 542 520" fill="none" stroke-width="7" opacity="0.3"/>
{salt_grains}
{hl("M370 420 L355 770",22,0.7)}
{dk("M515 420 L528 770",18,0.08)}
<path d="M570 815 Q600 740 690 725 Q790 740 830 815Z" fill="#ffffff" stroke-width="13"/>
{dots(rnd_in_ellipse(700,780,90,22,18),4,"#c9cdd2",0.9)}
{crystals}
''')

# ---------------- Seafood (platter)
def clam(x,y,rot,s=1,col="#e8c9a0"):
    ribs=''.join(f'<path d="M0 30 L{a} -40" stroke-width="{5/s:.0f}" opacity="0.4"/>' for a in (-36,-18,0,18,36))
    return f'<g transform="translate({x} {y}) rotate({rot}) scale({s})"><path d="M0 34 L-55 -10 Q-40 -58 0 -60 Q40 -58 55 -10Z" fill="{col}" stroke-width="{10/s:.0f}"/>{ribs}<rect x="-16" y="26" width="32" height="14" rx="5" fill="{col}" stroke-width="{8/s:.0f}"/></g>'
crab_legs=''.join(f'<path d="M{512+sx*90} {575+i*16} q{sx*70} {-30+i*10} {sx*120} {10+i*20}" fill="none" stroke-width="34"/><path d="M{512+sx*90} {575+i*16} q{sx*70} {-30+i*10} {sx*120} {10+i*20}" fill="none" stroke="#d9644b" stroke-width="14"/>' for sx in (-1,1) for i in range(3))
save("Food","Seafood","sky",f'''
{sh(512,745,380,60)}
<ellipse cx="512" cy="620" rx="380" ry="175" fill="#9a6a45" stroke-width="16"/>
<ellipse cx="512" cy="610" rx="330" ry="138" fill="#dcecf1" stroke-width="9"/>
{dots(rnd_in_ellipse(512,615,300,120,30),9,"#ffffff",0.8)}
{shrimp_small(265,615,-25,1.0)}{shrimp_small(765,625,30,1.0)}
{clam(380,690,-20,0.9)}{clam(650,700,15,0.85,"#d9b08a")}
{crab_legs}
<path d="M430 520 L385 450 M594 520 L639 450" fill="none" stroke-width="34"/>
<path d="M430 520 L385 450 M594 520 L639 450" fill="none" stroke="#d9644b" stroke-width="14"/>
<g fill="#d9644b" stroke-width="12"><path d="M385 470 C330 450 330 380 380 370 L372 410 L405 395 C420 430 410 460 385 470Z"/><path d="M639 470 C694 450 694 380 644 370 L652 410 L619 395 C604 430 614 460 639 470Z"/></g>
<ellipse cx="512" cy="580" rx="130" ry="85" fill="#d9644b" stroke-width="15"/>
{hl("M420 560 Q440 515 500 505",14,0.35)}
{dk("M600 600 Q580 640 530 655",16,0.15)}
<path d="M480 510 L475 480 M544 510 L549 480" stroke-width="9"/>
<circle cx="475" cy="475" r="13" fill="#f6ecd8" stroke-width="8"/><circle cx="549" cy="475" r="13" fill="#f6ecd8" stroke-width="8"/>
<circle cx="477" cy="477" r="5" fill="#2e211b" stroke="none"/><circle cx="551" cy="477" r="5" fill="#2e211b" stroke="none"/>
{lime_wedge(540,720,10,0.75)}
''')

# ---------------- Seasoning (Thai condiment caddy)
def jar(cx,fill,extra=""):
    return f'''<path d="M{cx-58} 480 L{cx-58} 700 Q{cx-58} 722 {cx-36} 722 L{cx+36} 722 Q{cx+58} 722 {cx+58} 700 L{cx+58} 480Z" fill="#eef4f7" stroke-width="13"/>
<path d="M{cx-52} 540 L{cx-52} 698 Q{cx-52} 716 {cx-34} 716 L{cx+34} 716 Q{cx+52} 716 {cx+52} 698 L{cx+52} 540Z" fill="{fill}" stroke="none"/>
<path d="M{cx-52} 540 L{cx+52} 540" stroke-width="6" opacity="0.35"/>
{extra}
<path d="M{cx+18} 540 L{cx+30} 400" stroke-width="20"/><path d="M{cx+18} 540 L{cx+30} 400" stroke="#d8d2c8" stroke-width="8"/>
{hl(f"M{cx-38} 500 L{cx-38} 690",12,0.6)}
<rect x="{cx-66}" y="465" width="132" height="26" rx="10" fill="#b9b2a6" stroke-width="11"/>'''
def slices(pts,col): return ''.join(f'<circle cx="{x}" cy="{y}" r="11" fill="{col}" stroke-width="6"/><circle cx="{x}" cy="{y}" r="4" fill="#f3d9a0" stroke="none"/>' for x,y in pts)
save("Food","Seasoning","sand",f'''
{sh(512,800,330,34)}
<path d="M512 740 L512 330" stroke-width="34"/><path d="M512 740 L512 330" stroke="#b9b2a6" stroke-width="16"/>
<circle cx="512" cy="290" r="44" fill="none" stroke-width="36"/><circle cx="512" cy="290" r="44" fill="none" stroke="#b9b2a6" stroke-width="16"/>
{jar(292,"#fbf8f0",dots([(270,600),(300,650),(320,590),(280,680),(305,620)],4,"#d8d0c4"))}
{jar(418,"#c8574b",dots([(395,590),(430,620),(410,670),(440,580),(400,640),(425,690)],6,"#7a2e26"))}
{jar(606,"#c98a3b",slices([(590,600),(625,650),(600,690)],"#c8574b"))}
{jar(732,"#efe6c4",slices([(715,610),(750,660),(725,695)],"#7fa05a"))}
<path d="M205 720 L819 720 L805 790 Q800 800 790 800 L234 800 Q224 800 219 790Z" fill="#a8a196" stroke-width="15"/>
{hl("M235 745 L790 745",10,0.35)}
''')

# ---------------- Shrimp (big)
P=[(330,520),(320,300),(650,270),(730,470)]
Q2=[(730,470),(770,570),(720,670),(620,700)]
pts=[bez(*P,t/6) for t in range(7)]+[bez(*Q2,t/4) for t in range(1,5)]
radii=[100,96,92,88,84,80,74,66,58,50,42]
segs=''
for (x,y),r in list(zip(pts,radii))[::-1][:-1]:
    segs+=f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r}" fill="#e8875f" stroke-width="13"/><path d="M{x-r*0.55:.0f} {y-r*0.55:.0f} A{r*0.78:.0f} {r*0.78:.0f} 0 0 1 {x+r*0.35:.0f} {y-r*0.7:.0f}" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.3"/>'
tx,ty=pts[-1]
legs=''.join(f'<path d="M{x:.0f} {y+40:.0f} q{-20} 50 {-5} 90" fill="none" stroke-width="20"/><path d="M{x:.0f} {y+40:.0f} q{-20} 50 {-5} 90" fill="none" stroke="#e8875f" stroke-width="8"/>' for x,y in [(390,560),(430,520),(480,440),(540,400)] )
save("Food","Shrimp","sky",f'''
{sh(512,820,300,34)}
<path d="M270 470 Q170 300 230 160" fill="none" stroke-width="10"/><path d="M290 460 Q230 330 330 200" fill="none" stroke-width="10"/>
{legs}
<path d="M{tx:.0f} {ty:.0f} L{tx-120:.0f} {ty+60:.0f} Q{tx-60:.0f} {ty+140:.0f} {tx+30:.0f} {ty+110:.0f}Z" fill="#e0764f" stroke-width="13"/>
<path d="M{tx-15:.0f} {ty+10:.0f} L{tx-85:.0f} {ty+80:.0f} M{tx-5:.0f} {ty+15:.0f} L{tx-25:.0f} {ty+110:.0f}" stroke-width="7" opacity="0.4"/>
{segs}
<path d="M230 520 C230 430 300 400 370 420 C420 440 440 520 420 600 C390 640 300 640 260 600 C240 580 230 550 230 520Z" fill="#e0764f" stroke-width="14"/>
<path d="M240 510 L150 540 L240 555Z" fill="#e0764f" stroke-width="11"/>
{hl("M260 500 C265 450 310 430 360 440",12,0.35)}
<circle cx="290" cy="480" r="20" fill="#2e211b"/><circle cx="283" cy="473" r="6" fill="#ffffff" stroke="none"/>
<path d="M330 610 q-10 40 -40 70 M360 612 q0 45 -20 80" fill="none" stroke-width="8"/>
''')

# ---------------- Soy sauce
save("Food","Soy sauce","beige",f'''
{sh(500,818,340,36)}
<rect x="370" y="205" width="130" height="70" rx="14" fill="#c8574b" stroke-width="14"/>
{hl("M390 225 L390 255",10,0.35)}
<path d="M378 275 L378 330 Q280 390 280 480 L280 780 Q280 810 310 810 L560 810 Q590 810 590 780 L590 480 Q590 390 492 330 L492 275Z" fill="#3b2a22" stroke-width="16"/>
<path d="M395 290 L395 340 Q310 395 305 480 L305 560" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.25"/>
<rect x="300" y="540" width="270" height="180" rx="12" fill="#f6ecd8" stroke-width="12"/>
<rect x="300" y="540" width="270" height="36" fill="#c8574b" stroke-width="10"/>
<ellipse cx="435" cy="650" rx="52" ry="38" fill="#9a6a45" stroke-width="9"/><path d="M435 612 Q420 650 435 688" fill="none" stroke-width="6" opacity="0.5"/>
{hl("M320 600 L320 700",12,0.4)}
{sh(700,800,130,18)}
<path d="M580 740 Q590 810 700 815 Q810 810 820 740Z" fill="#f6ecd8" stroke-width="14"/>
<path d="M592 770 Q700 790 808 770" fill="none" stroke="#5d82a8" stroke-width="9"/>
<ellipse cx="700" cy="740" rx="120" ry="30" fill="#f6ecd8" stroke-width="13"/>
<ellipse cx="700" cy="743" rx="98" ry="20" fill="#5a3526" stroke-width="7"/>
<ellipse cx="675" cy="738" rx="34" ry="6" fill="#ffffff" opacity="0.35" stroke="none"/>
''')

# ---------------- Spicy sour soup (tom yum pot)
def mush_half(x,y,s=1): return f'<g transform="translate({x} {y}) scale({s})"><path d="M-30 0 Q-30 -34 0 -36 Q30 -34 30 0Z" fill="#c9a27a" stroke-width="{9/s:.0f}"/><path d="M-14 0 L-12 22 L12 22 L14 0Z" fill="#f3e6d6" stroke-width="{8/s:.0f}"/></g>'
save("Food","Spicy sour soup","beige",f'''
{sh(512,808,330,36)}
{steam([420,512,604],300)}
<path d="M240 500 C170 490 170 590 250 580" fill="none" stroke-width="36"/><path d="M240 500 C170 490 170 590 250 580" fill="none" stroke="#a8b2b8" stroke-width="16"/>
<path d="M784 500 C854 490 854 590 774 580" fill="none" stroke-width="36"/><path d="M784 500 C854 490 854 590 774 580" fill="none" stroke="#a8b2b8" stroke-width="16"/>
<path d="M395 760 L380 805 L644 805 L629 760Z" fill="#8e9aa2" stroke-width="12"/>
<path d="M232 470 L258 720 Q270 775 512 778 Q754 775 766 720 L792 470Z" fill="#c3ccd2" stroke-width="16"/>
{hl("M275 510 L295 720",22,0.45)}
{dk("M740 510 L722 720",22,0.12)}
<path d="M248 610 Q512 650 776 610" fill="none" stroke-width="8" opacity="0.2"/>
<ellipse cx="512" cy="470" rx="282" ry="62" fill="#aeb8bf" stroke-width="16"/>
<ellipse cx="512" cy="476" rx="250" ry="45" fill="#e0763f" stroke-width="8"/>
<ellipse cx="420" cy="468" rx="70" ry="10" fill="#e0b04f" opacity="0.6" stroke="none"/>
{tube("M330 470 L450 370",14,"#d7df9a",8)}
{shrimp_small(560,462,-5,0.65)}{shrimp_small(680,472,25,0.5)}
{mush_half(420,488,0.9)}{mush_half(630,500,0.8)}
{leaf(480,500,-15,0.55,"#4f7a40")}{leaf(330,490,15,0.5,"#4f7a40")}{leaf(700,450,200,0.45,"#4f7a40")}
<circle cx="520" cy="498" r="12" fill="#c8574b" stroke-width="7"/><circle cx="370" cy="470" r="11" fill="#c8574b" stroke-width="7"/><circle cx="740" cy="478" r="10" fill="#c8574b" stroke-width="7"/>
{lime_wedge(215,790,-15,0.7)}
{chili_small(760,780,-20,0.85)}
''')

# ---------------- Squid
def tentacle(x0,y0,dx,curl,w):
    d=f"M{x0} {y0} C{x0+dx*0.2} {y0+120} {x0+dx} {y0+180} {x0+dx*1.1} {y0+250} q{curl} 40 {curl*1.6} 0"
    return tube(d,w,"#f0a07a",11)
tents=''.join(tentacle(x,600,dx,c,w) for x,dx,c,w in [(430,-160,-40,26),(460,-90,-30,26),(490,-30,30,28),(530,30,-30,28),(560,90,35,26),(590,160,40,26)])
save("Food","Squid","sky",f'''
{sh(512,860,260,26)}
{tube("M470 600 C400 720 320 760 250 860",20,"#f0a07a",11)}{tube("M550 600 C620 720 700 760 780 850",20,"#f0a07a",11)}
<ellipse cx="250" cy="860" rx="24" ry="16" fill="#f0a07a" stroke-width="10"/><ellipse cx="780" cy="850" rx="24" ry="16" fill="#f0a07a" stroke-width="10"/>
{tents}
<path d="M512 150 C440 220 380 300 360 380 L300 470 L390 460 Q400 520 420 560 L604 560 Q624 520 634 460 L724 470 L664 380 C644 300 584 220 512 150Z" fill="#ec9270" stroke-width="16"/>
<path d="M512 175 C460 240 420 320 410 420 Q412 500 430 550" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.3"/>
<path d="M600 300 C630 360 640 440 620 540 L600 552 C615 460 612 380 600 300Z" fill="#2e211b" opacity="0.15" stroke="none"/>
{dots([(470,330),(540,300),(500,400),(560,380),(470,470),(580,470),(520,240)],8,"#c96a50",0.7)}
<path d="M410 550 Q400 640 512 650 Q624 640 614 550Z" fill="#f0a07a" stroke-width="15"/>
<circle cx="460" cy="590" r="26" fill="#ffffff" stroke-width="10"/><circle cx="564" cy="590" r="26" fill="#ffffff" stroke-width="10"/>
<circle cx="465" cy="595" r="12" fill="#2e211b"/><circle cx="559" cy="595" r="12" fill="#2e211b"/>
<ellipse cx="512" cy="628" rx="16" ry="8" fill="#e8907f" opacity="0.8" stroke="none"/>
''')

# ---------------- Stir fried basil (kra pao + fried egg on rice)
porkbits=dots(rnd_in_ellipse(620,560,110,55,45),9,"#6b3e28",1,5)
bsave("Stir fried basil","beige",f'''
{plate(512,610,360,170,band="#5d82a8")}
<path d="M240 615 Q245 410 420 395 Q585 405 592 600 Q420 665 240 615Z" fill="#fbf6ea" stroke-width="14"/>
{''.join(f'<ellipse cx="{x:.0f}" cy="{y:.0f}" rx="12" ry="6" transform="rotate({random.randint(-40,40)} {x:.0f} {y:.0f})" fill="none" stroke-width="5" opacity="0.3"/>' for x,y in rnd_in_ellipse(400,560,130,60,22))}
<path d="M500 580 Q520 490 620 480 Q740 480 760 580 Q700 650 600 650 Q520 640 500 580Z" fill="#8a5a3f" stroke-width="14"/>
{porkbits}
{leaf(560,540,-30,0.7)}{leaf(660,520,10,0.75)}{leaf(700,590,-60,0.6)}{leaf(600,610,20,0.6,"#4f7a40")}
<g fill="#c8574b" stroke-width="7"><rect x="630" y="560" width="26" height="14" rx="6"/><rect x="560" y="590" width="24" height="14" rx="6" transform="rotate(30 572 597)"/><rect x="710" y="545" width="24" height="12" rx="6"/></g>
<g transform="translate(395 440) scale(0.72) translate(-410 -480)"><path d="M290 520 C260 460 320 400 390 420 C430 380 520 400 530 450 C580 470 560 540 500 550 C460 580 380 580 340 560 C290 570 270 540 290 520Z" fill="#c98a3b" stroke-width="13"/>
<path d="M310 515 C290 470 340 425 395 440 C430 410 505 425 510 460 C545 480 535 530 490 535 C455 555 385 558 350 545 C310 550 300 530 310 515Z" fill="#fbf6ea" stroke="none"/>
<circle cx="420" cy="485" r="42" fill="#f0a93c" stroke-width="11"/>
<ellipse cx="408" cy="472" rx="14" ry="9" fill="#ffffff" opacity="0.55" stroke="none"/></g>
<path d="M760 700 L870 780" stroke-width="30"/><path d="M760 700 L870 780" stroke="#b9b2a6" stroke-width="14"/>
''')

# ---------------- Stir fried mixed vegetables
def broc(x,y,s=1): return f'<g transform="translate({x} {y}) scale({s})"><path d="M-14 10 L-18 60 L18 60 L14 10Z" fill="#a9c47a" stroke-width="{9/s:.0f}"/><circle cx="-26" cy="0" r="28" fill="#5f8f4e" stroke-width="{9/s:.0f}"/><circle cx="24" cy="-2" r="30" fill="#5f8f4e" stroke-width="{9/s:.0f}"/><circle cx="0" cy="-26" r="32" fill="#5f8f4e" stroke-width="{9/s:.0f}"/><circle cx="-6" cy="-34" r="10" fill="#ffffff" opacity="0.25" stroke="none"/></g>'
def carrot(x,y,s=1): return f'<g transform="translate({x} {y}) scale({s})"><ellipse cx="0" cy="0" rx="34" ry="24" fill="#e0904f" stroke-width="{9/s:.0f}"/><ellipse cx="0" cy="-2" rx="16" ry="10" fill="none" stroke="#f3c08a" stroke-width="{6/s:.0f}"/></g>'
def corn(x,y,r): return f'<g transform="rotate({r} {x} {y})"><path d="M{x-50} {y} Q{x-50} {y-18} {x-30} {y-18} L{x+50} {y-6} Q{x+58} {y} {x+50} {y+6} L{x-30} {y+18} Q{x-50} {y+18} {x-50} {y}Z" fill="#e8c65a" stroke-width="9"/>{dots([(x-25,y-6),(x-25,y+6),(x,y-5),(x,y+5),(x+25,y-3),(x+25,y+3)],4,"#c9a03a")}</g>'
def pea(x,y,r): return f'<g transform="rotate({r} {x} {y})"><path d="M{x-55} {y} Q{x} {y-40} {x+55} {y} Q{x} {y+22} {x-55} {y}Z" fill="#8fbf5a" stroke-width="9"/><path d="M{x-35} {y-3} Q{x} {y-18} {x+35} {y-3}" fill="none" stroke="#ffffff" stroke-width="6" opacity="0.35"/></g>'
def mush_slice(x,y,s=1): return f'<g transform="translate({x} {y}) scale({s})"><path d="M-32 0 Q-32 -30 0 -32 Q32 -30 32 0 L12 0 L12 26 L-12 26 L-12 0Z" fill="#e8d2b0" stroke-width="{9/s:.0f}"/><path d="M-32 0 Q-32 -30 0 -32 Q32 -30 32 0Z" fill="#b58a62" stroke-width="{9/s:.0f}"/></g>'
bsave("Stir fried mixed vegetables","sage",f'''
{plate(512,620,360,168,band="#4f9a9a")}
<path d="M270 620 Q280 440 512 420 Q744 440 754 620 Q512 690 270 620Z" fill="#b87a45" stroke-width="12" opacity="0.9"/>
<path d="M310 600 Q330 520 512 500 Q690 520 715 600" fill="none" stroke="#ffffff" stroke-width="10" opacity="0.25"/>
<path d="M330 560 Q380 520 430 560 Q400 600 350 600Z" fill="#dfe8b8" stroke-width="10"/>
<path d="M600 610 Q650 570 700 600 Q680 640 620 640Z" fill="#dfe8b8" stroke-width="10"/>
{corn(430,610,-20)}{corn(620,520,15)}
{pea(560,590,-15)}{pea(360,530,30)}
{mush_slice(500,540,0.9)}{mush_slice(690,570,0.8)}
{broc(430,470,1)}{broc(590,440,0.95)}{broc(530,620,0.85)}
{carrot(340,600,1)}{carrot(470,580,0.9)}{carrot(660,630,0.95)}{carrot(720,530,0.85)}
<g fill="#c8574b" stroke-width="8"><rect x="385" y="555" width="40" height="18" rx="7" transform="rotate(-20 405 564)"/><rect x="590" y="555" width="40" height="18" rx="7" transform="rotate(25 610 564)"/></g>

''')

# ---------------- Pad thai
nd=''
for x,y,dx in [(360,470,70),(430,450,80),(520,470,60),(300,590,70),(340,560,80),(380,600,70),(420,540,90),(460,580,80),(500,520,70),(520,610,80),(560,560,70),(600,590,60),(360,520,60),(470,500,60),(620,540,50)]:
    nd+=tube(f"M{x} {y} q{dx/2:.0f} -34 {dx} 0 t{dx} 0",16,"#e6a45e",5)
sprouts=''.join(f'<path d="M{x} {y} q20 -18 45 -12" fill="none" stroke-width="16"/><path d="M{x} {y} q20 -18 45 -12" fill="none" stroke="#f6ecd8" stroke-width="7"/><circle cx="{x+46}" cy="{y-12}" r="7" fill="#e0c96a" stroke-width="5"/>' for x,y in [(330,640),(620,630),(560,500),(420,480)])
nuts=''.join(f'<path d="M{x-12} {y+6} L{x-6} {y-10} L{x+10} {y-8} L{x+12} {y+8}Z" fill="#d9a86a" stroke-width="6" transform="rotate({(x*7)%60-30} {x} {y})"/>' for x,y in [(700,560),(730,585),(760,560),(715,615),(750,612),(785,590),(735,540),(690,595),(770,630),(795,560)])
bsave("Stir fried noodles with peanuts","sand",f'''
{plate(512,620,370,170,col="#f6ecd8",inner="#ece0c8",band="#5d82a8")}
<path d="M270 620 Q280 440 480 420 Q665 430 690 620 Q480 670 270 620Z" fill="#d48e4a" stroke-width="12"/>
{nd}
<g fill="#f0c649" stroke-width="7"><rect x="400" y="570" width="30" height="22" rx="7"/><rect x="530" y="545" width="28" height="20" rx="7"/><rect x="340" y="590" width="26" height="20" rx="7"/></g>
{sprouts}
{tube("M300 560 L400 530",8,"#6f9a4a",6)}{tube("M530 590 L640 570",8,"#6f9a4a",6)}
{shrimp_small(450,520,-10,0.7)}{shrimp_small(560,610,10,0.62)}
{nuts}
{lime_wedge(330,690,-10,0.75)}
{dots([(480,580),(380,560),(600,540)],4,"#c8574b")}
''')

# ---------------- Sugar (bowl + cubes)
save("Food","Sugar","rose",f'''
{sh(480,815,330,36)}
<path d="M300 560 Q340 440 470 430 Q600 440 640 560Z" fill="#ffffff" stroke-width="14"/>
{dots(rnd_in_ellipse(470,520,120,30,36),4,"#d8d0c4",0.9)}
<path d="M560 540 L700 330" stroke-width="30"/><path d="M560 540 L700 330" stroke="#b9b2a6" stroke-width="14"/>
<path d="M240 560 Q250 790 470 800 Q690 790 700 560Z" fill="#8a5a86" stroke-width="16"/>
<path d="M254 620 Q470 660 686 620" fill="none" stroke="#f6ecd8" stroke-width="14"/>
{hl("M280 620 Q300 730 380 770",18,0.3)}
<path d="M240 560 Q470 600 700 560" fill="none" stroke-width="14"/>
{cube(700,660,80)}{cube(790,690,80)}{cube(745,580,80)}
''')

# ---------------- Tomato
save("Food","Tomato","sage",f'''
{sh(512,810,280,40)}
<path d="M512 330 C640 300 790 390 790 560 C790 720 660 800 512 800 C364 800 234 720 234 560 C234 390 384 300 512 330Z" fill="#cf5a47" stroke-width="16"/>
<path d="M650 360 C740 420 770 520 750 620 C720 730 630 780 540 790 C660 720 710 560 650 360Z" fill="#2e211b" opacity="0.15" stroke="none"/>
<path d="M430 360 Q400 560 450 780" fill="none" stroke-width="8" opacity="0.15"/><path d="M600 360 Q640 560 580 780" fill="none" stroke-width="8" opacity="0.15"/>
{hl("M300 520 C305 440 350 390 420 370",30,0.35)}
<ellipse cx="330" cy="600" rx="18" ry="26" fill="#ffffff" opacity="0.3" stroke="none"/>
<path d="M512 350 L420 320 L480 300 L450 250 L512 290 L574 250 L544 300 L604 320Z" fill="#5f8f4e" stroke-width="12"/>
<path d="M512 300 Q515 240 545 210" fill="none" stroke-width="30"/><path d="M512 300 Q515 240 545 210" fill="none" stroke="#5f8f4e" stroke-width="14"/>
''')

# ---------------- Vegetable (basket)
save("Food","Vegetable","sage",f'''
{sh(512,820,320,36)}
<g transform="rotate(-25 380 390)"><path d="M350 540 L380 250 L410 540Z" fill="#e0904f" stroke-width="13"/><path d="M365 400 L385 395 M370 460 L392 455" stroke-width="6" opacity="0.4"/><path d="M380 255 Q360 200 340 170 M380 255 Q385 190 400 160 M380 255 Q410 205 440 185" fill="none" stroke-width="22"/><path d="M380 255 Q360 200 340 170 M380 255 Q385 190 400 160 M380 255 Q410 205 440 185" fill="none" stroke="#7fa05a" stroke-width="10"/></g>
<g transform="rotate(30 640 420)"><path d="M640 320 C700 330 720 440 700 520 C690 560 590 560 580 520 C560 440 580 330 640 320Z" fill="#8a5a86" stroke-width="14"/>{hl("M605 380 C595 430 600 480 610 510",12,0.3)}<path d="M600 330 Q640 290 680 330 Q640 350 600 330Z" fill="#5f8f4e" stroke-width="10"/></g>
<circle cx="512" cy="470" r="120" fill="#b9d58a" stroke-width="14"/>
<path d="M430 420 Q512 360 594 420 M420 500 Q512 430 604 500" fill="none" stroke-width="8" opacity="0.35"/>
{hl("M430 400 Q470 370 520 370",14,0.4)}
{broc(700,520,1.3)}
<circle cx="340" cy="530" r="60" fill="#cf5a47" stroke-width="13"/><path d="M340 475 L310 465 L330 455 L340 440 L350 455 L370 465Z" fill="#5f8f4e" stroke-width="8"/>
{hl("M305 520 Q310 495 330 485",10,0.35)}
<path d="M230 560 L794 560 L740 800 Q735 815 715 815 L309 815 Q289 815 284 800Z" fill="#c98a4b" stroke-width="16"/>
<g stroke-width="7" opacity="0.4" fill="none"><path d="M250 620 L776 620"/><path d="M262 680 L762 680"/><path d="M275 740 L750 740"/>{''.join(f'<path d="M{x} 565 L{x+(512-x)*0.12:.0f} 810"/>' for x in range(300,760,70))}</g>
<rect x="215" y="545" width="594" height="42" rx="21" fill="#b5763d" stroke-width="15"/>
{hl("M260 560 L760 560",8,0.3)}
''')

# ---------------- Vinegar
save("Food","Vinegar","beige",f'''
{sh(460,815,300,36)}
<rect x="400" y="210" width="80" height="60" rx="12" fill="#c49a6c" stroke-width="13"/>
<path d="M410 225 L470 225" stroke-width="6" opacity="0.4"/>
<path d="M405 270 L405 380 C300 420 260 500 260 600 C260 720 350 800 440 800 C530 800 620 720 620 600 C620 500 580 420 475 380 L475 270Z" fill="#eef4f7" stroke-width="16"/>
<path d="M268 560 C265 580 262 590 262 600 C262 720 350 796 440 796 C530 796 618 720 618 600 C618 590 615 575 612 560 Q440 590 268 560Z" fill="#ebc56e" stroke="none"/>
<path d="M268 560 Q440 590 612 560" fill="none" stroke-width="7" opacity="0.3"/>
<path d="M405 270 L405 380 C300 420 260 500 260 600 C260 720 350 800 440 800 C530 800 620 720 620 600 C620 500 580 420 475 380 L475 270Z" fill="none" stroke-width="16"/>
{hl("M300 520 C285 560 285 640 310 690",22,0.6)}
{hl("M420 290 L420 370",10,0.6)}
{dk("M580 620 C575 700 530 750 470 770",18,0.12)}
{sh(720,805,120,16)}
<path d="M610 745 Q620 810 720 812 Q820 810 830 745Z" fill="#f6ecd8" stroke-width="14"/>
<path d="M620 772 Q720 790 820 772" fill="none" stroke="#5d82a8" stroke-width="9"/>
<ellipse cx="720" cy="745" rx="112" ry="28" fill="#f6ecd8" stroke-width="13"/>
<ellipse cx="720" cy="748" rx="92" ry="18" fill="#efd792" stroke-width="7"/>
{slices([(690,746),(735,742),(765,750)],"#7fa05a")}
''')

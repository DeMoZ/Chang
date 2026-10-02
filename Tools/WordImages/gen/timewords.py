import math
from common import *
DK="#2e211b"; CREAM="#f6ecd8"; PAPER="#fbf6ea"
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="{DK}" opacity="0.15" stroke="none"/>'
def T(x,y,s,size,fill=CREAM,sw=None,anchor="middle"):
    sw=sw or max(6,size//10)
    a=f'x="{x}" y="{y}" text-anchor="{anchor}" font-family="Arial, Helvetica, sans-serif" font-weight="bold" font-size="{size}"'
    return f'<text {a} fill="{DK}" stroke="{DK}" stroke-width="{2*sw}" stroke-linejoin="round">{s}</text><text {a} fill="{fill}" stroke="none">{s}</text>'
def ring(x,y0=190,h=100): return f'<rect x="{x-15}" y="{y0}" width="30" height="{h}" rx="15" fill="#8e8e8e" stroke-width="12"/>'
def mix(c,t):  # mix color with white, t = share of white
    r,g,b=int(c[1:3],16),int(c[3:5],16),int(c[5:7],16)
    return "#%02x%02x%02x"%tuple(int(v+(255-v)*t) for v in (r,g,b))
def hdr_path(x,y,w,h,r=40): return f'M{x} {y+h} L{x} {y+r} Q{x} {y} {x+r} {y} L{x+w-r} {y} Q{x+w} {y} {x+w} {y+r} L{x+w} {y+h}Z'
def page(x,y,w,h,hh,hcol,body="",bfill=PAPER,rings=(),cid="pg",sw=16,r=40):
    """calendar page: shadow, paper, clipped body content, header band, rings"""
    return f'''<defs><clipPath id="{cid}"><rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}"/></clipPath></defs>
<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{DK}" opacity="0.18" stroke="none" transform="translate(14 12)"/>
<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{bfill}" stroke="none"/>
<g clip-path="url(#{cid})">{body}</g>
<path d="{hdr_path(x,y,w,hh,r)}" fill="{hcol}" stroke-width="{sw}"/>
<path d="M{x+40} {y+38} L{x+w-40} {y+38}" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.3"/>
<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="none" stroke-width="{sw}"/>
{''.join(ring(rx,y-40,100) for rx in rings)}'''

# ---------- shared motifs ----------
def cloud(x,y,s=1,col="#dfe6ea",sw=12):
    return f'<path transform="translate({x} {y}) scale({s})" d="M-120 40 Q-160 40 -160 5 Q-160 -35 -115 -35 Q-110 -90 -50 -90 Q-10 -130 45 -95 Q110 -110 120 -45 Q170 -40 165 5 Q162 40 120 40Z" fill="{col}" stroke-width="{sw/s:.0f}"/>'
def drop(x,y,s=1,col="#5d82a8"):
    return f'<path transform="translate({x} {y}) scale({s})" d="M0 -30 Q20 -2 20 10 Q20 28 0 28 Q-20 28 -20 10 Q-20 -2 0 -30Z" fill="{col}" stroke-width="{7/s:.0f}"/>'
def rainlines(pts,col="#5d82a8"):
    return ''.join(f'<path d="M{x} {y} l-14 40" fill="none" stroke="{col}" stroke-width="10"/>' for x,y in pts)
def sun(x,y,r,rays=12,col="#e0b04f"):
    rs=''.join(f'<path d="M{x+(r+18)*math.cos(2*math.pi*i/rays):.0f} {y+(r+18)*math.sin(2*math.pi*i/rays):.0f} L{x+(r+55)*math.cos(2*math.pi*i/rays):.0f} {y+(r+55)*math.sin(2*math.pi*i/rays):.0f}" fill="none" stroke="{col}" stroke-width="14"/>' for i in range(rays))
    return rs+f'<circle cx="{x}" cy="{y}" r="{r}" fill="{col}" stroke-width="12"/><circle cx="{x-r*0.35:.0f}" cy="{y-r*0.35:.0f}" r="{r*0.28:.0f}" fill="#ffffff" opacity="0.35" stroke="none"/>'
def burst(x,y,r,col,n=12):
    out=[]
    for i in range(n):
        a=2*math.pi*i/n
        out.append(f'<path d="M{x+r*0.3*math.cos(a):.0f} {y+r*0.3*math.sin(a):.0f} L{x+r*math.cos(a):.0f} {y+r*math.sin(a):.0f}" fill="none" stroke="{col}" stroke-width="12"/>')
        out.append(f'<circle cx="{x+(r+16)*math.cos(a):.0f}" cy="{y+(r+16)*math.sin(a):.0f}" r="7" fill="{col}" stroke="none"/>')
    return ''.join(out)+f'<circle cx="{x}" cy="{y}" r="{r*0.15:.0f}" fill="{col}" stroke="none"/>'
def star(x,y,r,col="#e0b04f",sw=10,rot=-90):
    pts=[]
    for i in range(10):
        a=math.radians(rot+36*i); rr=r if i%2==0 else r*0.45
        pts.append(f'{x+rr*math.cos(a):.0f} {y+rr*math.sin(a):.0f}')
    return f'<path d="M{" L".join(pts)}Z" fill="{col}" stroke-width="{sw}"/>'
def tree(x,y,s=1):
    """christmas tree, (x,y) = bottom centre of trunk"""
    balls=''.join(f'<circle cx="{a}" cy="{b}" r="16" fill="{c}" stroke-width="7"/>' for a,b,c in [(-60,-140,"#c8574b"),(55,-120,"#e0b04f"),(-30,-250,"#5d82a8"),(40,-300,"#c8574b"),(-70,-60,"#e0b04f"),(75,-40,"#5d82a8"),(0,-190,"#e0b04f"),(10,-80,"#c8574b")])
    return f'''<g transform="translate({x} {y}) scale({s})">
<rect x="-30" y="-40" width="60" height="60" fill="#9a6a45" stroke-width="{12/s:.0f}"/>
<path d="M0 -440 L-110 -260 L-60 -260 L-160 -110 L-100 -110 L-190 0 L190 0 L100 -110 L160 -110 L60 -260 L110 -260Z" fill="#5f8f4e" stroke-width="{14/s:.0f}"/>
<path d="M-10 -400 L-80 -280 M-30 -240 L-120 -120 M-50 -100 L-150 -15" fill="none" stroke="#ffffff" stroke-width="{12/s:.0f}" opacity="0.3"/>
<path d="M-130 -60 Q0 -10 140 -80 M-100 -180 Q0 -140 110 -210" fill="none" stroke="#e0b04f" stroke-width="{10/s:.0f}"/>
{balls}
{star(0,-445,55,"#e0b04f",10/s)}
</g>'''
def gift(x,y,w,h,col,rib="#e0b04f"):
    return f'''<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="8" fill="{col}" stroke-width="12"/>
<rect x="{x-10}" y="{y}" width="{w+20}" height="{h*0.28:.0f}" rx="8" fill="{col}" stroke-width="12"/>
<rect x="{x+w/2-12:.0f}" y="{y}" width="24" height="{h}" fill="{rib}" stroke-width="8"/>
<path d="M{x+w/2:.0f} {y} Q{x+w/2-60:.0f} {y-60} {x+w/2-20:.0f} {y-50} Q{x+w/2:.0f} {y-30} {x+w/2:.0f} {y} Q{x+w/2:.0f} {y-30} {x+w/2+20:.0f} {y-50} Q{x+w/2+60:.0f} {y-60} {x+w/2:.0f} {y}Z" fill="{rib}" stroke-width="9"/>'''
def jasmine(x,y,s=1):
    pet=''.join(f'<ellipse cx="0" cy="-38" rx="22" ry="38" transform="rotate({i*72})" fill="#fbf6ea" stroke-width="{8/s:.0f}"/>' for i in range(5))
    return f'<g transform="translate({x} {y}) scale({s})">{pet}<circle r="14" fill="#e0b04f" stroke-width="{7/s:.0f}"/></g>'
def lotus_flower(x,y,s=1,col="#e48aa6"):
    return f'''<g transform="translate({x} {y}) scale({s})">
<path d="M0 0 Q-70 -10 -80 -60 Q-40 -50 0 0Z M0 0 Q70 -10 80 -60 Q40 -50 0 0Z" fill="{mix(col,0.2)}" stroke-width="{9/s:.0f}"/>
<path d="M0 0 Q-50 -40 -35 -100 Q-5 -60 0 0Z M0 0 Q50 -40 35 -100 Q5 -60 0 0Z" fill="{col}" stroke-width="{9/s:.0f}"/>
<path d="M0 0 Q-30 -60 0 -120 Q30 -60 0 0Z" fill="{mix(col,0.15)}" stroke-width="{9/s:.0f}"/></g>'''
def krathong(x,y,s=1):
    leaves=''.join(f'<path d="M{a} -40 L{a+22} -95 L{a+44} -40Z" fill="#7fa05a" stroke-width="{8/s:.0f}"/>' for a in range(-160,120,46))
    return f'''<g transform="translate({x} {y}) scale({s})">
<path d="M-190 -40 L190 -40 Q180 20 120 30 L-120 30 Q-180 20 -190 -40Z" fill="#5f8f4e" stroke-width="{12/s:.0f}"/>
{leaves}
{lotus_flower(-70,-60,0.8)}{lotus_flower(70,-60,0.8,"#e0b04f")}
{lotus_flower(0,-50,1.0)}
<rect x="-12" y="-230" width="24" height="90" rx="6" fill="#f6ecd8" stroke-width="{9/s:.0f}"/>
<path d="M0 -236 Q-22 -262 0 -300 Q22 -262 0 -236Z" fill="#e0b04f" stroke-width="{8/s:.0f}"/>
<path d="M-60 -150 L-60 -200 M60 -150 L60 -195" fill="none" stroke-width="{7/s:.0f}"/>
<circle cx="-60" cy="-206" r="7" fill="#c8574b" stroke="none"/><circle cx="60" cy="-201" r="7" fill="#c8574b" stroke="none"/>
</g>'''
def bowl(x,y,s=1):
    """silver khan bowl with water and floating flowers, (x,y)= centre of rim"""
    return f'''<g transform="translate({x} {y}) scale({s})">
<path d="M-170 0 Q-160 150 0 160 Q160 150 170 0Z" fill="#c9ccd1" stroke-width="{14/s:.0f}"/>
<path d="M-130 40 Q-110 120 -30 138" fill="none" stroke="#ffffff" stroke-width="{16/s:.0f}" opacity="0.6"/>
<path d="M-120 60 Q0 90 120 60 M-90 110 Q0 135 90 110" fill="none" stroke-width="{7/s:.0f}" opacity="0.35"/>
<rect x="-60" y="150" width="120" height="30" rx="8" fill="#b3b7bd" stroke-width="{12/s:.0f}"/>
<ellipse cx="0" cy="0" rx="170" ry="40" fill="#b3b7bd" stroke-width="{14/s:.0f}"/>
<ellipse cx="0" cy="4" rx="145" ry="28" fill="#8fc3d6" stroke-width="{8/s:.0f}"/>
{jasmine(-60,0,0.45)}{jasmine(40,6,0.45)}{jasmine(-5,-8,0.4)}
</g>'''

# ---------- MONTHS ----------
COOL,HOT,RAIN="#5d82a8","#c8574b","#4f9a9a"
PX,PY,PW,PH,HH=210,230,604,600,200
def month(key,n,hcol,bgpair,scene,bfill):
    body=f'<rect x="{PX}" y="{PY}" width="{PW}" height="{PH}" fill="{bfill}" stroke="none"/>'+scene
    save("Months",key,bgpair,f'''
{sh(512,860,330,30)}
{page(PX,PY,PW,PH,HH,hcol,body,bfill,rings=(300,724))}
{T(512,PY+172,n,190,CREAM,10)}
''')
BY=PY+HH  # 430: top of scene area; scene area x210-814, y430-830
# 1 January: new year fireworks at night
month("January","1",COOL,"sky",f'''
{burst(380,580,95,"#e0b04f")}{burst(640,540,80,"#e48aa6")}{burst(560,720,60,"#8fc3d6",10)}
{star(290,730,16,"#f6ecd8",6)}{star(720,700,14,"#f6ecd8",6)}{star(470,480,12,"#f6ecd8",6)}{star(760,470,12,"#f6ecd8",6)}
''',"#3d4f73")
# 2 February: kite in breezy cool sky
tail="M600 640 Q560 720 500 700 Q440 680 400 760 Q370 810 320 800"
month("February","2",COOL,"sage",f'''
{cloud(320,540,0.6,"#ffffff")}{cloud(730,760,0.5,"#ffffff")}
<path d="{tail}" fill="none" stroke-width="7"/>
<path d="M510 690 l-20 -20 l0 40z M420 725 l-20 -20 l0 40z" fill="#c8574b" stroke-width="6"/>
<path d="M600 470 L690 560 L600 650 L510 560Z" fill="#e0b04f" stroke-width="14"/>
<path d="M600 470 L600 650 M510 560 L690 560" fill="none" stroke-width="8"/>
<path d="M600 470 L690 560 L600 560Z M600 560 L510 560 L600 650Z" fill="#c8574b" stroke="none" opacity="0.8"/>
<path d="M600 470 L690 560 L600 650 L510 560Z" fill="none" stroke-width="14"/>
<path d="M300 500 q30 -12 60 0 q30 12 60 0 M660 700 q30 -12 60 0 q30 12 60 0" fill="none" stroke="#ffffff" stroke-width="9"/>
''',"#d8e7f0")
# 3 March: hot sun, dry cracked ground
month("March","3",HOT,"sand",f'''
<rect x="200" y="720" width="620" height="120" fill="#c9955f" stroke-width="12"/>
<path d="M300 740 l30 30 l-20 30 M500 735 l-25 35 l30 25 l-10 30 M680 745 l30 25 l-15 40" fill="none" stroke-width="8" opacity="0.6"/>
{sun(512,570,85,12,"#e8a33f")}
<path d="M270 690 q20 -15 40 0 q20 15 40 0 M680 690 q20 -15 40 0 q20 15 40 0" fill="none" stroke="#e8a33f" stroke-width="9"/>
''',"#fbe3c0")
# 4 April: Songkran water splash from silver bowl
spl=''.join(drop(x,y,s,"#8fc3d6") for x,y,s in [(360,520,1.2),(430,470,1.0),(520,450,1.3),(610,475,1.0),(680,520,1.2),(300,600,0.9),(730,610,0.9),(470,560,0.8),(575,550,0.8)])
month("April","4",HOT,"sky",f'''
<path d="M420 700 Q380 600 330 560 M512 690 Q512 590 512 520 M600 700 Q640 600 700 560" fill="none" stroke="#8fc3d6" stroke-width="22"/>
{spl}
{bowl(512,700,0.9)}
''',"#fdf0d8")
# 5 May: first rain over rice paddy sprouts
spr=''.join(f'<path d="M{x} 790 Q{x-6} 760 {x-22} 740 M{x} 790 Q{x+4} 755 {x+20} 735 M{x} 790 L{x} 745" fill="none" stroke="#5f8f4e" stroke-width="9"/>' for x in range(270,800,75))
month("May","5",RAIN,"sage",f'''
<rect x="200" y="730" width="620" height="120" fill="#9fc0c9" stroke-width="12"/>
<path d="M260 790 L780 790" fill="none" stroke="#ffffff" stroke-width="8" opacity="0.4"/>
{spr}
{cloud(512,560,1.0,"#dfe6ea")}
{drop(430,660,0.8)}{drop(560,690,0.8)}{drop(640,650,0.8)}
{sun(700,490,40,10,"#e0b04f")}
{cloud(512,560,1.0,"#dfe6ea")}
''',"#e3eee8")
# 6 June: rain and umbrella
month("June","6",RAIN,"sky",f'''
{rainlines([(260,470),(330,540),(270,640),(740,470),(700,560),(770,640),(420,470),(620,470),(300,760),(740,750)])}
<path d="M512 560 L512 770 Q512 810 475 800" fill="none" stroke-width="14"/>
<path d="M312 590 Q320 460 512 450 Q704 460 712 590 Q680 560 646 590 Q612 560 579 590 Q545 560 512 590 Q479 560 445 590 Q412 560 378 590 Q345 560 312 590Z" fill="#c8574b" stroke-width="14"/>
<path d="M512 450 Q460 500 445 590 M512 450 Q564 500 579 590" fill="none" stroke-width="8"/>
<path d="M350 540 Q380 480 450 465" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.35"/>
''',"#dde8ee")
# 7 July: Khao Phansa candle festival, light rain
month("July","7",RAIN,"rose",f'''
{rainlines([(270,480),(320,580),(260,680),(750,480),(700,590),(760,690)])}
<path d="M390 800 L634 800 L600 740 L424 740Z" fill="#c9955f" stroke-width="12"/>
<rect x="455" y="520" width="114" height="220" rx="10" fill="#e8bf4a" stroke-width="14"/>
<path d="M470 560 q42 30 84 0 M470 620 q42 30 84 0 M470 680 q42 30 84 0" fill="none" stroke="#b8862a" stroke-width="9"/>
<path d="M480 540 L480 720" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.35"/>
<path d="M512 520 L512 500" fill="none" stroke-width="8"/>
<path d="M512 500 Q480 470 512 420 Q544 470 512 500Z" fill="#e8983f" stroke-width="10"/>
<path d="M512 492 Q498 474 512 450 Q526 474 512 492Z" fill="#f2d36a" stroke="none"/>
''',"#f3e3e0")
# 8 August: jasmine flowers (Mother's Day) with rain drops
month("August","8",RAIN,"sage",f'''
{drop(290,500,0.8)}{drop(740,520,0.8)}{drop(280,760,0.7)}{drop(750,770,0.7)}
<path d="M340 700 Q512 800 690 700" fill="none" stroke="#5f8f4e" stroke-width="10"/>
<path d="M420 700 Q400 650 440 620 Q470 660 420 700Z M610 700 Q640 650 600 620 Q570 660 610 700Z" fill="#5f8f4e" stroke-width="9"/>
{jasmine(512,590,1.3)}{jasmine(390,660,0.9)}{jasmine(634,660,0.9)}{jasmine(455,510,0.6)}{jasmine(575,505,0.6)}
''',"#e9f0e4")
# 9 September: heavy rain, rubber boots in a puddle
def boot(x,col):
    return f'''<path d="M{x} 560 L{x+80} 560 L{x+80} 720 L{x+150} 735 Q{x+170} 745 {x+165} 780 L{x} 780Z" fill="{col}" stroke-width="14"/>
<rect x="{x-8}" y="545" width="96" height="36" rx="10" fill="{mix(col,0.25)}" stroke-width="12"/>
<path d="M{x+18} 600 L{x+18} 740" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.35"/>'''
month("September","9",RAIN,"sand",f'''
{rainlines([(250,460),(330,470),(420,450),(510,470),(600,455),(690,470),(770,460),(280,530),(740,530),(460,510),(630,515)])}
<ellipse cx="512" cy="785" rx="260" ry="40" fill="#8fc3d6" stroke-width="12"/>
{boot(330,"#e0b04f")}{boot(520,"#e0b04f")}
''',"#e2eaee")
# 10 October: end of rain, rainbow
rb=''.join(f'<path d="M{270+i*28} 760 A{242-i*28} {242-i*28} 0 0 1 {754-i*28} 760" fill="none" stroke="{c}" stroke-width="28"/>' for i,c in enumerate(["#c8574b","#e8983f","#e0b04f","#7fa05a","#5d82a8"]))
month("October","10",RAIN,"rose",f'''
<path d="M256 760 A256 256 0 0 1 768 760" fill="none" stroke-width="10"/>
{rb}
<path d="M398 760 A114 114 0 0 1 626 760" fill="none" stroke-width="10"/>
{cloud(300,760,0.7,"#ffffff")}{cloud(730,760,0.7,"#ffffff")}
{drop(512,700,0.6,"#8fc3d6")}
''',"#e5eef3")
# 11 November: Loy Krathong under full moon
month("November","11",COOL,"sky",f'''
<rect x="200" y="700" width="620" height="150" fill="#4f7fa0" stroke-width="12"/>
<path d="M260 760 q30 -14 60 0 M660 780 q30 -14 60 0 M300 810 q30 -14 60 0" fill="none" stroke="#ffffff" stroke-width="8" opacity="0.5"/>
<circle cx="700" cy="520" r="55" fill="#f6ecd8" stroke-width="12"/>
{star(300,510,14,"#f6ecd8",6)}{star(380,470,10,"#f6ecd8",5)}
<path d="M512 760 Q480 760 460 730" fill="none" stroke="#e0b04f" stroke-width="8" opacity="0.6"/>
{krathong(512,745,0.95)}
''',"#2f4466")
# 12 December: cool season, Christmas tree
month("December","12",COOL,"sage",f'''
{tree(512,805,0.78)}
{star(300,520,14,"#ffffff",6)}{star(730,540,14,"#ffffff",6)}{star(280,700,10,"#ffffff",5)}{star(750,700,10,"#ffffff",5)}
''',"#dbe7ef")

# ---------- WEEKDAYS ----------
DAYS=[("Monday","#e8c14a","sky"),("Tuesday","#e48aa6","sage"),("Wednesday","#6fa35a","rose"),("Thursday","#e27f3a","sky"),
      ("Friday","#6fa8d6","sand"),("Saturday","#8a5a9a","sage"),("Sunday","#c94a45","sky")]
COLS=[c for _,c,_ in DAYS]
def dots(sel,y,x0=272,dx=80,r=26,rbig=46):
    out=[]
    for i,c in enumerate(COLS):
        x=x0+i*dx
        if i==sel: continue
        out.append(f'<circle cx="{x}" cy="{y}" r="{r}" fill="{c}" stroke-width="9" opacity="0.4"/>')
    x=x0+sel*dx; c=COLS[sel]
    out.append(f'<circle cx="{x}" cy="{y}" r="{rbig+14}" fill="#ffffff" stroke-width="10"/><circle cx="{x}" cy="{y}" r="{rbig}" fill="{c}" stroke-width="10"/><circle cx="{x-14}" cy="{y-14}" r="12" fill="#ffffff" opacity="0.4" stroke="none"/>')
    return ''.join(out)
for i,(key,c,bgp) in enumerate(DAYS):
    x=272+i*80
    body=f'''<rect x="{PX}" y="{PY}" width="{PW}" height="{PH}" fill="{mix(c,0.8)}" stroke="none"/>
<path d="M{x-45} 530 L{x+45} 530 L{x+45} 580 L{x+80} 580 L{x} 660 L{x-80} 580 L{x-45} 580Z" fill="{c}" stroke-width="12"/>
<rect x="240" y="690" width="544" height="100" rx="50" fill="#ffffff" opacity="0.7" stroke-width="10"/>
{dots(i,740)}'''
    save("WeekDays",key,bgp,f'''
{sh(512,860,330,30)}
{page(PX,PY,PW,PH,230,c,body,mix(c,0.8),rings=(300,512,724))}
{sun(512,345,52,10,"#fbf6ea")}
''')

# relative days: 5 tiles, today in middle
def tile(x,y,w,h,hcol,fill,op=1,sw=12):
    return f'''<g opacity="{op}"><rect x="{x}" y="{y}" width="{w}" height="{h}" rx="16" fill="{fill}" stroke-width="{sw}"/>
<path d="{hdr_path(x,y,w,h*0.3,16)}" fill="{hcol}" stroke-width="{sw}"/>
<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="16" fill="none" stroke-width="{sw}"/>
<path d="M{x+w*0.3:.0f} {y-14} L{x+w*0.3:.0f} {y+18} M{x+w*0.7:.0f} {y-14} L{x+w*0.7:.0f} {y+18}" fill="none" stroke="#8e8e8e" stroke-width="10"/></g>'''
TEAL="#4f9a9a"; RED="#c8574b"
def relday(key,off,bgp):
    W_,G=112,34; y=500; h=190
    xs=[512-2*(W_+G)+k*(W_+G) for k in range(5)]  # centres
    out=[f'<path d="M{xs[0]-80} {y+h+30} L{xs[4]+80} {y+h+30}" fill="none" stroke-width="10" opacity="0.3"/>']
    tgt=2+off
    for k,cx in enumerate(xs):
        if k==tgt or k==2: continue
        out.append(tile(cx-W_/2,y,W_,h,"#b9a88f",PAPER,0.5))
    s_=1.22; tw,th=W_*s_,h*s_
    if off!=0:
        out.append(tile(xs[2]-W_/2,y,W_,h,TEAL,"#e3f0ef"))
        out.append(tile(xs[tgt]-tw/2,y+h-th,tw,th,RED,"#f6dcd6",1,14))
        x1=xs[2]; x2=xs[tgt]; top=y-30; ttop=y+h-th-30
        peak=ttop-150-50*(abs(off)-1)
        cx=(x1+x2)/2
        out.append(f'<path d="M{x1} {top} C{x1} {peak} {x2} {peak} {x2} {ttop-40:.0f}" fill="none" stroke="{RED}" stroke-width="20"/>')
        out.append(f'<path d="M{x2-34} {ttop-52:.0f} L{x2+34} {ttop-52:.0f} L{x2} {ttop:.0f}Z" fill="{RED}" stroke-width="10"/>')
        lbl=("+" if off>0 else "−")+str(abs(off))
        by=peak+ (top-peak)*0.25 - 60
        out.append(f'<circle cx="{cx:.0f}" cy="{by:.0f}" r="68" fill="{PAPER}" stroke-width="12"/>'+T(f"{cx:.0f}",f"{by+26:.0f}",lbl,76,RED,5))
    else:
        s_=1.4; tw,th=W_*s_,h*s_
        out.append(tile(xs[2]-tw/2,y+h-th,tw,th,TEAL,"#e3f0ef",1,14))
        out.append(sparkle(xs[2]-130,y-40,1.1)+sparkle(xs[2]+135,y-10,0.9)+sparkle(xs[2]+40,y-150,0.8))
    px=xs[2]
    out.append(f'<path d="M{px} {y+h+60} L{px-46} {y+h+140} L{px+46} {y+h+140}Z" fill="{TEAL}" stroke-width="12"/>')
    save("WeekDays",key,bgp,''.join(out))
relday("Today",0,"sage")
relday("Tomorrow",1,"sky")
relday("Yesterday",-1,"sand")
relday("The_day_after_t_1585425685",2,"rose")
relday("The_day_before__111228420",-2,"beige")

# strip of 7 tiles Mon..Sun with selection
def strip(sel,y=220,h=150,W_=84,G=12,fade=0.3):
    x0=512-(7*W_+6*G)/2; out=[]
    for i,c in enumerate(COLS):
        on = sel is None or i in sel
        out.append(tile(x0+i*(W_+G),y,W_,h,c,mix(c,0.75) if on else PAPER,1 if on else fade,10))
    return ''.join(out)
# Week: all 7 days bracketed, badge 7
save("WeekDays","Week","beige",f'''
{sh(512,700,330,26)}
{strip(None,460,220,80,12)}
<path d="M210 420 L210 390 L814 390 L814 420" fill="none" stroke-width="14"/>
<path d="M512 390 L512 350" fill="none" stroke-width="14"/>
<circle cx="512" cy="290" r="72" fill="{RED}" stroke-width="14"/>
{T(512,322,"7",96,CREAM,7)}
''')
# Weekend: Sat+Sun highlighted + deck chair under umbrella
def deckchair(x,y,s=1):
    return f'''<g transform="translate({x} {y}) scale({s})">
<path d="M-120 0 L40 -170 M60 0 L-60 -120 M120 0 L60 -60" fill="none" stroke="#9a6a45" stroke-width="{18/s:.0f}"/>
<path d="M-100 -30 L40 -175 L80 -150 L-40 -20Z" fill="#5d82a8" stroke-width="{12/s:.0f}"/>
<path d="M-70 -50 L50 -165 M-40 -30 L70 -140" fill="none" stroke="#f6ecd8" stroke-width="{12/s:.0f}"/>
<path d="M-60 -70 L140 -70" fill="none" stroke="#9a6a45" stroke-width="{16/s:.0f}"/></g>'''
def umbrella(x,y,s=1,c1="#c8574b",c2="#f6ecd8"):
    return f'''<g transform="translate({x} {y}) scale({s})">
<path d="M0 0 L0 -260" fill="none" stroke-width="{12/s:.0f}"/>
<path d="M-180 -220 Q0 -380 180 -220 Z" fill="{c1}" stroke-width="{12/s:.0f}"/>
<path d="M-60 -220 Q0 -380 60 -220Z" fill="{c2}" stroke-width="{10/s:.0f}"/></g>'''
save("WeekDays","Weekend","sage",f'''
{strip({5,6},230)}
{sh(512,835,300,28)}
<path d="M170 830 Q512 790 854 830" fill="none" stroke-width="10" opacity="0.3"/>
{umbrella(620,830,1.0)}
{deckchair(470,830,1.25)}
<path d="M660 780 L720 780 L710 830 L670 830Z" fill="#e0b04f" stroke-width="10"/>
<path d="M700 780 L720 740" fill="none" stroke-width="8"/>
{sun(270,480,45,10,"#e0b04f")}
''')
# Long weekend: Fri+Sat+Sun highlighted + suitcase and palm
def palm(x,y,s=1):
    fr=''.join(f'<path transform="rotate({r})" d="M0 0 Q50 -85 175 -55 Q100 15 0 0Z" fill="#5f8f4e" stroke-width="{10/s:.0f}"/>' for r in [-170,-140,-100,-60,-20,10])
    return f'''<g transform="translate({x} {y}) scale({s})">
<path d="M-20 360 Q-10 180 0 0 L20 0 Q15 180 20 360Z" fill="#9a6a45" stroke-width="{12/s:.0f}"/>
<g>{fr}</g><circle cx="-10" cy="12" r="16" fill="#7a5a3a" stroke-width="{8/s:.0f}"/><circle cx="14" cy="16" r="16" fill="#7a5a3a" stroke-width="{8/s:.0f}"/></g>'''
save("WeekDays","Long_weekend","sky",f'''
{strip({4,5,6},230)}
{sh(512,845,320,28)}
{palm(730,560,0.8)}
<rect x="330" y="560" width="300" height="270" rx="30" fill="#d9825b" stroke-width="14"/>
<path d="M430 560 L430 510 Q430 490 450 490 L510 490 Q530 490 530 510 L530 560" fill="none" stroke-width="14"/>
<path d="M400 560 L400 830 M560 560 L560 830" fill="none" stroke="#9a6a45" stroke-width="18"/>
<path d="M355 590 L355 800" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.3"/>
<circle cx="480" cy="700" r="36" fill="#e0b04f" stroke-width="9"/>
<circle cx="600" cy="660" r="22" fill="#8fc3d6" stroke-width="8"/>
{sun(250,470,40,10,"#e0b04f")}
''')
# Holiday / day off: hammock between two palms, relaxed person
save("WeekDays","Holiday__day_off","sand",f'''
{sh(512,850,340,28)}
{palm(230,420,1.0)}{palm(800,420,1.0)}
<path d="M240 520 Q512 820 790 520" fill="none" stroke-width="8"/>
<path d="M280 560 Q512 800 750 560 Q512 720 280 560Z" fill="#c8574b" stroke-width="12"/>
<path d="M330 600 Q512 740 700 600" fill="none" stroke="#e0b04f" stroke-width="10"/>
<path d="M430 640 Q520 690 640 620 L660 590 Q560 640 440 600Z" fill="#4f9a9a" stroke-width="10"/>
<circle cx="400" cy="590" r="50" fill="#f1c9a5" stroke-width="12"/>
<path d="M352 580 Q360 530 405 535 Q440 535 450 570 Q420 555 390 560 Q370 560 352 580Z" fill="#3b2a22" stroke-width="8"/>
<path d="M378 595 q10 8 20 0 M410 598 q10 8 20 0" fill="none" stroke-width="6"/>
<path d="M395 615 q10 8 20 0" fill="none" stroke-width="6"/>
<circle cx="378" cy="612" r="9" fill="#e8907f" opacity="0.5" stroke="none"/>
<path d="M640 610 Q680 600 700 570" fill="none" stroke="#f1c9a5" stroke-width="22"/>
{sun(512,300,50,10,"#e0b04f")}
''')
# Public holiday: calendar with red day + striped bunting
def bunting(y):
    out=[f'<path d="M150 {y} Q512 {y+70} 874 {y}" fill="none" stroke-width="8"/>']
    for k in range(9):
        x=190+k*80; t=(x-150)/724; yy=y+4*70*t*(1-t)*0.5*2*0.5*2  # parabola approx
        yy=y+70*4*t*(1-t)/2
        out.append(f'''<g transform="translate({x} {yy:.0f})"><defs><clipPath id="bf{k}"><path d="M-32 0 L32 0 L0 80Z"/></clipPath></defs>
<g clip-path="url(#bf{k})" stroke="none"><rect x="-40" y="0" width="80" height="14" fill="#c8574b"/><rect x="-40" y="14" width="80" height="14" fill="#f6ecd8"/><rect x="-40" y="28" width="80" height="24" fill="#5d82a8"/><rect x="-40" y="52" width="80" height="14" fill="#f6ecd8"/><rect x="-40" y="66" width="80" height="14" fill="#c8574b"/></g>
<path d="M-32 0 L32 0 L0 80Z" fill="none" stroke-width="8"/></g>''')
    return ''.join(out)
grid=[]
for r in range(4):
    for cc in range(5):
        x=270+cc*110; yy=490+r*85
        if (r,cc)==(1,2):
            grid.append(f'<rect x="{x}" y="{yy}" width="84" height="64" rx="12" fill="{RED}" stroke-width="10"/>{star(x+42,yy+32,22,"#f6ecd8",6)}')
        else:
            grid.append(f'<rect x="{x}" y="{yy}" width="84" height="64" rx="12" fill="#efe4cf" stroke="none"/>')
save("WeekDays","Public_holiday","beige",f'''
{sh(512,870,330,28)}
{page(PX,300,PW,550,150,RED,''.join(grid),PAPER,rings=(300,724))}
{bunting(170)}
''')
# New year's day: fireworks + calendar page with 1
save("WeekDays","New_year_s_day","sky",f'''
{burst(260,290,90,"#e0b04f")}{burst(770,280,80,"#e48aa6")}{burst(512,200,60,"#5d82a8",10)}{burst(230,560,50,"#7fa05a",10)}{burst(800,560,55,"#d9825b",10)}
{sh(512,860,230,24)}
{page(342,420,340,420,110,RED,"",PAPER,rings=(420,604),cid="ny",sw=14,r=30)}
{T(512,780,"1",260,"#c8574b",10)}
''')
# Thai new year (Songkran): water gun + bowl with flowers
save("WeekDays","Thai_New_year_s_day","sky",f'''
{sh(560,845,260,26)}
<path d="M380 360 Q520 320 610 520" fill="none" stroke="#8fc3d6" stroke-width="30"/>
{drop(600,560,1.1,"#8fc3d6")}{drop(660,500,0.9,"#8fc3d6")}{drop(540,470,0.8,"#8fc3d6")}{drop(700,580,0.8,"#8fc3d6")}
<g transform="rotate(-20 270 380)">
<rect x="170" y="340" width="220" height="70" rx="30" fill="#e48aa6" stroke-width="14"/>
<rect x="385" y="358" width="50" height="34" rx="8" fill="#e0b04f" stroke-width="10"/>
<circle cx="250" cy="320" r="46" fill="#6fa8d6" stroke-width="12"/>
<path d="M210 405 L190 500 Q185 520 205 522 L245 522 L260 405Z" fill="#e48aa6" stroke-width="14"/>
<path d="M190 360 L360 360" fill="none" stroke="#ffffff" stroke-width="12" opacity="0.35"/>
</g>
{bowl(580,690,1.05)}
{jasmine(760,420,0.6)}{jasmine(320,700,0.55)}
''')
# Christmas day: tree + gifts
save("WeekDays","Christmas_s_day","rose",f'''
{sh(512,850,330,30)}
{tree(470,830,1.25)}
{gift(610,700,150,130,"#c8574b")}
{gift(280,740,110,90,"#5d82a8")}
''')

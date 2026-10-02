import random
from common import *
random.seed(3)
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def hl(d,w=16,op=0.3): return f'<path d="{d}" fill="none" stroke="#ffffff" stroke-width="{w}" opacity="{op}"/>'
lines=[]
for i in range(60):
    x=random.uniform(340,690); y=random.uniform(440,610)
    if y < 610-(1-((x-512)/200)**2)*170: continue
    dx=random.uniform(-40,40); dy=random.uniform(-15,15)
    lines.append(f'<path d="M{x:.0f} {y:.0f} q{dx/2:.0f} -12 {dx:.0f} {dy:.0f}" fill="none" stroke="#7fa05a" stroke-width="7"/>')
strands=''.join(lines)
def tomato(x,y,r): return f'<g transform="rotate({r} {x} {y})"><path d="M{x-50} {y} A50 50 0 0 0 {x+50} {y}Z" fill="#c8574b" stroke-width="11"/><path d="M{x-32} {y+4} A32 32 0 0 0 {x+32} {y+4}Z" fill="#e8907f" stroke="none"/><circle cx="{x-12}" cy="{y+16}" r="5" fill="#f6ecd8" stroke="none"/><circle cx="{x+12}" cy="{y+16}" r="5" fill="#f6ecd8" stroke="none"/></g>'
def bean(x1,y1,x2,y2): return f'<path d="M{x1} {y1} L{x2} {y2}" stroke-width="30"/><path d="M{x1} {y1} L{x2} {y2}" stroke="#6f9a4a" stroke-width="15"/>'
def chs(x,y): return f'<circle cx="{x}" cy="{y}" r="14" fill="#c8574b" stroke-width="7"/><circle cx="{x}" cy="{y}" r="5" fill="#f3d9a0" stroke="none"/>'
save("HowToCook","Spicy sour salad","sand",f'''
{sh(512,770,350,50)}
<ellipse cx="512" cy="620" rx="360" ry="160" fill="#f6ecd8" stroke-width="16"/>
<ellipse cx="512" cy="612" rx="280" ry="115" fill="#e8dcc6" stroke-width="8"/>
<path d="M310 620 Q320 430 512 410 Q704 430 714 620 Q512 680 310 620Z" fill="#cfe0a0" stroke-width="14"/>
{strands}
{hl("M350 560 Q370 470 450 440",14,0.5)}
{bean(380,500,520,460)}{bean(560,560,690,600)}
{tomato(420,600,-15)}{tomato(610,470,20)}{tomato(530,640,5)}
{chs(470,520)}{chs(640,560)}{chs(360,560)}{chs(560,420)}
<g fill="#d9a86a" stroke-width="6"><ellipse cx="500" cy="570" rx="12" ry="9"/><ellipse cx="590" cy="520" rx="12" ry="9"/><ellipse cx="440" cy="470" rx="12" ry="9"/><ellipse cx="660" cy="630" rx="12" ry="9"/><ellipse cx="380" cy="630" rx="12" ry="9"/></g>
<g transform="rotate(-15 800 640)"><path d="M720 640 A80 80 0 0 0 880 640Z" fill="#8fbf5a" stroke-width="12"/><path d="M740 646 A60 60 0 0 0 860 646Z" fill="#d6e8a8" stroke="none"/><path d="M800 646 L800 700 M800 646 L760 680 M800 646 L840 680" stroke="#8fbf5a" stroke-width="5"/></g>
''')

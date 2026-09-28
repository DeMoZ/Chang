from common import *
def sh(x,y,rx,ry): return f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def leaf(x,y,rot,s=1): return f'<path transform="translate({x} {y}) rotate({rot}) scale({s})" d="M0 0 Q30 -40 80 -30 Q50 10 0 0Z" fill="#5f8f4e" stroke-width="{9/s:.0f}"/>'
def banana(x,y,col,rot=0,s=1,spots=False,op=1):
    sp=''.join(f'<ellipse cx="{a}" cy="{b}" rx="9" ry="6" fill="#5c3d2e" stroke="none" opacity="0.8"/>' for a,b in [(90,95),(170,140),(240,160),(130,110)]) if spots else ''
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({s})" opacity="{op}">
<path d="M-10 -20 L-30 -60 L0 -70 L15 -25Z" fill="#7a5a3a" stroke-width="{10/s:.0f}"/>
<path d="M-15 -20 C-15 170 160 270 345 200 C370 190 362 158 335 160 C200 175 100 100 40 -30 Z" fill="{col}" stroke-width="{14/s:.0f}"/>
<path d="M20 10 C50 120 150 185 270 180" fill="none" stroke="#ffffff" stroke-width="{12/s:.0f}" opacity="0.35"/>
<path d="M5 30 C40 150 150 215 280 205" fill="none" stroke-width="{7/s:.0f}" opacity="0.35"/>
{sp}
</g>'''
# Fruit basket
weave=''.join(f'<path d="M{250+i*70} 600 L{290+i*55} 800" fill="none" stroke-width="7" opacity="0.4"/>' for i in range(8))
grapes=''.join(f'<circle cx="{x}" cy="{y}" r="30" fill="#8a5a86" stroke-width="9"/>' for x,y in [(680,470),(740,470),(710,515),(770,515),(650,515),(680,560),(740,560),(710,605)])
save("Fruits","Fruit","sand",f'''
{sh(512,830,320,40)}
{banana(330,330,"#e0b04f",-10,1.1)}
<path d="M300 580 Q270 470 340 430 Q400 420 420 480 Q440 560 390 610Z" fill="#d9825b" stroke-width="14"/>
<path d="M320 540 Q310 480 345 455" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.35"/>
{grapes}
<path d="M710 440 L715 400" stroke-width="10"/>{leaf(712,410,-20,0.8)}
<circle cx="600" cy="520" r="95" fill="#c8574b" stroke-width="14"/>
<path d="M600 430 Q595 400 610 380" fill="none" stroke-width="10"/>{leaf(605,395,-30,0.9)}
<path d="M540 480 Q550 450 580 440" fill="none" stroke="#ffffff" stroke-width="14" opacity="0.4"/>
<circle cx="460" cy="540" r="90" fill="#e0a040" stroke-width="14"/>
<circle cx="435" cy="505" r="20" fill="#ffffff" opacity="0.35" stroke="none"/>
<circle cx="460" cy="454" r="7" fill="#5f8f4e" stroke-width="5"/>
<path d="M230 580 L794 580 L740 820 Q512 850 284 820Z" fill="#9a6a45" stroke-width="14"/>
{weave}
<path d="M270 680 Q512 710 754 680 M290 760 Q512 790 734 760" fill="none" stroke-width="7" opacity="0.4"/>
<rect x="210" y="560" width="604" height="50" rx="25" fill="#b88458" stroke-width="14"/>
<path d="M240 575 L780 575" stroke="#ffffff" stroke-width="10" opacity="0.3"/>
''')
# Ripe
save("Fruits","Ripe","sage",f'''
{sh(300,790,160,25)}{sh(660,705,220,28)}
<g opacity="0.4">{banana(150,560,"#8fb060",0,0.85)}</g>
<circle cx="660" cy="540" r="250" fill="#ffffff" opacity="0.35" stroke="none"/>
{banana(450,390,"#e8b84a",0,1.2,True)}
{sparkle(820,380,0.9)}{sparkle(560,330,0.6)}{sparkle(850,650,0.6,"#d9825b")}
''')

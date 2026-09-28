from common import *
SH='<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#2e211b" opacity="0.15" stroke="none"/>'
def sh(x,y,rx,ry): return SH.format(x=x,y=y,rx=rx,ry=ry)

# Rice
grains="".join(f'<ellipse cx="{x}" cy="{y}" rx="14" ry="7" transform="rotate({r} {x} {y})" fill="none" stroke-width="5" opacity="0.35"/>' for x,y,r in [(400,430,20),(470,380,-30),(560,400,10),(620,450,-20),(450,480,40),(530,470,-10),(350,490,0),(680,500,30),(590,500,15),(510,345,5)])
save("Food","Rice","beige",f'''
{sh(512,800,280,45)}
{steam([430,512,594],300)}
<path d="M270 520 Q280 350 512 325 Q744 350 754 520Z" fill="#fbf6ea" stroke-width="14"/>
{grains}
<path d="M330 450 Q360 380 440 355" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.8"/>
<path d="M392 750 L380 800 L644 800 L632 750Z" fill="#4a6d8f" stroke-width="12"/>
<path d="M232 520 Q240 770 512 780 Q784 770 792 520Z" fill="#5d82a8" stroke-width="14"/>
<path d="M262 590 Q512 640 762 590" fill="none" stroke="#f6ecd8" stroke-width="16"/>
<g fill="#f6ecd8" stroke-width="6"><circle cx="360" cy="680" r="16"/><circle cx="512" cy="700" r="16"/><circle cx="664" cy="680" r="16"/></g>
<path d="M280 560 Q300 690 380 740" fill="none" stroke="#ffffff" stroke-width="18" opacity="0.3"/>
<path d="M232 520 Q512 560 792 520" fill="none" stroke-width="14"/>
''')

# Spicy
save("Food","Spicy","rose",f'''
{flame(400,560,1.25,-10)}{flame(560,560,1.55)}{flame(710,600,1.2,12)}
{sh(530,740,300,36)}
{chili(250,500,-45,1.3)}
{sparkle(250,330,0.8,"#d9825b")}{sparkle(820,420,0.7,"#e0b04f")}
''')

# A little spicy: small chili on a small plate
save("Food","A little spicy","sand",f'''
{sh(512,720,270,50)}
<ellipse cx="512" cy="690" rx="260" ry="80" fill="#f6ecd8" stroke-width="14"/>
<ellipse cx="512" cy="680" rx="170" ry="45" fill="#e8dcc6" stroke-width="8"/>
{chili(420,610,-60,0.6)}
{flame(640,520,0.45,10)}
''')

# Not spicy
save("Food","Not spicy","sage",f'''
{sh(512,800,260,40)}
{chili(290,420,-40,1.15)}
<circle cx="512" cy="512" r="300" fill="none" stroke-width="64"/>
<circle cx="512" cy="512" r="300" fill="none" stroke="#c8574b" stroke-width="38"/>
<path d="M300 300 L724 724" stroke-width="64"/>
<path d="M300 300 L724 724" stroke="#c8574b" stroke-width="38"/>
''')

# Delicious: happy face licking lips + noodle bowl + sparkles
save("Food","Delicious","beige",f'''
<circle cx="512" cy="400" r="215" fill="#f1c9a5" stroke-width="14"/>
<path d="M300 380 Q290 180 512 175 Q734 180 724 380 Q690 260 600 250 Q520 300 400 270 Q320 300 300 380Z" fill="#3b2a22" stroke-width="14"/>
<path d="M400 390 Q430 355 460 390" fill="none" stroke-width="14"/>
<path d="M564 390 Q594 355 624 390" fill="none" stroke-width="14"/>
<ellipse cx="400" cy="450" rx="34" ry="20" fill="#e8907f" opacity="0.5" stroke="none"/>
<ellipse cx="624" cy="450" rx="34" ry="20" fill="#e8907f" opacity="0.5" stroke="none"/>
<path d="M450 470 Q512 540 574 470 Z" fill="#8a3b33" stroke-width="12"/>
<path d="M540 490 Q575 530 600 505 Q600 480 570 478Z" fill="#e8907f" stroke-width="10"/>
{sh(512,860,270,35)}
<path d="M300 690 Q310 850 512 855 Q714 850 724 690Z" fill="#c8574b" stroke-width="14"/>
<path d="M340 720 Q360 810 430 835" fill="none" stroke="#ffffff" stroke-width="16" opacity="0.3"/>
<path d="M300 690 Q512 650 724 690 Q512 730 300 690Z" fill="#e0b04f" stroke-width="12"/>
<path d="M370 690 Q410 670 450 690 Q490 710 530 690 Q570 670 610 690" fill="none" stroke-width="7" opacity="0.5"/>
<path d="M600 700 L700 560 M630 705 L735 575" stroke-width="14"/>
<path d="M600 700 L700 560 M630 705 L735 575" stroke="#9a6a45" stroke-width="6"/>
{sparkle(230,250,1.1)}{sparkle(800,230,0.9,"#d9825b")}{sparkle(820,480,0.6)}{sparkle(200,520,0.7,"#d9825b")}
''')

# Taste: face tasting from a spoon
save("Food","Taste","sky",f'''
{sh(512,860,230,30)}
<path d="M320 860 Q320 700 512 690 Q704 700 704 860Z" fill="#4f9a9a" stroke-width="14"/>
<circle cx="512" cy="450" r="215" fill="#f1c9a5" stroke-width="14"/>
<path d="M300 430 Q290 225 512 220 Q734 225 724 430 Q690 310 600 300 Q520 350 400 320 Q320 350 300 430Z" fill="#3b2a22" stroke-width="14"/>
<circle cx="450" cy="445" r="15" fill="#2e211b"/>
<circle cx="594" cy="445" r="15" fill="#2e211b"/>
<path d="M415 395 Q445 380 475 392 M559 392 Q589 380 619 395" fill="none" stroke-width="10"/>
<ellipse cx="400" cy="505" rx="34" ry="20" fill="#e8907f" opacity="0.5" stroke="none"/>
<ellipse cx="640" cy="505" rx="30" ry="18" fill="#e8907f" opacity="0.5" stroke="none"/>
<ellipse cx="500" cy="565" rx="34" ry="28" fill="#8a3b33" stroke-width="10"/>
<path d="M670 650 L800 790" stroke-width="40"/>
<path d="M670 650 L800 790" stroke="#e2cfb3" stroke-width="18"/>
<ellipse cx="615" cy="600" rx="78" ry="42" transform="rotate(35 615 600)" fill="#e2cfb3" stroke-width="12"/>
<ellipse cx="613" cy="597" rx="56" ry="26" transform="rotate(35 613 597)" fill="#d9825b" stroke-width="6"/>
<circle cx="770" cy="760" r="48" fill="#f1c9a5" stroke-width="12"/>
<path d="M740 740 Q770 730 800 745" fill="none" stroke-width="8"/>
{sparkle(790,330,0.9)}{sparkle(250,300,0.6,"#d9825b")}
''')

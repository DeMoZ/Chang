import os
W=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BG={"beige":("#f3e6d6","#c9ab93"),"rose":("#f1e0dc","#bf9a98"),"sage":("#e7ecdc","#a9b595"),"sky":("#e3ecf1","#9fb3c1"),"sand":("#f5ead0","#cfb27e")}
def save(cat,key,bg,body,defs=""):
    l,d=BG[bg]
    s=f'''<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">
<defs>
  <radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{l}"/><stop offset="100%" stop-color="{d}"/></radialGradient>
{defs}
</defs>
<rect width="1024" height="1024" fill="url(#bg)"/>
<g stroke="#2e211b" stroke-linejoin="round" stroke-linecap="round">
{body}
</g>
</svg>'''
    os.makedirs(f"{W}/svg/{cat}",exist_ok=True)
    open(f"{W}/svg/{cat}/{key}.svg","w").write(s)
def chili(x,y,rot=0,sc=1,col="#c8574b",extra=""):
    # chili pointing down-right, origin at stem
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({sc})" {extra}>
<path d="M0 0 C60 -10 140 20 180 110 C220 200 230 300 200 380 C190 330 150 250 90 190 C40 140 -10 110 -20 60 C-25 30 -15 5 0 0Z" fill="{col}" stroke-width="{14/sc:.0f}"/>
<path d="M40 40 C100 50 150 110 175 190" fill="none" stroke="#ffffff" stroke-width="{16/sc:.0f}" opacity="0.35"/>
<path d="M0 -15 C-5 -60 10 -85 40 -95" fill="none" stroke="#2e211b" stroke-width="{34/sc:.0f}"/>
<path d="M0 -15 C-5 -60 10 -85 40 -95" fill="none" stroke="#5f8f4e" stroke-width="{16/sc:.0f}"/>
<path d="M-10 20 C-30 0 -30 -20 -10 -30 C10 -20 20 -5 20 10 C10 25 0 25 -10 20Z" fill="#5f8f4e" stroke-width="{10/sc:.0f}"/>

</g>'''

def flame(x,y,s=1,rot=0):
    return f'''<g transform="translate({x} {y}) rotate({rot}) scale({s})">
<path d="M0 0 C-70 0 -95 -60 -70 -120 C-60 -90 -40 -85 -35 -100 C-45 -160 -10 -200 10 -250 C30 -200 75 -170 70 -110 C85 -120 92 -140 88 -160 C130 -100 110 0 0 0Z" fill="#d9825b" stroke-width="{12/s:.0f}"/>
<path d="M0 -15 C-35 -15 -50 -45 -35 -80 C-25 -65 -15 -65 -10 -75 C-15 -110 5 -130 12 -150 C25 -120 50 -100 45 -60 C45 -30 30 -15 0 -15Z" fill="#e0b04f" stroke="none"/>
</g>'''
def sparkle(x,y,s=1,col="#e0b04f"):
    return f'<path transform="translate({x} {y}) scale({s})" d="M0 -40 Q6 -6 40 0 Q6 6 0 40 Q-6 6 -40 0 Q-6 -6 0 -40Z" fill="{col}" stroke-width="{7/s:.0f}"/>'
def steam(xs,y0,op=0.5):
    return "".join(f'<path d="M{x} {y0} q-30 -40 0 -80 q30 -40 0 -80" fill="none" stroke-width="12" opacity="{op}"/>' for x in xs)

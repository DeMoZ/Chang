import os
W=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
keys="0 1 2 3 4 5 6 7 8 9 10 11 20 21 30 31 40 41 50 51 60 61 70 71 80 81 90 91 100 1_000 10_000 100_000 1_000_000 10_000_000".split()
tiles=["#c8574b","#4f9a9a","#d9825b","#5d82a8","#7fa05a","#8a5a86","#e0b04f","#5f8f4e"]
bgs=[("#f3e6d6","#c9ab93"),("#e3ecf1","#9fb3c1"),("#f5ead0","#cfb27e"),("#f1e0dc","#bf9a98"),("#e7ecdc","#a9b595")]
cnt_cols={"#c8574b":"#e0b04f","#4f9a9a":"#d9825b","#d9825b":"#5f8f4e","#5d82a8":"#c8574b","#7fa05a":"#c8574b","#8a5a86":"#e0b04f","#e0b04f":"#c8574b","#5f8f4e":"#d9825b"}
D="#2e211b"
def text(s,cx,cy,fs):
    base=cy+0.358*fs
    sw=max(9,fs*0.07)
    return (f'<text x="{cx}" y="{base:.0f}" text-anchor="middle" font-family="Arial, Helvetica, sans-serif" font-weight="bold" '
            f'font-size="{fs:.0f}" fill="#f6ecd8" stroke="{D}" stroke-width="{sw:.0f}" stroke-linejoin="round" paint-order="stroke">{s}</text>')
def width_em(s): return sum(0.278 if c==',' else 0.556 for c in s)
def fruit(x,y,r,col):
    return (f'<circle cx="{x}" cy="{y}" r="{r}" fill="{col}" stroke="{D}" stroke-width="9"/>'
            f'<circle cx="{x-r*0.35:.0f}" cy="{y-r*0.35:.0f}" r="{r*0.28:.0f}" fill="#ffffff" opacity="0.35"/>'
            f'<path d="M{x} {y-r} q6 -22 24 -26" fill="none" stroke="{D}" stroke-width="8" stroke-linecap="round"/>'
            f'<path d="M{x+2} {y-r-4} q18 -24 36 -8 q-18 16 -36 8z" fill="#5f8f4e" stroke="{D}" stroke-width="6" stroke-linejoin="round"/>')
for i,k in enumerate(keys):
    t=tiles[i%len(tiles)]; bl,bd=bgs[i%len(bgs)]
    s=[f'<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">',
       f'<defs><radialGradient id="bg" cx="50%" cy="45%" r="75%"><stop offset="0%" stop-color="{bl}"/><stop offset="100%" stop-color="{bd}"/></radialGradient></defs>',
       '<rect width="1024" height="1024" fill="url(#bg)"/>',
       f'<rect x="188" y="196" width="680" height="680" rx="80" fill="{D}" opacity="0.15"/>',
       f'<rect x="172" y="172" width="680" height="680" rx="80" fill="{t}" stroke="{D}" stroke-width="16"/>',
       f'<path d="M212 300 Q212 212 300 212 L724 212" fill="none" stroke="#ffffff" stroke-width="18" stroke-linecap="round" opacity="0.3"/>']
    n=int(k.replace('_',''))
    label=f"{n:,}"
    if n<=11:
        s.append(text(label,512,390,300))
        s.append(f'<rect x="222" y="572" width="580" height="240" rx="50" fill="#f6ecd8" stroke="{D}" stroke-width="12"/>')
        s.append(f'<rect x="222" y="752" width="580" height="60" rx="30" fill="{D}" opacity="0.08"/>')
        if n==0:
            s.append(f'<ellipse cx="512" cy="700" rx="200" ry="70" fill="#ffffff" stroke="{D}" stroke-width="12"/>'
                     f'<ellipse cx="512" cy="692" rx="120" ry="38" fill="#e8dcc6" stroke="{D}" stroke-width="8"/>'
                     f'<path d="M390 670 Q420 650 460 648" fill="none" stroke="#ffffff" stroke-width="10" stroke-linecap="round" opacity="0.8"/>')
        else:
            rows=[n] if n<=5 else [ (n+1)//2, n//2 ]
            r=46 if n<=5 else 36
            sp=108 if n<=5 else 92
            ys=[692] if len(rows)==1 else [642,748]
            col=cnt_cols[t]
            for row,y in zip(rows,ys):
                x0=512-(row-1)*sp/2
                for j in range(row):
                    s.append(fruit(round(x0+j*sp),y+8,r,col))
    else:
        fs=min(380, 640/width_em(label))
        s.append(text(label,512,512,fs))
    s.append('</svg>')
    os.makedirs(f"{W}/svg/Numbers",exist_ok=True)
    open(f"{W}/svg/Numbers/{k}.svg","w").write("\n".join(s))

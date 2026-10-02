import os, sys, importlib
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
W = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
out = os.path.join(W, 'svg', 'Places'); os.makedirs(out, exist_ok=True)
allS = {}
for m in ('places_a', 'places_b', 'places_c'):
    try:
        allS.update(importlib.import_module(m).S)
    except ModuleNotFoundError as e:
        if e.name != m: raise
only = sys.argv[1:]
for k, f in allS.items():
    if only and k not in only: continue
    open(os.path.join(out, k + '.svg'), 'w').write(f())
print(len(allS))

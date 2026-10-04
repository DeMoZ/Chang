"""Regenerate every svg/<Category>/<Key>.svg by running all generator scripts.

Each generator writes its own set of keys (no overlaps), so the order does not matter.
gen/auto/<Category>/<Key>.py are one-picture generators made from the Unity prompts config.
"""
import glob
import os
import subprocess
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))

GENERATORS = [
    "gen_people.py",      # FamilyAndPeople (family), Gender, Ocupation
    "gen_cc.py",          # Clothes, Colors, FamilyAndPeople/Boss, Chief, Colleague
    "gen_bev.py",         # Beverages, Sweetnes
    "gen_prep.py",        # Prepositions, PointThings
    "gen_greet.py",       # Greeting, Introduce
    "gen/numbers.py",     # Numbers
    "gen/food.py",        # Food: taste words, Rice
    "gen/food_a.py",      # Food: Beef .. Morning glory
    "gen/food2.py",       # Food: Mushroom .. Vinegar
    "gen/fruits.py",      # Fruits/Fruit, Fruits/Ripe
    "gen/fruits2.py",     # Fruits: single fruits
    "gen/cook.py",        # HowToCook, Months/Month
    "gen/timewords.py",   # Months, WeekDays
    "gen/shopmix.py",     # Shopping, Mix
    "gen/places_run.py",  # Places (optionally: keys as args)
]

AUTO_GENERATORS = sorted(os.path.relpath(p, ROOT) for p in glob.glob(os.path.join(ROOT, "gen", "auto", "*", "*.py")))

if __name__ == "__main__":
    for script in GENERATORS + AUTO_GENERATORS:
        path = os.path.join(ROOT, script)
        print(script)
        subprocess.run([sys.executable, os.path.basename(path)], cwd=os.path.dirname(path), check=True,
                       stdout=subprocess.DEVNULL)

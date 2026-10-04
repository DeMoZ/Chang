# Word images

Sources and tools for the word pictures in
`Assets/Project/Resources_Bundled/ImageWords/Thai/<Category>/<Key>.png`.

Every picture is an SVG in `svg/<Category>/<Key>.svg`, produced by a Python generator
(`gen_*.py`, `gen/*.py`). The file name is the word key exactly as in the word configs /
sound files (`SoundWords/Thai/...`). Style rules: [STYLE.md](STYLE.md).

## Setup (once)

```sh
brew install cairo
python3 -m venv venv
venv/bin/pip install -r requirements.txt
```

`render.py` loads Homebrew's cairo by its full path, no `DYLD_*` variables are needed.

## Update pictures

Usually from Unity: **Chang/Utilities/Media Prompts** keeps a prompt for every word picture and its **Image**
button runs the steps below (a new picture is drawn by the Claude Code CLI as `gen/auto/<Category>/<Key>.py`).
See `Docs/content-pipeline.md`.

By hand:

1. Edit the generator that owns the picture (see the list in `generate.py`) and run it,
   or run `venv/bin/python generate.py` to rebuild all SVGs.
   A new word: add a drawing for its key to the matching generator (or a new script in `gen/`).
2. Render and review: `venv/bin/python render.py <Category> [<Category> ...]`
   → `png/<Category>/*.png` + `png/<Category>/_contact.png` contact sheet.
   `<Category>/<Key>` renders one picture.
3. Install into the Unity project: `venv/bin/python install.py <Category> [...]` (or `<Category>/<Key>`).
   - Existing PNGs are overwritten, their `.meta` (GUID) is kept.
   - New PNGs get a sprite `.meta`; new category folders get a folder `.meta` and are added
     to the `Remote_Thai_Image_Words` Addressables group.
4. Open Unity so it reimports the textures.

`png/` is a build output and is not committed.

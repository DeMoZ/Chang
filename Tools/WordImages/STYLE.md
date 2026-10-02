# Word illustration style guide (Chang project)

Goal: flat-vector illustrations that sit comfortably next to the existing painted
word pictures (warm, soft, storybook feel, thick dark outlines, vignette background).

## Canvas
- `viewBox="0 0 1024 1024"`, `width="1024" height="1024"`, root `<svg xmlns="http://www.w3.org/2000/svg">`.
- Always start with the shared background (copy exactly, then pick ONE palette pair):

```xml
<defs>
  <radialGradient id="bg" cx="50%" cy="45%" r="75%">
    <stop offset="0%" stop-color="BG_LIGHT"/>
    <stop offset="100%" stop-color="BG_DARK"/>
  </radialGradient>
</defs>
<rect width="1024" height="1024" fill="url(#bg)"/>
```

  Background pairs (BG_LIGHT / BG_DARK) — choose one fitting the subject:
  - warm beige: `#f3e6d6` / `#c9ab93`
  - dusty rose: `#f1e0dc` / `#bf9a98`
  - sage:       `#e7ecdc` / `#a9b595`
  - sky:        `#e3ecf1` / `#9fb3c1`
  - sand:       `#f5ead0` / `#cfb27e`

## Drawing rules
- Main subject centered, occupying roughly 60–75% of the canvas (about x 150–874, y 150–874). Nothing touches the edges.
- Every shape has a dark outline: `stroke="#2e211b"`, `stroke-width` 12–16 for main shapes, 8–10 for details,
  `stroke-linejoin="round" stroke-linecap="round"`.
- Soft ground shadow under standing objects: ellipse `fill="#2e211b" opacity="0.15"`, no stroke.
- Muted, warm palette (avoid pure saturated primaries). Examples: brick red `#c8574b`, terracotta `#d9825b`,
  mustard `#e0b04f`, cream `#f6ecd8`, olive green `#7fa05a`, leaf `#5f8f4e`, teal `#4f9a9a`, dusty blue `#5d82a8`,
  plum `#8a5a86`, wood brown `#9a6a45`, dark brown `#5c3d2e`, skin tones `#f1c9a5` `#e0a882` `#b57d58`, hair `#3b2a22`.
- Add depth with one highlight and one shade per big shape: lighter overlay (`#ffffff` opacity 0.25–0.35) or darker
  overlay (`#2e211b` opacity 0.12–0.2), or simple linear gradients. Keep it clean — no noise.
- People: friendly simple cartoon style — round head, dot eyes, small smile, rosy cheeks (`#e8907f` opacity 0.5),
  simple bodies. Southeast-Asian context (Thailand) where relevant. Keep a consistent look across family members.
- Text: avoid words. Allowed ONLY when unavoidable: digits for Numbers, `?` for questions, `฿` for money, `kg` on a scale.
  Use font-family="Arial, Helvetica, sans-serif", font-weight="bold", with the same dark stroke via `paint-order="stroke"`.
- Abstract words (prepositions, greetings, particles, adjectives): use a clear visual metaphor — e.g. a ball and a box
  for In/On/Under/Behind, arrows on a road for Turn/Go straight, two people waving for Hi/Bye, a steaming bowl for
  "Have you eaten yet", a speech bubble with `?` for question words, a thumbs-up for "I am good", etc.
  Comparison words (Big/Small/Medium, Cheap/Expensive, Less/Extra sweet) should visually highlight the target concept
  (e.g. the highlighted item is big, others faded at opacity 0.35).
- Renderer is CairoSVG: supported = paths, basic shapes, linear/radial gradients, opacity, transforms, text, clipPath.
  NOT supported: filters (blur, drop-shadow, feTurbulence), masks may be unreliable, CSS in `<style>` is ok but prefer attributes.
  No external images, no `<use href>` to external files.
- CairoSVG ignores `paint-order="stroke"`: for outlined text draw a stroked copy first and a plain filled copy on top.
- The default font has no `฿` glyph (renders as a box): draw the baht sign as a path (see `baht()` in `gen/shopmix.py`).

## Files
- Save as `svg/<Category>/<Key>.svg`, where `<Key>` is EXACTLY the name from the todo list (same spaces/underscores/case).
- Render check: `python render.py <Category>` (see README.md for setup) →
  writes `png/<Category>/<Key>.png` and `png/<Category>/_contact.png` (a contact sheet). LOOK at the contact sheet
  and fix anything unclear, broken, or off-style.

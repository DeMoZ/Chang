# Proposal

## Why

Today the WebGL page (`Assets/WebGLTemplates/ChangPWA/index.html`) starts downloading the Unity build (about 50 MB: `.data.br`, `.wasm.br`, `.framework.js.br`) as soon as the page runs JavaScript. The Service Worker adds a second trigger: on install it precaches the same build files. Because of this, every crawler, link-preview bot or scanner that runs JavaScript downloads the whole game. That wastes bandwidth, makes the first screen wait for the download, and makes the Cloudflare request counts for `.wasm.br` useless as a count of real launches.

## What Changes

- The page opens with a start screen: the Kru Chaang mascot and a **Play** button. No Unity build file is requested before the button is pressed.
- Pressing **Play** loads the Unity loader script and calls `createUnityInstance(...)`. The start screen then turns into the existing loading bar.
- The Service Worker no longer precaches the build files (`Build/*`) on install. It still caches them cache-first at runtime, the first time Unity fetches them after **Play**. So a page visit by itself does not download the build.
- No click is needed in these cases (the person already chose to play):
  - the page was opened as an installed PWA (`display-mode: standalone`);
  - the page reloads itself because of the build version check (#104) while the game is running.
- Docs: `Docs/build-and-deploy.md` (WebGL template section) describes the start screen and what `.wasm.br` requests now count.

Out of scope:

- Cloudflare-side bot protection (WAF, Bot Fight Mode, rate limits) and analytics dashboards.
- Any change inside the Unity game (C#, scenes); the in-game start flow stays as it is.
- Unity Play hosting: it uses its own page, not this template.
- Localizing the start screen: the page is in English like the rest of the page texts (#107).

## Capabilities

### New Capabilities
- `webgl-page-loading`: how the WebGL web page starts the game — the start screen, when the Unity build is downloaded, and what the Service Worker caches.

### Modified Capabilities
<!-- none: there are no specs in openspec/specs/ yet -->

## Impact

- `Assets/WebGLTemplates/ChangPWA/index.html` — start screen markup, deferred loader/`createUnityInstance`, autostart rules.
- `Assets/WebGLTemplates/ChangPWA/TemplateData/style.css` — start screen and **Play** button styles.
- `Assets/WebGLTemplates/ChangPWA/ServiceWorker.js` — precache list without `Build/*`.
- `Docs/build-and-deploy.md` — WebGL template notes.
- Analytics: requests to `Build/*.wasm.br` count first launches of a build per browser. A returning player whose build is already cached (Service Worker or Unity data cache) does not send that request again, so it is not a count of every launch.

# Design

## Context

- `index.html` (template `ChangPWA`) currently appends the loader script `Build/<loader>.js` right away, and its `onload` calls `createUnityInstance(canvas, config, onProgress)`. The loading bar (`#unity-loading-bar`, mascot `#unity-logo` + progress bar) is shown immediately.
- `ServiceWorker.js` is registered on `window.load`. Its `install` handler runs `cache.addAll(contentToCache)`, and that list includes `Build/<loader>`, `<framework>`, `<data>`, `<code>`. So the 50 MB build is downloaded on any visit, even if the page itself never loads Unity. The `fetch` handler already caches same-origin GET requests cache-first at runtime.
- Unity data caching (`webGLDataCaching: 1`) also stores the `.data` file in the browser's Cache Storage after the first download.
- The version check (#104) deletes all caches and calls `location.reload()` when `version.json` differs from the baked-in build version; `sessionStorage` key `chang-version-reload` prevents loops.

## Goals / Non-Goals

**Goals:**
- Zero `Build/*` requests until a person starts the game (see spec "No build download before Play").
- Keep offline play and the #104 version check working.

**Non-Goals:**
- Preloading or prefetching the build while the start screen is shown (it would bring the bot traffic back).
- Changing the visual design of the loading bar.

## Decisions

1. **Start screen reuses the loading container, drawn like the in-game loading screen.** `#unity-loading-bar` becomes a full-screen layer with the logo ช้าง, the `<button id="unity-play">Play</button>` and the progress bar with the percent (hidden until start). It copies `Assets/Project/UI/Screens/Loading.prefab`: DesignTheme colors, sizes in game UI units (`--u` = CanvasScaler 540×1080 Expand), logo anchored to the top and bar/button/percent to the bottom like the prefab, so the hand-over to the game has no visual jump. The button is the game's `Button/Primary` (Royal indigo, radius 20), at least 48 px high. Fonts are tiny woff2 subsets (Charmonman for ช้าง, Prompt SemiBold for `Play`/digits) instead of images: crisp at any scale, 6 KB in total. Alternative: a picture of the logo — blurry when scaled and not text.

2. **`startGame()` wraps the existing loader code.** The code that creates the loader `<script>` and calls `createUnityInstance` moves into `startGame()`, guarded by a `started` flag (double press). The button calls it; the autostart cases call it on page start. The `<canvas>` stays in the DOM from the start (Unity needs it), with the template background.

3. **Autostart detection.**
   - Installed app: `matchMedia("(display-mode: standalone)").matches || navigator.standalone === true` (iOS).
   - Version reload: when `startGame()` runs, set `sessionStorage["chang-started"] = "1"`. On page start, if that key is set, autostart. The key is per tab and survives `location.reload()`, so it exactly covers "reload in a tab where the game was started". It also covers a manual F5 after playing; that is acceptable (the person already chose to play in that tab).
   - Alternative: a URL flag (`?autostart`) passed to `location.reload` — it would be copied into shared links and bookmarks, so not used.

4. **Service Worker: precache only the shell.** `contentToCache` keeps `TemplateData/style.css` (and the mascot image used by the start screen); all `Build/*` entries are removed. Build files are still cached by the existing cache-first `fetch` handler the first time Unity fetches them, under the same per-build cache name, so the second launch is offline-capable. Alternative: register the Service Worker only after **Play** — it would also work, but a PWA install prompt and offline page shell then depend on the person having played; removing the precache is simpler and keeps registration as it is.

5. **First-play race.** On the very first visit the Service Worker may not yet control the page when **Play** is pressed (`clients.claim()` runs on activate). Then Unity's fetches go straight to the network and are not stored by the Service Worker; Unity's own data cache still stores `.data`. The next visit is controlled and caches the files on the next play. Acceptable: no extra download, only one more online launch before full offline support.

## Risks / Trade-offs

- [Returning players must click Play every time in a browser tab] → one click; installed-app and version-reload cases autostart.
- [Analytics: `.wasm.br` requests count first launches of a build per browser, not every launch] → documented in `Docs/build-and-deploy.md`.
- [Bots that click buttons still download the build] → out of scope (Cloudflare-side protection).
- [Browser audio] → the Play click is a user gesture, so Unity audio can start without the extra "tap to unmute" problem; no risk, a small gain.

## Migration Plan

- Ship with a normal WebGL build: dev → staging → production (Docs/build-and-deploy.md). Old clients pick up the new page through the network-first page fetch and the #104 version check.
- The new Service Worker deletes old caches on activate (per-build cache name), so the old precached build is removed.
- Rollback: Cloudflare Pages rollback to the previous deployment.

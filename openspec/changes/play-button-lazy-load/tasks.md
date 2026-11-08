# Tasks

## 1. Start screen and deferred loading

- [x] 1.1 In `Assets/WebGLTemplates/ChangPWA/index.html` add the **Play** button under the mascot in `#unity-loading-bar` and hide the progress bar until start; verify in a local WebGL build that the page shows mascot + Play and no progress bar
- [x] 1.2 Move the loader `<script>` creation and `createUnityInstance(...)` into `startGame()` with a `started` guard, called by the button; verify in DevTools → Network that no `Build/*` request happens before the click and that a double click loads once
- [x] 1.3 Add autostart: installed app (`display-mode: standalone` / `navigator.standalone`) and `sessionStorage["chang-started"]` set by `startGame()`; verify that F5 after Play autostarts, and a fresh tab shows the button
- [x] 1.4 Style the start screen and button in `TemplateData/style.css` (centered, ≥48 px touch target, keyboard focus outline, works at phone width); verify on a phone-sized viewport and with Tab + Enter

## 2. Service Worker

- [x] 2.1 In `ServiceWorker.js` remove all `Build/*` entries from `contentToCache` (keep the shell: `TemplateData/style.css`, the mascot image); verify on a clean profile that opening the page without clicking makes no `Build/*` requests, including from the Service Worker (DevTools → Application → Service Workers, Network with "Show requests from service workers")
- [x] 2.2 Verify runtime caching: Play once, reload, Play again → `Build/*` served from the Service Worker cache; then go offline in DevTools and Play again → the game starts

## 3. Docs

- [x] 3.1 Update `Docs/build-and-deploy.md` (WebGL build → template changes): the start screen, the autostart cases, the Service Worker precache change, and what `.wasm.br` requests count in analytics; verify the section reads correctly on GitHub

## 5. Look of the in-game loading screen

- [x] 5.1 Redraw the start and loading screens in `index.html` / `TemplateData/style.css` like `Assets/Project/UI/Screens/Loading.prefab` (DesignTheme colors, game UI units 540×1080 Expand, logo from the top, bar/button/percent from the bottom); verify on the real build that the logo and percent match the game's loading screen within 2 px on 390×780 and 390×844
- [x] 5.2 Add the font subsets (`Tools/PageArt/fonts.sh` → `logo-charmonman.woff2`, `prompt-semibold.woff2`), precache them in the Service Worker, remove the unused mascot and progress bar images; verify the behaviour test still passes (12 checks)
- [x] 5.3 Update `Docs/build-and-deploy.md` (start screen section, page images table); verify the section reads correctly

## 4. Integration check

- [ ] 4.1 Deploy to `dev.kruchaang.win` (user runs the wrangler command) and check: no `Build/*` before Play, Play loads and starts the game, the #104 version check still reloads to a new build and autostarts
- [ ] 4.2 Check `staging.kruchaang.win` on a phone (browser tab and installed PWA) before promoting to production

## Workflow follow-up

- Open a PR from `play-button-lazy-load`; merge into `main` is the user's step.
- Archive the change (`/opsx:archive`) after the PR is merged, creating `openspec/specs/webgl-page-loading/spec.md`.

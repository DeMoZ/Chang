# Build and Deploy

## Project settings

| Setting | Value |
|---|---|
| Unity | 6000.3.12f1 |
| Product / Company | Chang / roman.kolychev |
| Version | `bundleVersion` in `ProjectSettings/ProjectSettings.asset` (currently 0.1.1) |
| Bundle id | `com.roman.kolychev.Chang` (Android, Standalone). iOS still has the template id. |
| Scenes in build | Bootstrap, Reboot, Game |
| Unity Cloud | project "Chang", organization "demozbox" |

## Platforms

- **WebGL** is the main public platform: project template `ChangPWA`, hosted on Cloudflare Pages (see [Environments](#environments)). [Unity Play](https://play.unity.com/en/games/efe1a90f-d700-4609-adfd-6481103f4d5b/chang) (`com.unity.connect.share`) is kept only for quick one-off sharing; there the game uses the `dev` environment. Its auto publish is off (`autoPublish` in `ProjectSettings/Packages/com.unity.connect.share/Settings.json`), otherwise every WebGL build would be uploaded to Unity Play; publish manually from the WebGL Publisher window.
- **Android**: min SDK 25. The keystore is in the `ChangExternal/AndroidKeystore` submodule.
- `Builds/` and `WebGL Builds/` are in `.gitignore`.

## What exists today

- **Content:** Addressables are built with **Build & Release** into the `CCDBuildData` folder and uploaded to CCD (environment `dev`, badge `latest`); see [Content builds](#content-builds). The Addressables profile is in CCD **Automatic** mode: the bucket is the one named after the build target (`WebGL`, `Android`, `StandaloneOSX`), so content of different platforms does not overwrite each other.
- **Client:** **Chang/Build/WebGL Release** and **Chang/Build/WebGL Development** (`Assets/Project/Scripts/Editor/Build/WebGLBuild.cs`) build WebGL to `Builds/WebGL/Chang`. **DMZ/Build Data Window** shows the version and the last build date and can increment the version (`bundleVersion`).
- **Build info:** on every player build `OnBuildPreprocesses` (LoginScreen submodule, `BuildConfig/Editor/`) writes the version and the current date to `Assets/Resources/BuildInfo/BuildInfo.asset`, creating it if missing. The asset is generated and in `.gitignore`. `ShowBuildVersion` (prefab `BuildVersionData`: Profile view, Bootstrap, Reboot) loads it with `Resources.Load` and shows `build: <Application.version>` and `date: <build date>`, or `date: editor` when no build has been made yet.
- There is no CI and no command-line build script.

<!-- TODO: Android release process (store build, content in the production environment) is not settled yet. -->

## Environments

Issue [#83](https://github.com/DeMoZ/Chang/issues/83). Three tiers; a build is **promoted**, not rebuilt, between them.

| Env | Purpose | Web address (Cloudflare Pages) | Unity Cloud environment / CCD badge | Access |
|---|---|---|---|---|
| dev | development, Editor, any commit, may be broken | `dev.kruchaang.win` | `dev` / `latest` | private (Cloudflare Access) |
| staging | release candidate, checked before prod | `staging.kruchaang.win` | `staging` / `latest` | private (Cloudflare Access) |
| prod | what players see | `kruchaang.win` | `production` / `live` | public |

The domain `kruchaang.win`, Pages and Access are in the Cloudflare account demozbox@gmail.com. Production hosts are listed in `ProductionHosts` in `AppEnvironment.cs`.

### Unity Cloud terms

- **Environment** (`dev`, `staging`, `production`): a separate copy of the project's Unity Gaming Services data: CCD content, Cloud Save data, Remote Config. Player accounts (Authentication) are shared by all environments of the project; the data saved for a player is per environment.
- **Bucket**: a folder of content files in CCD inside one environment, one per platform (`WebGL`, `Android`, `StandaloneOSX`, `iOS`; the name is the build target name, Addressables looks the bucket up by it). Buckets with the same name in different environments are different buckets with different ids.
- **Release**: an immutable snapshot of a bucket (catalog + bundles). Every upload or promotion creates a new release.
- **Badge**: a label on one release; the game asks for "the release with badge X". `latest` is moved by CCD to the newest release automatically; `live` is moved by hand.
- **Promotion**: copying a release into a bucket of another environment.
- **Promotion only** bucket: content can get there only by promotion, no direct uploads or deletes. `staging` and `production` buckets are promotion only, so they contain exactly what was checked in the previous environment.

### How the client picks the environment

`Chang.Resources.AppEnvironment` (`Assets/Project/Scripts/Game/Resources/AppEnvironment.cs`) resolves the environment before the first scene loads:

- WebGL player: by the page hostname: `dev.*` → `dev`, `staging.*` → `staging`, a host from `ProductionHosts` → `production`;
- Editor: **Chang/Environment → Dev / Staging / Production** (saved per machine in EditorPrefs, applies on the next Play);
- anything else (localhost, Unity Play, Cloudflare's own `*.pages.dev` addresses, Android/iOS builds for now) → `dev`.

The environment is used for:

- **CCD content**: `Remote.LoadPath` is built from `{CcdManager.EnvironmentName}`, `{CcdManager.BucketId}` and `{CcdManager.Badge}`, evaluated at runtime. Addressables fills `CcdManager` from the content build only if it is not set yet, so `AppEnvironment` sets it first (bucket by platform, badge by environment). The bucket ids are in the same file;
- **Unity Gaming Services** (Cloud Save, Remote Config): `ProjectInstaller` passes `AppEnvironment.Current` to `LogInController`, which calls `InitializationOptions.SetEnvironmentName` before `UnityServices.InitializeAsync`.

The catalog and bundles are the same in every environment (the load path inside the catalog has the `{CcdManager.*}` placeholders), so a CCD release can be promoted from `dev` to `staging` to `production` as is.

In the Editor, CCD content is loaded only with the Addressables Play Mode Script **Use Existing Build**; with **Use Asset Database** (the default) the content comes from the project and the environment only affects Unity Gaming Services.

### WebGL build

**Chang/Build/WebGL Release** (or **Development**) sets the player settings the deploy relies on and builds to `Builds/WebGL/Chang`:

- template `PROJECT:ChangPWA` (`Assets/WebGLTemplates/ChangPWA`, a copy of Unity's PWA template);
- Brotli compression, no decompression fallback;
- file names as content hashes, so `Build/*` can be cached forever.

Template changes compared to Unity's PWA template:

- `ServiceWorker.js`: the cache name is unique per build and old caches are deleted; the page (`index.html`, manifest) is network-first; requests to other hosts (CCD catalog and bundles) are not cached by the Service Worker. Unity's original cached every request cache-first and never deleted old caches, so players could keep getting the old page and the old content catalog after a deploy.
- `_headers` (Cloudflare Pages): `Content-Encoding: br` for `Build/*.br`, long caching for `Build/*`, `no-cache` for the page, manifest and Service Worker.
- The page title is the product name. Unity 6's template has no "not supported on mobile" warning, so nothing to remove there.

### Deploy to Cloudflare Pages

One Pages project `kruchaang` (Direct Upload), one branch per environment: `main` (production), `dev`, `staging`. The "branch" is only a label given on upload, not a git branch. Each branch gets its own address of the project's domain: `kruchaang.win` → `main`, `dev.kruchaang.win` → `dev`, `staging.kruchaang.win` → `staging`. Deploy with Wrangler (needs Node.js; `npx` downloads Wrangler on first use):

```bash
npx wrangler pages deploy Builds/WebGL/Chang --project-name kruchaang --branch dev
```

Use `--branch staging` and `--branch main` for the other environments.

### Release flow

1. **Content**: with the WebGL build target, **Chang/Content/Build & Release to dev** (the same as Addressables Groups → Build → Build & Release; CCD `dev`, badge `latest`). Nothing to commit.
2. **Client**: increment the version in **DMZ/Build Data Window**, run **Chang/Build/WebGL Release**, deploy with `--branch dev`. Check on `dev.kruchaang.win`.
3. **Promote to staging**: **Chang/Content/Promote dev to staging** (or in Unity Cloud → CCD, promote the release from `dev`/`WebGL` to `staging`/`WebGL`; it gets `latest` there). Deploy **the same** `Builds/WebGL/Chang` folder with `--branch staging`. Check on `staging.kruchaang.win`.
4. **Promote to production**: **Chang/Content/Promote staging to production (live)** promotes the CCD release from `staging` to `production` and puts the `live` badge on it; deploy the same folder with `--branch main`.
5. **Rollback**: content — move the `live` badge back to the previous release in CCD; client — in Cloudflare (Workers & Pages → kruchaang → Deployments) use **Rollback** on the previous production deployment. No rebuild.

Keep the build folder (or a zip of it) until the release reaches production, because promotion means deploying the same files.

### One-time manual setup

Unity Cloud (project "Chang", done on 2026-10-18):

- environments `dev`, `staging`, `production` (project → Environments);
- in each environment the public buckets `WebGL`, `Android`, `StandaloneOSX`, `iOS` (bucket name = build target name). In `staging` and `production` they are **Promotion only**: content gets there only by promotion from `dev`. Their ids are in `AppEnvironment.cs`;
- a new bucket or environment: create it in Content Delivery (the Create Bucket dialog can create it in several environments at once), add its id to `AppEnvironment.cs`, and refresh **Addressables Profiles → Cloud Content Delivery** in the Editor.

Cloudflare (account demozbox@gmail.com, domain `kruchaang.win`). Done on 2026-10-19:

- Wrangler: `npx wrangler login` once on the build machine (choose the demozbox account in the browser). `npx wrangler whoami` shows the account.
- Pages project `kruchaang` (Direct Upload, production branch `main`): `npx wrangler pages project create kruchaang --production-branch main`.
- Custom domain of a branch (done for `dev` and `staging`). It needs a successful deploy of that branch first:
  1. Workers & Pages → kruchaang → Custom domains → **Set up a domain** → `dev.kruchaang.win` → Continue → Activate domain;
  2. kruchaang.win → DNS → Records → CNAME `dev` → Edit: target `kruchaang.pages.dev` → `dev.kruchaang.pages.dev`, Proxied (orange cloud) must stay on, otherwise the address shows `main`. See [Custom branch aliases](https://developers.cloudflare.com/pages/how-to/custom-branch-aliases/).
  
  For production (branch `main`) only step 1: done for `kruchaang.win` and `www.kruchaang.win` (both serve the game; `www` is in `ProductionHosts` too).
- Cloudflare Access (Zero Trust Free plan, login page `kruchaang.cloudflareaccess.com`, the team domain set in Zero Trust → Settings):
  - reusable policy **Testers** (Access controls → Policies): Allow, Include → Emails. Add a tester by adding their email there; they sign in with a one-time code sent to that email;
  - self-hosted application **KruChaang dev & staging** (Access controls → Applications) with the Testers policy for `dev.kruchaang.win`, `staging.kruchaang.win`, `dev.kruchaang.pages.dev`, `staging.kruchaang.pages.dev`;
  - Pages → kruchaang → Settings → **Preview access: Restrict previews**, which closes the per-deploy `<hash>.kruchaang.pages.dev` addresses.
- `kruchaang.pages.dev` (production on Cloudflare's own address) redirects to `kruchaang.win`, so players only see the own domain. It is a **Bulk Redirect** (account level, Delivery & performance → Bulk redirects): list `kruchaang_pages_dev` with one entry `kruchaang.pages.dev/` → `https://kruchaang.win/`, 301, Preserve query string, Subpath matching, Preserve path suffix, **without** Include subdomains (so `dev.`/`staging.kruchaang.pages.dev` keep working behind Access), and the rule "kruchaang.pages.dev to kruchaang.win" that enables the list. To stop the redirect, disable the rule.
- kruchaang.win → Caching → Configuration → **Browser Cache TTL: Respect Existing Headers**. The default (4 hours) replaced `no-cache` from `_headers` on `ServiceWorker.js` with `max-age=14400`.
- A new production host (another domain, `www.`) goes into `ProductionHosts` in `AppEnvironment.cs`.

### Content builds

Issue [#103](https://github.com/DeMoZ/Chang/issues/103).

- Remote groups (`Remote_*`) use bundle naming **Append Hash**: `<name>_<hash>.bundle`. The hash changes with the content, so an updated bundle gets a new URL and a CDN or browser cache can never serve the old one. Every content build can rename bundles, so the catalog and the bundles always go to CCD together.
- `CCDBuildData` is the folder the Addressables CCD **Automatic** profile builds into (the path is fixed in the Addressables package). **Build & Release** deletes it, builds, uploads its contents to the bucket of the active build target and creates a release. The folder is in `.gitignore`.
- Build artifacts are stored only in CCD: every release is an immutable snapshot of a bucket (catalog + bundles), promotion copies it to `staging` and `production`, rollback moves the badge back. Nothing is kept in git. `addressables_content_state.bin` is not needed: content is always built from scratch, "Update a Previous Build" is not used.
- The remote catalog is named after the player version (`catalog_<bundleVersion>.json`); the player build takes the catalog name from the last content build of its platform. Build the content before the player.
- **Chang/Content** (`Assets/Project/Scripts/Editor/Build/ContentRelease.cs`) works with the active build target: Build & Release to `dev`, and promotion of the release a badge points to into the next environment through the CCD Management API with the Editor's Unity account. Bucket ids and badges come from `AppEnvironment`. The menu items can be run from a terminal: `unity command menu --project-path . "Chang/Content/Build & Release to dev"` (the result is in the Editor console).
- Each platform has its own bucket, so after a change of the bundle settings run Build & Release once per platform (switch the build target first).

### Not done yet

- build number check (reload the page / open the store on mismatch): [#104](https://github.com/DeMoZ/Chang/issues/104);
- content environments for Android and the Editor: [#105](https://github.com/DeMoZ/Chang/issues/105).

## Main dependencies

| Package | Purpose |
|---|---|
| Zenject 9.2.0 | DI |
| UniTask | Async |
| Addressables 2.9.1, CCD Management 3.0.3 | Remote content |
| Authentication 3.6.0, Cloud Save 3.4.0 | Accounts and saves |
| DOTween / DOTweenPro | UI animation |
| Odin Inspector | Editor tools |
| SimpleLocalization | Localization |
| ProceduralUIImage | Rounded UI elements |
| Google.Apis.Sheets.v4, Google.Cloud.TextToSpeech.V1 (NuGet) | Editor content import and sound generation |

Installed but unused in code: Mobile Notifications, Adaptive Performance, OpenAI/Azure.AI.OpenAI. Analytics is disabled.

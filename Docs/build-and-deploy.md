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

- **WebGL** is the main public platform: project template `ChangPWA`, hosted on Cloudflare Pages (see [Environments](#environments)). [Unity Play](https://play.unity.com/en/games/efe1a90f-d700-4609-adfd-6481103f4d5b/chang) (`com.unity.connect.share`) is kept only for quick one-off sharing; there the game uses the content environment it was built with (`dev`).
- **Android**: min SDK 25. The keystore is in the `ChangExternal/AndroidKeystore` submodule.
- `Builds/` and `WebGL Builds/` are in `.gitignore`.

## What exists today

- **Content:** Addressables are built into the `CCDBuildData` submodule and uploaded to CCD (environment `dev`, badge `latest`). The Addressables profile is in CCD **Automatic** mode: the bucket is the one named after the build target (`WebGL`, `Android`, `StandaloneOSX`), so content of different platforms does not overwrite each other.
- **Client:** **Chang/Build/WebGL Release** and **Chang/Build/WebGL Development** (`Assets/Project/Scripts/Editor/Build/WebGLBuild.cs`) build WebGL to `Builds/WebGL/Chang`. **DMZ/Build Data Window** shows the version and the last build date and can increment the version (`bundleVersion`).
- **Build info:** on every player build `OnBuildPreprocesses` (LoginScreen submodule, `BuildConfig/Editor/`) writes the version and the current date to `Assets/Resources/BuildInfo/BuildInfo.asset`, creating it if missing. The asset is generated and in `.gitignore`. `ShowBuildVersion` (prefab `BuildVersionData`: Profile view, Bootstrap, Reboot) loads it with `Resources.Load` and shows `build: <Application.version>` and `date: <build date>`, or `date: editor` when no build has been made yet.
- There is no CI and no command-line build script.

<!-- TODO: Android release process (store build, content in the production environment) is not settled yet. -->

## Environments

Issue [#83](https://github.com/DeMoZ/Chang/issues/83). Three tiers; a build is **promoted**, not rebuilt, between them.

| Env | Purpose | Web address (Cloudflare Pages) | CCD environment / badge | Access |
|---|---|---|---|---|
| dev | any commit, may be broken | `dev.kruchaang.pages.dev` | `dev` / `latest` | private (Cloudflare Access) |
| staging | release candidate | `staging.kruchaang.pages.dev` | `staging` / `live` | private (Cloudflare Access) |
| prod | what players see | `kruchaang.pages.dev` | `production` / `live` | public |

A custom domain can replace `*.pages.dev` later: keep the `dev.` / `staging.` prefixes and add the production host to `ProductionHosts` in `CcdEnvironments.cs`.

### How the client picks the content environment

`Remote.LoadPath` is built from `{CcdManager.EnvironmentName}`, `{CcdManager.BucketId}` and `{CcdManager.Badge}`, which are evaluated at runtime. Addressables fills `CcdManager` from the build (the profile's `EnvironmentName`, i.e. `dev`) only if it is not set yet.

`Chang.Resources.CcdEnvironments` (`Assets/Project/Scripts/Game/Resources/CcdEnvironments.cs`) sets it before the first scene loads, in WebGL players only, from the page hostname:

- `dev.*` → `dev`, `staging.*` → `staging`, a host from `ProductionHosts` → `production`;
- anything else (localhost, Unity Play, a per-deploy `<hash>.kruchaang.pages.dev` address), the Editor and other platforms → the environment the content was built with.

The bucket ids of every environment and platform are in the same file. If a bucket id is empty, a warning is logged and the build environment is used.

The catalog and bundles are the same in every environment (the load path inside the catalog has the `{CcdManager.*}` placeholders), so a CCD release can be promoted from `dev` to `staging` to `production` as is.

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

One Pages project `kruchaang` (Direct Upload), one branch per environment. `main` is the production branch; `dev` and `staging` are preview branches with the stable aliases `dev.kruchaang.pages.dev` and `staging.kruchaang.pages.dev`. Deploy with Wrangler (needs Node.js; `npx` downloads Wrangler on first use):

```bash
npx wrangler pages deploy Builds/WebGL/Chang --project-name kruchaang --branch dev
```

Use `--branch staging` and `--branch main` for the other environments.

### Release flow

1. **Content**: build Addressables (WebGL target) and release to CCD `dev`, badge `latest`. Commit `CCDBuildData`.
2. **Client**: increment the version in **DMZ/Build Data Window**, run **Chang/Build/WebGL Release**, deploy with `--branch dev`. Check on `dev.kruchaang.pages.dev`.
3. **Promote to staging**: in Unity Cloud → CCD, promote the release from `dev`/`WebGL` to `staging`/`WebGL` and put the `live` badge on it. Deploy **the same** `Builds/WebGL/Chang` folder with `--branch staging`. Check on `staging.kruchaang.pages.dev`.
4. **Promote to production**: promote the CCD release from `staging` to `production` and move the `live` badge; deploy the same folder with `--branch main`.
5. **Rollback**: content — move the `live` badge back to the previous release in CCD; client — in Cloudflare (Workers & Pages → kruchaang → Deployments) use **Rollback** on the previous production deployment. No rebuild.

Keep the build folder (or a zip of it) until the release reaches production, because promotion means deploying the same files.

### One-time manual setup

Unity Cloud (project "Chang" → Content Delivery):

- create the environment `staging`;
- create the buckets `WebGL` and `Android` in `staging` and `WebGL` in `production` (bucket name = build target name), then put their ids into `CcdEnvironments.cs`;
- refresh **Addressables Profiles → Cloud Content Delivery** in the Editor so `ProfileDataSourceSettings.asset` lists the new environment and buckets.

Cloudflare (the same account as the other projects):

- `npx wrangler login` once on the build machine;
- create the project: `npx wrangler pages project create kruchaang --production-branch main`;
- the first `--branch dev` / `--branch staging` deploy creates the `dev.` / `staging.` aliases;
- protect dev and staging with Cloudflare Access: Pages project → Settings → General → **Enable access policy** for preview deployments (covers every non-production address, including `dev.` and `staging.`), then edit the created policy in Zero Trust → Access → Applications to list the testers' emails (one-time PIN login; free up to 50 users);
- optional custom domain: Pages project → Custom domains (`kruchaang.<domain>`, `dev.kruchaang.<domain>` pointing to the `dev` branch, and so on), then update `ProductionHosts` and this table.

### Not done yet

- hashed Addressables bundle names and build artifacts storage: [#103](https://github.com/DeMoZ/Chang/issues/103);
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

# Build and Deploy

## Project settings

| Setting | Value |
|---|---|
| Unity | 6000.3.12f1 |
| Product / Company | Chang / roman.kolychev |
| Version | `bundleVersion` in `ProjectSettings/ProjectSettings.asset` (currently 0.0.95) |
| Bundle id | `com.roman.kolychev.Chang` (Android, Standalone). iOS still has the template id. |
| Scenes in build | Bootstrap, Reboot, Game |
| Unity Cloud | project "Chang", organization "demozbox" |

## Platforms

- **WebGL** is the main public platform: PWA template, published on [Unity Play](https://play.unity.com/en/games/efe1a90f-d700-4609-adfd-6481103f4d5b/chang) through the `com.unity.connect.share` package.
- **Android**: min SDK 25. The keystore is in the `ChangExternal/AndroidKeystore` submodule.
- `Builds/` and `WebGL Builds/` are in `.gitignore`.

## What exists today

- **Content:** Addressables are built into the `CCDBuildData` submodule and uploaded to CCD (environment `dev`, badge `latest`).
- **Client:** **DMZ/Build Data Window** and **DMZ/Build WebGL** from the LoginScreen submodule make a Development WebGL build. `OnBuildPreprocesses` stamps the version and date from `Assets/Configs/OnBuildConfig.asset` into the build.
- There is no CI and no command-line build script.

<!-- TODO: the release process is not settled yet. Document it once it is (version bump, bundle upload, WebGL publishing, Android build). -->

The WebGL publishing problem is open; see [known-issues.md](known-issues.md#webgl-publishing).

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

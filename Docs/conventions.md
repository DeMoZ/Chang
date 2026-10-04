# Conventions

## Code

- Namespaces: `Chang.*` (`Chang.Core`, `Chang.Services`, `Chang.UI`, `Chang.GoogleSheets`…).
- Async methods are named `*Async`, return `UniTask` and take a `CancellationToken`. Owned CTSs are cancelled in `Exit` or `Dispose`.
- Logging goes through the alias `using Debug = DMZ.DebugSystem.DMZLogger;`. Message format: `$"[{nameof(Class)}] [{methodName}] ..."`.
- New services are bound in the right installer (`ProjectInstaller` or `GameInstaller`) as `AsSingle`.
- A new screen is a `*Controller` (C#, from DI) plus a `*View` (`MonoBehaviour` in `Game.unity`, wired through `[SerializeField]` in `GameInstaller`).

## Content and data rules

These rules are easy to break by accident, so they are collected here:

1. **Keys** for words and sentences: only Latin letters, digits and spaces; everything else becomes `_`. See [content-pipeline.md](content-pipeline.md#keys).
2. **Word sounds** have no silence at the start or end. See [content-pipeline.md](content-pipeline.md#word-sounds).
3. **Content and localization** are edited only in Google Sheets. Don't edit imported assets or `Localization/*.csv` by hand.
4. **The `Languages` enum**: new values only at the end.
5. **Section sorting is not repetition.** They are separate features. See [learning-logic.md](learning-logic.md#section-sorting-the-s-button).
6. **Secrets** (Google keys, keystore) live only in the private `ChangExternal` submodule. Never commit them to the main repository.

## Git

- The main branch is `main`. Features are developed in `feature/<Name>` branches.
- Tasks are tracked in the [GitHub Project](https://github.com/users/DeMoZ/projects/1).
- A behavior change comes with a docs update in the same commit or PR.

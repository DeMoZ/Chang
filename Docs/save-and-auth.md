# Profile, Saves and Authentication

## Player data (`Scripts/Profile/`)

- `PlayerProfile`:
  - `ProfileData`: `Name`, `Gender`, `Mascot` (`MascotLook`: an option index per mascot part, see [ui-design-system.md](ui-design-system.md#mascot)), `LearnLanguage` (Thai by default), `NativeLanguage`, `UnityCloudSavePlayerId`, `UtcTime`;
  - per language, `ProgressData<VocabularyQuestLog>` and `ProgressData<SentenceQuestLog>`, with a log per word or sentence key (see [learning-logic.md](learning-logic.md#mark)).

## Storage (`Scripts/Services/SaveLoad/`)

Both providers implement `IDataProvider` and serialize to JSON (Newtonsoft).

| Provider | Where |
|---|---|
| `PrefsDataProvider` | PlayerPrefs (local) |
| `UnityCloudDataProvider` | Unity Cloud Save (`CloudSaveService.Instance.Data.Player`). Retries up to 3 times on rate limiting. Errors lead to re-authentication or are shown in a popup. |

Save keys: `ProfileData`, `{Language}_VocabularyProgressData`, `{Language}_SentencesProgressData`.

## Sync (`ProfileService`)

`LoadStoredData`:

1. Without an active session, requests authentication.
2. Wipes local data that belongs to a different player (by player id).
3. Compares cloud and local data by `UtcTime` and keeps the newer copy. If the local copy is newer, pushes it to the cloud.

`SaveProgressAsync` runs after every answer and saves only the changed data to both stores.

In the Editor, the current PlayerPrefs data is shown in `Assets/Project/EditorCheckSaveLoad.asset` (`PrefsDataViewEditor`), which refreshes after every save.

> As of 2026-10 there is no production release, so old saves are not kept compatible and no migrations are written. Revisit this after launch.

## Authentication

`AuthorizationService` (`Scripts/Services/`) wraps `LogInController` from the `Assets/DMZ.Legacy.LoginScreen` submodule, which uses Unity Authentication. It supports:

- guest (anonymous) sign-in;
- username/password sign-in and sign-up;
- silent session restore from a token;
- sign-out and account deletion.

On logout or a lost session, `Game` sends the player back to the Reboot scene.

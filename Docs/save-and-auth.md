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
- guest upgrade: a guest adds a name and password or signs in to an existing account and keeps the progress (see below);
- silent session restore from a token;
- sign-out and account deletion.

On logout or a lost session, `Game` sends the player back to the Reboot scene.

## Guest upgrade (keep the guest progress)

A guest (anonymous player: no username and no external identity) sees a **Save my progress** card over the account card
(Profile → Log out). It opens the log in form in the upgrade mode (`LogInModel.IsUpgradingGuest`, Back returns to the account card):

- **Sign up** adds the name and password to the guest player (`AddUsernamePasswordAsync`). The player id stays, so all the
  data stays too, nothing is merged.
- **Log in** switches to an existing account. `ProfileService` first keeps all the guest data (the newer of cloud and local
  for the profile and every language) in PlayerPrefs as `PendingGuestData`. The SDK signs in only when signed out, so the guest
  is signed out keeping its session token; if the sign-in fails, the guest is signed in again with it and the pending data is
  dropped. On success the game reboots, and `LoadStoredData` merges the pending data into the account before loading.
  A failed merge keeps the pending data and is retried on the next load.

Merge rules (`PlayerDataMerge`), the account data is the base and is never replaced by a smaller progress:

- a word or sentence only the guest has is copied to the account;
- a key both have: the account log stays; the guest mark, success sequence and time are taken only when the guest answered
  later and its mark is not lower; the answers of both are merged by time and cut to `LOG_LIMIT`;
- profile: the account values stay, an empty name, `GenderType.No` and the default mascot are filled from the guest;
  the languages are the account ones;
- the merged data is saved to the cloud (`UnityCloudDataProvider.SaveAllAsync`, which throws on errors, unlike the other saves),
  the guest's local data is deleted, and the load takes the merged data from the cloud.

The guest anonymous account is left as it is (it can't be deleted without signing in to it).

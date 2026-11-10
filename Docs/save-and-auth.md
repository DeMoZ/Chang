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
- sign in with Google on WebGL and Android (see [Google](#google));
- guest upgrade: a guest adds a name and password or Google, or signs in to an existing account, and keeps the progress (see below);
- silent session restore from a token;
- sign-out and account deletion.

On logout or a lost session, `Game` sends the player back to the Reboot scene.

## Guest upgrade (keep the guest progress)

A guest (anonymous player: no username and no external identity) sees a card over the account card (Profile → Log out)
with **Continue with Google** and **Save my progress**.

**Continue with Google** (any external provider, see [External providers](#external-providers)):

- the Google identity is linked to the guest (`LinkWithGoogleAsync`): the player id stays, nothing is merged;
- if another player has this Google account already (`AccountAlreadyLinked`), the guest switches to that player exactly like
  **Log in** below (pending guest data, reboot, merge).

**Save my progress** opens the log in form in the upgrade mode (`LogInModel.IsUpgradingGuest`, Back returns to the account card):

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

## External providers

`LogInController` takes a list of `IExternalSignInProvider` (submodule `DMZ.Legacy.LoginScreen`), created in `ProjectInstaller.BindLogin`.
A provider has an `Id` (`"google"`), `IsSupported`, `RequestCredentialAsync` (the provider's own sign-in UI, returns an ID token
or `null` when cancelled) and `SignInAsync` / `LinkAsync` (the Unity Authentication calls). `LogInView` binds its provider buttons
by id (`_providerBtns` on the login screen, `_guestProviderBtns` in the guest card) and hides the buttons of unsupported providers.

- Signed out: the button signs in; Unity Authentication creates a new player when nobody has the identity.
- A signed-in guest: link, or switch and merge (see [Guest upgrade](#guest-upgrade-keep-the-guest-progress)).

Apple (#127) and Telegram (#128) are added as more providers and buttons the same way.

## Google

`GoogleSignInProvider` (`Scripts/Services/Auth/`) gets a Google ID token and calls `SignInWithGoogleAsync` / `LinkWithGoogleAsync`.
Unity Player Accounts is not used: it does not work on WebGL.

| Platform | How the ID token is taken |
|---|---|
| WebGL | `Plugins/WebGL/ChangGoogleSignIn.jslib`: a popup with Google's OpenID Connect page (`response_type=id_token`, `prompt=select_account`, random `state` and `nonce`). Google returns to `google-auth/` of the same site (`WebGLTemplates/ChangPWA/google-auth/index.html`), which sends the token back with a `BroadcastChannel` (or `window.opener`) and closes. |
| Android | `Plugins/Android/ChangGoogleSignIn.androidlib`: Credential Manager with `GetSignInWithGoogleOption` (the "Sign in with Google" account picker), the token is requested for the **Web** client ID. |
| Editor, iOS, other | not supported, the Google buttons are hidden. iOS comes with the iOS release. |

Why not Google Identity Services `google.accounts.id` on WebGL: `prompt()` (One Tap / FedCM) is not shown again for hours
after the player closes it once, so it can't back a button, and the GIS button is an iframe that would have to float as HTML
over the Unity canvas. The popup gives the same ID token. The game notices a closed popup when it gets the focus back and
no answer comes within 1.5 s (`popup.closed` can't be used: Google's page cuts the popup off with Cross-Origin-Opener-Policy).
A blocked popup is shown as an error.

Why Credential Manager on Android: the Google Sign-In for Unity plugin (`googlesamples/google-signin-unity`) is archived and
uses the deprecated `GoogleSignInClient`; Credential Manager is what Google recommends now and needs only a small Java bridge
and three Gradle dependencies (`androidx.credentials`, `credentials-play-services-auth`, `googleid`), no Unity package.

The client ID is in `Assets/Project/Resources/GoogleSignInConfig.asset` (`WebClientId`). It is public, not a secret (every page
with Google sign-in shows it); there is no client secret anywhere. Empty: the Google buttons are hidden.

### Owner setup

1. **Google Cloud project** (console.cloud.google.com, one project for all environments): APIs & Services → **OAuth consent screen**
   (Google Auth Platform → Branding): app name KruChaang, support email, logo optional; authorized domain `kruchaang.win`;
   Audience: External; scopes: `openid`, `email`, `profile` only (no verification needed for them). Publish the app
   (Audience → Publish app), otherwise only the test users can sign in.
2. **Web client**: Clients → Create client → Web application, name `KruChaang Web`.
   - Authorized JavaScript origins: `https://kruchaang.win`, `https://www.kruchaang.win`, `https://staging.kruchaang.win`, `https://dev.kruchaang.win`
     (for a local test also `http://localhost:<port>`).
   - Authorized redirect URIs: the same hosts with `/google-auth/`: `https://kruchaang.win/google-auth/`, `https://www.kruchaang.win/google-auth/`,
     `https://staging.kruchaang.win/google-auth/`, `https://dev.kruchaang.win/google-auth/` (local: `http://localhost:<port>/google-auth/`).
   - Copy the **Client ID** (`….apps.googleusercontent.com`) into `GoogleSignInConfig.asset` → Web Client Id. The client secret is not needed, don't copy it anywhere.
3. **Android client**: Clients → Create client → Android, package name `com.roman.kolychev.Chang`, SHA-1 of the signing key
   (`keytool -list -v -keystore <keystore> -alias <alias>`, it asks for the keystore password; the SHA-1 is not a secret).
   Add one Android client per signing key: the upload/developer key, and later the Google Play app signing key
   (Play Console → Setup → App signing). Its client ID is not used in code.
4. **Unity Dashboard** → the project → Player Authentication → Identity providers → Add → **Google**: the Web Client ID from step 2.
   Accounts are shared by all environments, so this is done once.


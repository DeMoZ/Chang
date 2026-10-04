# Getting Started

## Requirements

- **Unity 6000.3.12f1** (`ProjectSettings/ProjectVersion.txt`).
- SSH access to the private submodule repositories on GitHub.
- For the editor content tools:
  - Python 3 and `brew install cairo` to generate word images (`Tools/WordImages`);
  - Google Cloud keys from the `ChangExternal` submodule to import Google Sheets and generate sounds (TTS).

## Cloning

```bash
git clone --recurse-submodules git@github.com:DeMoZ/Chang.git
```

If the repository was cloned without submodules:

```bash
git submodule update --init --recursive
```

| Submodule | Path | Contents |
|---|---|---|
| `DeMoZ/ChangExternal` (private) | `Assets/Project/Scripts/Utilities/ChangExternal` | Google API keys, spreadsheet ids, Android keystore. **Secrets; never copy them elsewhere.** |
| `DeMoZ/DMZ.Legacy.LoginScreen` | `Assets/DMZ.Legacy.LoginScreen` | Login screen, Unity Authentication, build window |
| `DeMoZ/ChangBundlesBuilds` | `CCDBuildData` | Built Addressables bundles for Unity CCD |

## First run

1. Open the project in Unity Hub.
2. Open `Assets/Project/Scenes/Bootstrap.unity`. The game always starts from this scene: Bootstrap → Reboot → Game (see [architecture.md](architecture.md#startup-and-scenes)).
3. Press Play. On the first run the `Base` label bundles are downloaded from CCD, then you sign in (as a guest or with a login).

Useful in the Editor:

- **Space** in Play Mode restarts the game through the Reboot scene (`EditorRestartTrigger`), unless an input field has focus.
- `Assets/Project/EditorCheckSaveLoad.asset` shows the current player data from PlayerPrefs. It refreshes after every save.
- **Chang/Content/Addressables/Addressables Resources Window** clears the downloaded bundle cache.

`UITest.unity` is a sandbox for UI and popups (`PopupTester`). It is not part of the build.

## Repository layout

```
Assets/
  Project/
    Scenes/              Bootstrap, Reboot, Game, UITest
    Scripts/             all game code (no asmdef, everything compiles into Assembly-CSharp)
      Game/              FSMs, screens, Core model, Zenject installers
      Services/          profile, auth, repetition, localization, content loading
      Profile/           player progress data
      Google/            ScriptableObject classes imported from Google Sheets
      Utilities/Editor/  editor tools (Sheets, sounds, localization)
      UI/                popups and shared UI components
    Resources_Bundled/   content that goes into Addressables (book configs, ImageWords, SoundWords)
    Configs/             editor import configs
  Resources/             LanguagesConfig, RepetitionConfig, Localization/*.csv, ProjectContext
  Plugins/               Zenject, DOTween, Odin, SimpleLocalization, ProceduralUIImage
  AddressableAssetsData/ Addressables and CCD settings
Docs/                    this documentation
Tools/WordImages/        Python generators for word images
UML/                     draw.io diagrams
CCDBuildData/            submodule with built bundles
```

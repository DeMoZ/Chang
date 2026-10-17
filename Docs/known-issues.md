# Known Issues and Tech Debt

Found while writing the docs (2026-10-04). Remove an item once it is fixed. When an item is taken into work, move it to a task in the [GitHub Project](https://github.com/users/DeMoZ/projects/1).

## WebGL publishing

Publishing the WebGL version has a problem that is planned to be investigated. Record the symptoms, cause and fix here (or in an ADR) once it is understood.

Likely causes found and fixed in #83 (check after the first Cloudflare deploys, then remove this section):

- Unity's PWA Service Worker cached every request (the page and the CCD catalog too) cache-first and never deleted old caches, so a new deploy or new content could stay invisible. Fixed in the `ChangPWA` template.
- The old **DMZ/Build WebGL** menu renamed the files in the build root (`ServiceWorker.js`, `manifest.webmanifest`), which broke the PWA. Replaced by **Chang/Build/WebGL**.

See [build-and-deploy.md](build-and-deploy.md#environments).

## Likely bugs

- **Hint on the sentence page.** `PagesState` enables the Hint button on every page, but `SentenceSelectWordView.ShowHint()` throws `NotImplementedException`.
- **`ScreenManager`** hides the page controllers at startup but does not register `SentenceSelectWordController`.
- **`SentenceSelectWordState`**: the question direction is hard-coded (`isQuestInTranslation = false // todo`).

## Dead or unfinished code

- Tests: `Scripts/Tests/Tests.cs` is not NUnit and is never called. There are effectively no automated tests.
- `LoadBootstrapScene.cs` is not used in any scene.
- Empty folders: `Game/PreloaderScreen`, `FSM/GameFSMStates/Vocabulary`, `FSM/GameFSMStates/Sentences`, `Assets/Project/Configs/BookConfigs/Thai`.
- `PhraseData.cs`, `PhraseConfig.cs`, `LanguageBankConfig`, `PromptsBank` (`ConfigsDepricated`) are leftovers of an older approach.
- The constant `MIX_WORDS_AMOUNT_IN_LEARN_SELECT_WORD_PAGE` is declared but unused.
- Unused packages: Mobile Notifications, Adaptive Performance, OpenAI/Azure.AI.OpenAI.

## Other

- `GameOverlayController` has a TODO to move the overlay to an FSM.
- The iOS bundle id is still the template one.
- Typos in names: `MARK_DICREMENT`, `ConfigsDepricated`, `LocalizatioinViewer`.
- No CI.

# Localization

The project uses the **SimpleLocalization** plugin (`Assets/Plugins/SimpleLocalization`).

## Source of truth: the Google Sheet

- The spreadsheet and its sheets are set in `Assets/Resources/LocalizationSettings.asset`: the `Lobby` sheet (UI) and the `*.V` and `*.S` content sheets.
- The **Download Sheets** button in the plugin settings downloads the sheets as CSV into `Assets/Resources/Localization/*.csv`.

> **Rule.** Don't edit the CSV files by hand: the next Download Sheets overwrites them.
> Add new keys to the Google Sheet, then download the sheets again.

CSV format:

```
Key,English,Russian,German,French,ChineseSimplified,ChineseTraditional,Malay,Indonesian,Korean,Japanese,Lao,Vietnamese
```

- UI keys are dot-separated: `Lobby.Tab.Words`.
- Content keys are the `WordKey` and `SentenceKey`: `Thai/Vocabulary/Food/Fried rice`.
- Sentence translations contain `{0}` placeholders.

## At runtime

- `LocalizationService` (`Scripts/Services/`) uses the device language at startup and `ProfileData.NativeLanguage` once the profile is loaded. If the CSV has no column for that language, it falls back to English.
- `LocalizationService.Localize(key, fallback)` returns the translation in the current language, else the English one, else the fallback. It is called by `Word.Translation` and `Sentence.GetTranslation` (fallback: `DefaultTranslation` from the content sheet) and by the views for formatted texts.
- UI texts are translated by the `LocalizedTMPText` component; its fallback is the text the prefab has, so a key that is not in the sheet yet shows the design text.
- The available UI languages are listed in `Assets/Project/Resources/LanguagesConfig.asset`: display name, `SystemLanguage` mapping and the default language.

## The `Languages` enum

Defined in `Scripts/ProjectEnums.cs`.

> **Rule.** Add new values **only at the end**. The enum is serialized as an `int` in the content ScriptableObjects (Word, Sentence, Vocabulary, books), so reordering it breaks the content until it is re-imported from Google Sheets.
> Value names must match the CSV column names.

## Viewing CSVs in the Editor

**Chang/Utilities/Localization/Create Sheet View** creates a `LocalizationSheetView` asset. It shows the keys as a tree and reports duplicates and missing columns. Existing viewer assets are in `Assets/Project/Configs/LocalizatioinViewer/`.

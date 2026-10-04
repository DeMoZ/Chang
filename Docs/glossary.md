# Glossary

| Term | Meaning |
|---|---|
| **Book** | A set of sections with lessons. There is a word book (`VocabularyBook`) and a sentence book (`SentencesBook`). |
| **Section** | A themed group of lessons, e.g. Food or Beverages. |
| **Lesson** | A list of keys played in one go. At runtime, a queue of questions. |
| **Key** | Path-like id of a word or sentence: `Thai/Vocabulary/Food/Fried rice`. Also the localization key and the asset path. |
| **Mark** | How well a word or sentence is known, from 0 to 10. |
| **Log** (progress log) | Per-key record: mark, success streak, time, recent answers. |
| **Quest / Question** | A lesson question: `QuestSelectWord`, `QuestMatchWords`, `SentenceSelectWords`. |
| **Page** | A screen inside a lesson: Demonstration, SelectWord, MatchWords, SentenceSelectWord, PlayResult. |
| **Demonstration** | Introduces a new word before the first question about it. |
| **Phonetics** | Latin transliteration of a Thai word. |
| **Hint** | Shows phonetics. On SelectWord it cancels the mark increment. |
| **Repetition** | A lesson made of due keys, following a spaced-repetition schedule. |
| **Sort** ("S") | Redistributes a section's keys across its lessons by mark. Not repetition. |
| **Modifier** (M/R/G) | Flags of a word in a sentence: MixFiller, Replaceable, Gender. |
| **Bus** | A shared data object: `MainScreenBus`, `GameBus`, `PagesBus`. |
| **CCD** | Unity Cloud Content Delivery, where content bundles are downloaded from. |

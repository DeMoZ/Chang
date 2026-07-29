using System.Collections.Generic;
using System.Linq;

namespace Chang.Core
{
    public static class GoogleSheetsToCore
    {
        public static VocabularyBook GetVocabularyBook(GoogleSheets.VocabularyBook book)
        {
            List<VocabularyBookSection> coreSections = new();

            foreach (var section in book.Sections)
            {
                List<Lesson> coreLessons = section.Lessons
                    .Select(lesson => new Lesson(lesson.Language, lesson.Section, lesson.Keys))
                    .ToList();

                VocabularyBookSection coreSection = new VocabularyBookSection
                {
                    Language = section.Language,
                    Section = section.Section,
                    SectionKey = section.SectionKey,
                    Lessons = coreLessons,
                };

                coreSections.Add(coreSection);
            }

            return new VocabularyBook(book.Language, coreSections);
        }

        public static SentencesBook GetSentencesBook(GoogleSheets.SentencesBook book)
        {
            List<SentencesBookSection> coreSections = new();

            foreach (var section in book.Sections)
            {
                List<Lesson> coreLessons = section.SectionLessons
                    .Select(lesson => new Lesson(lesson.Language, lesson.Section, lesson.Keys))
                    .ToList();

                SentencesBookSection coreSection = new()
                {
                    Language = section.Language,
                    Section = section.Section,
                    SectionKey = section.SectionKey,
                    SectionLessons = coreLessons
                };
                coreSections.Add(coreSection);
            }

            return new SentencesBook(book.Language, coreSections);
        }

        public static List<Sentence> GetSentences(List<GoogleSheets.Sentence> itemSentences)
        {
            return itemSentences.Select(sentence => new Sentence
            {
                Language = sentence.Language,
                Section = sentence.Section,
                Key = sentence.Key,
                SentenceKey = sentence.SentenceKey,
                ImageKey = sentence.ImageKey,
                SoundKey = sentence.SoundKey,
                DefaultTranslation = sentence.DefaultTranslation,
                SentenceWords = GetSentenceWords(sentence.SentenceWords),
            }).ToList();
        }

        private static List<SentenceWord> GetSentenceWords(List<GoogleSheets.SentenceWord> sentenceSentenceWords)
        {
            return sentenceSentenceWords.Select(word => new SentenceWord
            {
                WordKey = word.WordKey,
                Modifiers = word.Modifiers
            }).ToList();
        }
    }
}
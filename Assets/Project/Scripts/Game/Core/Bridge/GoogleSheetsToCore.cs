using System.Collections.Generic;
using System.Linq;

namespace Chang.Core
{
    public static class GoogleSheetsToCore
    {
        public static VocabularyBook GetVocabularyBook(GoogleSheets.VocabularyBook book)
        {
            List<VocabularyBookSection> coreSections = new List<VocabularyBookSection>();
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
    }
}
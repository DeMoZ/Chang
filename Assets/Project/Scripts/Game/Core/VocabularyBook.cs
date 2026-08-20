using System.Collections.Generic;
using System.Linq;

namespace Chang.Core
{
    public class VocabularyBook
    {
        public Languages Language;
        public List<VocabularySection> Sections;

        public VocabularyBook(Languages language, List<VocabularySection> sections)
        {
            Language = language;
            Sections = sections;

            PopulateQuestions();
        }

        private void PopulateQuestions()
        {
            // add lessons questions
            foreach (VocabularySection section in Sections)
            {
                foreach (Lesson lesson in section.Lessons)
                {
                    List<IQuestion> questions = new List<IQuestion>();

                    foreach (var key in lesson.Keys)
                    {
                        IQuestion question = new QuestSelectWord
                        {
                            Key = key,
                            WordsKeys = lesson.Keys.Where(k => !k.Equals(key)).ToHashSet(),
                            SectionKey = section.SectionKey,
                            // Language = lesson.Language
                        };

                        questions.Add(question);
                    }

                    lesson.SetQuestions(questions);
                }
            }
        }
    }
}
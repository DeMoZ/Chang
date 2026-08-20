using System.Collections.Generic;

namespace Chang.Core
{
    public class SentencesBook
    {
        public Languages Language;
        public List<SentencesSection> Sections;

        public SentencesBook(Languages language, List<SentencesSection> sections)
        {
            Language = language;
            Sections = sections;

            PopulateQuestions();
        }

        private void PopulateQuestions()
        {
            // add lessons questions
            foreach (SentencesSection section in Sections)
            {
                foreach (Lesson lesson in section.SectionLessons)
                {
                    List<IQuestion> questions = new List<IQuestion>();

                    foreach (var sentenceKey in lesson.Keys)
                    {
                        IQuestion question = new SentenceSelectWords
                        {
                            Key = sentenceKey,
                        };

                        questions.Add(question);
                    }

                    lesson.SetQuestions(questions);
                }
            }
        }
    }
}
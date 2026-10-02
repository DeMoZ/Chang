using System.Collections.Generic;

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

            foreach (VocabularySection section in Sections)
            {
                section.PopulateQuestions();
            }
        }
    }
}
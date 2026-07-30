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
        }
    }
}
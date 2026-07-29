using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.Core
{
    public class SentencesBook : SerializedScriptableObject
    {
        public Languages Language;
        public List<SentencesBookSection> Sections;

        public SentencesBook(Languages language, List<SentencesBookSection> sections)
        {
            Language = language;
            Sections = sections;
        }
    }
}
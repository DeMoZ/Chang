using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.GoogleSheets
{
    public class VocabularyBook : SerializedScriptableObject
    {
        public Languages Language;
        public List<VocabularyBookSection> Sections;
    }
}
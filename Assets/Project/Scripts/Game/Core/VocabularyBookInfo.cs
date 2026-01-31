using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.Core
{
    public class VocabularyBookInfo : SerializedScriptableObject
    {
        public Languages Language;
        public List<VocabularyBookSection> Sections;
    }
}
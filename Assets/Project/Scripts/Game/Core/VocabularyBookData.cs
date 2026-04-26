using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.Core
{
    public class VocabularyBookData : SerializedScriptableObject
    {
        public Languages Language;
        public List<VocabularyBookSection> Sections;
    }
}
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.Core
{
    public class VocabularyInfo : SerializedScriptableObject
    {
        public Languages Language;
        public List<Word> Words;
    }
}
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.Core
{
    public class SentencesBookData : SerializedScriptableObject
    {
        public Languages Language;
        public List<SentencesBookSection> Sections;
    }
}
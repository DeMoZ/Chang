using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.Core
{
    public class SentencesBookInfo : SerializedScriptableObject
    {
        public Languages Language;
        public List<SentencesBookSection> Sections;
    }
}
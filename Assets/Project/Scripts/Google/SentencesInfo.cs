using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Chang.GoogleSheets
{
    public class SentencesInfo : SerializedScriptableObject
    {
        public Languages Language;
        public List<Sentence> Sentences;
    }
}
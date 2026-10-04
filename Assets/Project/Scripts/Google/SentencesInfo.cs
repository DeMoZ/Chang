using System.Collections.Generic;
using UnityEngine;

namespace Chang.GoogleSheets
{
    public class SentencesInfo : ScriptableObject
    {
        public Languages Language;
        public List<Sentence> Sentences;
    }
}
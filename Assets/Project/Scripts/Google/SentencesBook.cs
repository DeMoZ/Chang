using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang.GoogleSheets
{
    public class SentencesBook : SerializedScriptableObject
    {
        public Languages Language;
        public List<SentencesBookSection> Sections;
    }
}
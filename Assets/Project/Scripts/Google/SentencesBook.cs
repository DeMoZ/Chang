using System.Collections.Generic;
using UnityEngine;

namespace Chang.GoogleSheets
{
    public class SentencesBook : ScriptableObject
    {
        public Languages Language;
        public List<SentencesBookSection> Sections;
    }
}
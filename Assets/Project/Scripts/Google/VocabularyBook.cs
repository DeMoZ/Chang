using System.Collections.Generic;
using UnityEngine;

namespace Chang.GoogleSheets
{
    public class VocabularyBook : ScriptableObject
    {
        public Languages Language;
        public List<VocabularyBookSection> Sections;
    }
}
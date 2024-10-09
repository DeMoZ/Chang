using System.Collections.Generic;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/LanguageBank Config", fileName = "WordConfig")]
    public class LanguageBankConfig : ScriptableObject
    {
        public List<PhraseConfig> Key;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/Phrase Config", fileName = "WordConfig")]
    public class PhraseConfig : ScriptableObject
    {
        [field: SerializeField] public string Key { get; set; } = string.Empty;
        /*[field: SerializeField]*/
        public Languages Language { get; set; } = Languages.Thai;
        [field: SerializeField] public AudioClip AudioClip { get; set; }
        [field: SerializeField] public Sprite Sprite { get; set; }
        [field: SerializeField] public WordConfig Word { get; set; }
    }

    [Serializable]
    public class WordConfig
    {
        //[field: SerializeField] public string Name = string.Empty;
        /// <summary>
        /// Do not use this field
        /// </summary>
        [Tooltip("Only to show in the inspector")]
        [field: SerializeField]
        public string EngWord { get; set; } = string.Empty;

        [field: SerializeField] public string Word { get; set; } = string.Empty;
        [field: SerializeField] public string Phonetic { get; set; } = string.Empty;
        [field: SerializeField] public List<Translation> Meanings { get; set; }

        public string GetTranslation()
        {
            return Meanings.FirstOrDefault(t => t.Language == Languages.English)?.Meaning;
        } 
    }

    [Serializable]
    public class Translation
    {
        [field: SerializeField] public Languages Language { get; set; }
        [field: SerializeField] public string Meaning { get; set; }
    }
}
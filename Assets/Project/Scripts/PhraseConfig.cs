using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/Phrase Config", fileName = "PhraseConfig")]
    public class PhraseConfig : ScriptableObject
    {
        [field: SerializeField] public string Key { get; set; } = string.Empty;
        [field: SerializeField] public Languages Language { get; set; } = Languages.English;
        [field: SerializeField] public string Section { get; set; } = string.Empty;
        [field: SerializeField] public Sprite Sprite { get; set; }
        [field: SerializeField] public WordConfig Word { get; set; }

        public AudioClip AudioClip { get; set; }
        public PhraseData PhraseData => new(Key, AudioClip, Sprite, Word.WordData);
    }

    [Serializable]
    public class WordConfig
    {
        [field: SerializeField] public string Section { get; set; } = string.Empty;
        
        /// <summary>
        /// Do not use this field
        /// </summary>
        [Tooltip("Only to show in the inspector")]
        [field: SerializeField]
        public string Key { get; set; } = string.Empty;

        [field: SerializeField] public string Word { get; set; } = string.Empty;
        [field: SerializeField] public string Phonetic { get; set; } = string.Empty;
        [field: SerializeField] public List<Translation> Meanings { get; set; }

        public WordData WordData => new(Section, Key, Word, Phonetic, Meanings);
    }

    [Serializable]
    public class Translation
    {
        [field: SerializeField] public Languages Language { get; set; }
        [field: SerializeField] public string Meaning { get; set; }
    }
}
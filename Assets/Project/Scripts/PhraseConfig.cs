using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/Phrase Config", fileName = "PhraseConfig")]
    public class PhraseConfig : ScriptableObject
    {
        [field: SerializeField] public string Key { get; set; } = string.Empty;

        /*[field: SerializeField]*/
        public Languages Language { get; set; } = Languages.Thai;
        [field: SerializeField] public AudioClip AudioClip { get; set; }
        [field: SerializeField] public Sprite Sprite { get; set; }
        [field: SerializeField] public WordConfig Word { get; set; }

        public PhraseData PhraseData => new(Key, AudioClip, Sprite, Word.WordData);
        
        /// <summary>
        /// DEPRICATED. Very temp solution. Take Key From Phrase config and put into Word.Key
        /// </summary>
        // [Button]
        public void FixWordDataKey()
        {
           Word.Key = Key;
        }
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
        public string Key { get; set; } = string.Empty;

        [field: SerializeField] public string Word { get; set; } = string.Empty;
        [field: SerializeField] public string Phonetic { get; set; } = string.Empty;
        [field: SerializeField] public List<Translation> Meanings { get; set; }

        public WordData WordData => new(Key, Word, Phonetic, Meanings);
    }

    [Serializable]
    public class Translation
    {
        [field: SerializeField] public Languages Language { get; set; }
        [field: SerializeField] public string Meaning { get; set; }
    }
}
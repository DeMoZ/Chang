using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chang
{
    public class PhraseData
    {
        public string Key { get; private set; }
        public Languages Language { get; private set; } = Languages.Thai;
        public AudioClip AudioClip { get; private set; }
        public Sprite Sprite { get; private set; }
        public WordData Word { get; private set; }

        public PhraseData(string key, /*Languages language,*/ AudioClip audioClip, Sprite sprite, WordData word)
        {
            Key = key;
            //Language = language;
            AudioClip = audioClip;
            Sprite = sprite;
            Word = word;
        }
    }

    public class WordData
    {
        public string EngWord { get; private set; }
        public string Word { get; private set; }
        public string Phonetic { get; private set; }
        public List<Translation> Meanings { get; private set; }

        public WordData(string engWord, string word, string phonetic, List<Translation> meanings)
        {
            EngWord = engWord;
            Word = word;
            Phonetic = phonetic;
            Meanings = meanings;
        }

        public string GetTranslation()
        {
            return Meanings.FirstOrDefault(t => t.Language == Languages.English)?.Meaning;
        }
    }
}
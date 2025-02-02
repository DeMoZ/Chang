using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chang
{
    public class PhraseData
    {
        public string Key { get; private set; }
        public Languages Language { get; private set; } = Languages.Thai;
        public WordData Word { get; private set; }
        public bool ShowPhonetics { get; private set; }
        public AudioClip AudioClip { get; private set; }
        public Sprite Sprite { get; private set; }
        
        public PhraseData(string key, /*Languages language,*/ AudioClip audioClip, Sprite sprite, WordData word)
        {
            Key = key;
            //Language = language;
            AudioClip = audioClip;
            Sprite = sprite;
            Word = word;
        }

        public void SetPhonetics(bool showPhonetics)
        {
            ShowPhonetics = showPhonetics;
        }
        
        public override string ToString()
        {
            return $"key: {Key}; language: {Language}; word: {Word.LearnWord}; phonetic: {ShowPhonetics}"; //  audioclip: {AudioClip?.name}; sprite: {Sprite?.name}
        }
    }

    public class WordData
    {
        public string Key { get; private set; }
        public string LearnWord { get; private set; }
        public string Phonetic { get; private set; }
        public List<Translation> Meanings { get; private set; }

        public WordData(string key, string learnWord, string phonetic, List<Translation> meanings)
        {
            Key = key;
            LearnWord = learnWord;
            Phonetic = phonetic;
            Meanings = meanings;
        }

        public string GetTranslation()
        {
            return Meanings.FirstOrDefault(t => t.Language == Languages.English)?.Meaning;
        }
    }
}
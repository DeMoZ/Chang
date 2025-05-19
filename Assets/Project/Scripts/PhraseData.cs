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
        public string LogKey => $"{Language}/{Word.LogKey}";
        
        public PhraseData(string key, /*Languages language,*/WordData word)
        {
            Key = key;
            //Language = language;
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
        public string Section { get; private set; }
        public string Key { get; private set; }
        public string LearnWord { get; private set; }
        public string Phonetic { get; private set; }
        public List<Translation> Meanings { get; private set; }
        public bool ShowPhonetics { get; private set; }
        public AudioClip AudioClip { get; set; } 
        public string LogKey => $"Words/{Section}/{Key}";
        public string Translation => Meanings.FirstOrDefault(t => t.Language == Languages.English)?.Meaning;
        
        public WordData(string section, string key, string learnWord, string phonetic, List<Translation> meanings)
        {
            Section = section;
            Key = key;
            LearnWord = learnWord;
            Phonetic = phonetic;
            Meanings = meanings;
        }
        
        public WordData(string section, string key, string learnWord, string phonetic, List<Translation> meanings, bool showPhonetics)
        {
            Section = section;
            Key = key;
            LearnWord = learnWord;
            Phonetic = phonetic;
            Meanings = meanings;
            ShowPhonetics = showPhonetics;
        }

        public void SetShowPhonetics(bool showPhonetics)
        {
            ShowPhonetics = showPhonetics;
        }
    }
}
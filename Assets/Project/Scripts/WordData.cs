using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chang
{
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
using System;
using System.Collections.Generic;
using Chang.Core;

namespace Chang.Profile
{
    public class PlayerProfile : IDisposable
    {
        public readonly Dictionary<Languages, ProgressData<VocabularyQuestLog>> VocabularyProgressDict = new();
        public readonly Dictionary<Languages, ProgressData<SentenceQuestLog>> SentencesProgressDict = new();

        public ProfileData ProfileData = new ProfileData();

        /// <summary>
        /// key Thai/Fruits, value section
        /// </summary>
        public Dictionary<string, VocabularyBookSection> ReorderedVocabularySections { get; } = new();

        public Dictionary<string, SentencesBookSection> ReorderedSentencesSections { get; } = new();

        public ProgressData<VocabularyQuestLog> VocabularyProgress => VocabularyProgressDict[ProfileData.LearnLanguage];
        public ProgressData<SentenceQuestLog> SentencesProgress => SentencesProgressDict[ProfileData.LearnLanguage];

        public void Dispose()
        {
        }

        /// <summary>
        /// initialize with saved data from Prefs/Remote etc.
        /// </summary>
        public void Init()
        {
        }

        public void AddReorderVocabularySection(string key, VocabularyBookSection sectionData)
        {
            ReorderedVocabularySections[key] = sectionData;
        }

        public void AddReorderSentencesSection(string key, SentencesBookSection sectionData)
        {
            ReorderedSentencesSections[key] = sectionData;
        }
    }
}
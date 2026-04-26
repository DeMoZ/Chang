using System;
using System.Collections.Generic;

namespace Chang.Profile
{
    public class PlayerProfile : IDisposable
    {
        public readonly Dictionary<Languages, ProgressData<VocabularyQuestLog>> VocabularyProgressDict = new();
        public readonly Dictionary<Languages, ProgressData<SentenceQuestLog>> SentencesProgressDict = new();

        public ProfileData ProfileData;

        /// <summary>
        /// key Thai/Fruits, value section
        /// </summary>
        public Dictionary<string, Vocabulary.SectionData> ReorderedVocabularySections { get; } = new();

        public Dictionary<string, Sentences.SectionData> ReorderedSentencesSections { get; } = new();

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

        public void AddReorderVocabularySection(string key, Vocabulary.SectionData sectionData)
        {
            ReorderedVocabularySections[key] = sectionData;
        }

        public void AddReorderSentencesSection(string key, Sentences.Deprecated.SectionData sectionData)
        {
            ReorderedSentencesSections[key] = sectionData;
        }
    }
}
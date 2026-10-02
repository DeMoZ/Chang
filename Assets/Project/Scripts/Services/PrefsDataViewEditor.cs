using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Profile;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Services.DataProvider
{
    /// <summary>
    /// Editor only view of the data stored in PlayerPrefs, for the visual control and testing
    /// </summary>
    [CreateAssetMenu(menuName = "Chang/Services/Prefs Data View", fileName = "PrefsDataView")]
    public class PrefsDataViewEditor : ScriptableObject
    {
        public string PlayerId;
        public string Name;
        public GenderType Gender;
        public Languages LearnLanguage;
        public string ProfileUtcTime;
        public string VocabularyUtcTime;
        public string SentencesUtcTime;

        [Tooltip("Sorted by the last answer time, the latest is the first")]
        public List<QuestLogView> Vocabulary = new();
        [Tooltip("Sorted by the last answer time, the latest is the first")]
        public List<QuestLogView> Sentences = new();

        public JsonView ProfileJson;
        public JsonView VocabularyJson;
        public JsonView SentencesJson;

        public void Refresh(Languages language)
        {
            ProfileJson.Json = PlayerPrefs.GetString(DataProviderConstants.ProfileDataKey, string.Empty);
            VocabularyJson.Json = PlayerPrefs.GetString(DataProviderConstants.VocabularyProgressKey(language), string.Empty);
            SentencesJson.Json = PlayerPrefs.GetString(DataProviderConstants.SentencesProgressKey(language), string.Empty);

            ProfileData profile = Deserialize<ProfileData>(ProfileJson.Json);
            PlayerId = profile?.UnityCloudSavePlayerId;
            Name = profile?.Name;
            Gender = profile?.Gender ?? GenderType.No;
            LearnLanguage = language;
            ProfileUtcTime = profile?.UtcTime.ToString("O");

            ProgressData<VocabularyQuestLog> vocabulary = Deserialize<ProgressData<VocabularyQuestLog>>(VocabularyJson.Json);
            VocabularyUtcTime = vocabulary?.UtcTime.ToString("O");
            Vocabulary = ToViews(vocabulary, log => new QuestLogView(log.FileName, log.Presentation, log.Mark, log.SuccessSequence, log.UtcTime));

            ProgressData<SentenceQuestLog> sentences = Deserialize<ProgressData<SentenceQuestLog>>(SentencesJson.Json);
            SentencesUtcTime = sentences?.UtcTime.ToString("O");
            Sentences = ToViews(sentences, log => new QuestLogView(log.FileName, log.Presentation, log.Mark, log.SuccessSequence, log.UtcTime));
        }

        private static T Deserialize<T>(string json) where T : class
        {
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);
        }

        private static List<QuestLogView> ToViews<T>(ProgressData<T> progress, Func<T, QuestLogView> toView) where T : IQuestLog
        {
            if (progress == null)
            {
                return new List<QuestLogView>();
            }

            return progress.Log.Values
                .Select(toView)
                .OrderByDescending(view => view.SortTime)
                .ToList();
        }
    }

    /// <summary>
    /// Wraps the json to show it foldable in the inspector
    /// </summary>
    [Serializable]
    public struct JsonView
    {
        [TextArea(3, 30)] public string Json;
    }

    [Serializable]
    public struct QuestLogView
    {
        public string Key;
        public string Presentation;
        public int Mark;
        public int SuccessSequence;
        public string UtcTime;

        [NonSerialized] public DateTime SortTime;

        public QuestLogView(string key, string presentation, int mark, int successSequence, DateTime utcTime)
        {
            Key = key;
            Presentation = presentation;
            Mark = mark;
            SuccessSequence = successSequence;
            UtcTime = utcTime.ToString("O");
            SortTime = utcTime;
        }
    }
}

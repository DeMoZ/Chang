using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Chang
{
    public class RepetitionView : MonoBehaviour
    {
        private const string TranslationIndent = "  ";

        [SerializeField] private OverviewItem questions;
        [SerializeField] private OverviewItem words;
        [SerializeField] private OverviewItem sentences;
        [SerializeField] private Transform logContainer;

        [Space] [SerializeField] private OverviewLogItem overviewLogItemPrefab;
        [FormerlySerializedAs("repeatBtn")]
        [SerializeField] private Button repeatWordsBtn;
        [SerializeField] private Button repeatSentencesBtn;
        [SerializeField] private Button repeatMixedBtn;

        private Action _onRepeatWordsClick;
        private Action _onRepeatSentencesClick;
        private Action _onRepeatMixedClick;

        public void Set(List<RepetitionLogItem> items)
        {
            foreach (RepetitionLogItem item in items)
            {
                var overviewLogItem = Instantiate(overviewLogItemPrefab, logContainer);
                overviewLogItem.Set(
                    $"{item.LearnText}\n<size=75%>{TranslationIndent}{item.Translation}</size>",
                    item.Mark.ToString(),
                    item.AnswersCount.ToString(),
                    FormatDate(item.UtcTime),
                    item.SuccessSequence.ToString());
            }
        }

        /// <summary>
        /// "02 Oct 06:55" for the current year, "02 Oct 2025" for the previous ones, local time
        /// </summary>
        private static string FormatDate(DateTime utcTime)
        {
            DateTime localTime = utcTime.ToLocalTime();
            string format = localTime.Year == DateTime.Now.Year ? "dd MMM HH:mm" : "dd MMM yyyy";
            return localTime.ToString(format, CultureInfo.InvariantCulture);
        }

        public void Init(Action onRepeatWordsClick, Action onRepeatSentencesClick, Action onRepeatMixedClick)
        {
            _onRepeatWordsClick = onRepeatWordsClick;
            _onRepeatSentencesClick = onRepeatSentencesClick;
            _onRepeatMixedClick = onRepeatMixedClick;
        }

        public void SetInteractableRepeatButtons(bool words, bool sentences, bool mixed)
        {
            repeatWordsBtn.interactable = words;
            repeatSentencesBtn.interactable = sentences;
            repeatMixedBtn.interactable = mixed;
        }

        private void OnEnable()
        {
            repeatWordsBtn.onClick.AddListener(OnRepeatWordsClick);
            repeatSentencesBtn.onClick.AddListener(OnRepeatSentencesClick);
            repeatMixedBtn.onClick.AddListener(OnRepeatMixedClick);
        }

        private void OnDisable()
        {
            repeatWordsBtn.onClick.RemoveListener(OnRepeatWordsClick);
            repeatSentencesBtn.onClick.RemoveListener(OnRepeatSentencesClick);
            repeatMixedBtn.onClick.RemoveListener(OnRepeatMixedClick);

            foreach (Transform child in logContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnRepeatWordsClick()
        {
            _onRepeatWordsClick?.Invoke();
        }

        private void OnRepeatSentencesClick()
        {
            _onRepeatSentencesClick?.Invoke();
        }

        private void OnRepeatMixedClick()
        {
            _onRepeatMixedClick?.Invoke();
        }
    }

    public readonly struct RepetitionLogItem
    {
        public readonly string LearnText;
        public readonly string Translation;
        public readonly int Mark;
        public readonly int AnswersCount;
        public readonly DateTime UtcTime;
        public readonly int SuccessSequence;

        public RepetitionLogItem(string learnText, string translation, int mark, int answersCount, DateTime utcTime, int successSequence)
        {
            LearnText = learnText;
            Translation = translation;
            Mark = mark;
            AnswersCount = answersCount;
            UtcTime = utcTime;
            SuccessSequence = successSequence;
        }
    }
}

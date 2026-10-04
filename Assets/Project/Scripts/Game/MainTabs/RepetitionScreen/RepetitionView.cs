using System;
using System.Collections.Generic;
using System.Globalization;
using Chang.UI.DesignSystem;
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

        [Header("Design (optional): a segmented mode switch + one review button instead of three buttons")]
        [Tooltip("Items in order: Words, Sentences, Mixed")]
        [SerializeField] private DesignSelection modeSelection;
        [SerializeField] private Button reviewBtn;

        private readonly bool[] _canRepeat = { true, true, true };
        private int _mode = -1;

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
            if (repeatWordsBtn != null) repeatWordsBtn.interactable = words;
            if (repeatSentencesBtn != null) repeatSentencesBtn.interactable = sentences;
            if (repeatMixedBtn != null) repeatMixedBtn.interactable = mixed;

            _canRepeat[0] = words;
            _canRepeat[1] = sentences;
            _canRepeat[2] = mixed;

            // Keep the chosen mode while it can be repeated, otherwise switch to the first available one.
            var mode = _mode >= 0 && _canRepeat[_mode] ? _mode : Array.IndexOf(_canRepeat, true);
            SelectMode(mode < 0 ? 0 : mode);
        }

        private void SelectMode(int mode)
        {
            _mode = mode;
            if (modeSelection != null)
            {
                modeSelection.Select(mode);
            }

            if (reviewBtn != null)
            {
                reviewBtn.interactable = _canRepeat[mode];
            }
        }

        private void OnReviewClick()
        {
            switch (_mode)
            {
                case 0:
                    OnRepeatWordsClick();
                    break;
                case 1:
                    OnRepeatSentencesClick();
                    break;
                case 2:
                    OnRepeatMixedClick();
                    break;
            }
        }

        private void OnEnable()
        {
            if (repeatWordsBtn != null) repeatWordsBtn.onClick.AddListener(OnRepeatWordsClick);
            if (repeatSentencesBtn != null) repeatSentencesBtn.onClick.AddListener(OnRepeatSentencesClick);
            if (repeatMixedBtn != null) repeatMixedBtn.onClick.AddListener(OnRepeatMixedClick);
            if (reviewBtn != null) reviewBtn.onClick.AddListener(OnReviewClick);
            if (modeSelection != null)
            {
                modeSelection.Clicked += SelectMode;
            }
        }

        private void OnDisable()
        {
            if (repeatWordsBtn != null) repeatWordsBtn.onClick.RemoveListener(OnRepeatWordsClick);
            if (repeatSentencesBtn != null) repeatSentencesBtn.onClick.RemoveListener(OnRepeatSentencesClick);
            if (repeatMixedBtn != null) repeatMixedBtn.onClick.RemoveListener(OnRepeatMixedClick);
            if (reviewBtn != null) reviewBtn.onClick.RemoveListener(OnReviewClick);
            if (modeSelection != null)
            {
                modeSelection.Clicked -= SelectMode;
            }

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

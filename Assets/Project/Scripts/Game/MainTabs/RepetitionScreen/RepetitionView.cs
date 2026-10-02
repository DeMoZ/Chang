using System;
using System.Collections.Generic;
using Chang.Profile;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Chang
{
    public class RepetitionView : MonoBehaviour
    {
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

        public void Set(List<VocabularyQuestLog> sortedList)
        {
            foreach (var questLog in sortedList)
            {
                var overviewLogItem = Instantiate(overviewLogItemPrefab, logContainer);
                overviewLogItem.Set(
                    questLog.Presentation,
                    questLog.Mark.ToString(),
                    questLog.Log.Count.ToString(),
                    questLog.UtcTime.ToString(),
                    questLog.SuccessSequence.ToString());
            }
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
}
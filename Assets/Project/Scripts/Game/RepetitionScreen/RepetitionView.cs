using System;
using System.Collections.Generic;
using Chang.Profile;
using UnityEngine;
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
        [SerializeField] private Button repeatBtn;
        private Action _onRepeatClick;

        public void Set(List<QuestLog> sortedList)
        {
            foreach (var questLog in sortedList)
            {
                var overviewLogItem = Instantiate(overviewLogItemPrefab, logContainer);
                overviewLogItem.Set(questLog.FileName, questLog.Mark.ToString(), questLog.Log.Count.ToString(), questLog.UtcTime.ToString(),
                    questLog.SuccessSequence.ToString());
            }
        }

        public void Init(Action onRepeatClick)
        {
            _onRepeatClick = onRepeatClick;
        }

        private void OnEnable()
        {
            repeatBtn.onClick.AddListener(OnRepeatClick);
        }

        private void OnDisable()
        {
            repeatBtn.onClick.RemoveListener(OnRepeatClick);

            foreach (Transform child in logContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnRepeatClick()
        {
            _onRepeatClick?.Invoke();
        }
    }
}
using System;
using System.Collections.Generic;
using Chang.Profile;
using UnityEngine;

namespace Chang
{
    public class RepetitionView : MonoBehaviour
    {
        [SerializeField] private OverviewItem questions;
        [SerializeField] private OverviewItem words;
        [SerializeField] private OverviewItem sentences;
        [SerializeField] private Transform logContainer;
        
        [Space]
        [SerializeField] private OverviewLogItem overviewLogItemPrefab;

        internal void Set(List<QuestLog> sortedList)
        {
            foreach (var questLog in sortedList)
            {
                var overviewLogItem = Instantiate(overviewLogItemPrefab, logContainer);
                overviewLogItem.Set(questLog.FileName, questLog.Mark.ToString(), questLog.Log.Count.ToString(), questLog.UtcTime.ToString(), questLog.SuccesSequese.ToString());
            }
        }
    }
}
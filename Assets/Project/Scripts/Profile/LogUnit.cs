using System;
using UnityEngine;

namespace Chang.Profile
{
    [Serializable]
    public class LogUnit
    {
        [field: SerializeField] public DateTime UtcTime { get; private set; }
        [field: SerializeField] public bool IsCorrect { get; private set; }
        
        public bool NeedIncrementMark { get; private set; }

        public LogUnit(DateTime utcTime, bool isCorrect, bool needIncrement)
        {
            UtcTime = utcTime;
            IsCorrect = isCorrect;
            NeedIncrementMark = needIncrement;
        }
    }
}
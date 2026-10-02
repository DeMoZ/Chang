using System;
using UnityEngine;

namespace Chang.Services
{
    [CreateAssetMenu(fileName = "RepetitionConfig", menuName = "Chang/Services/Repetition Config")]
    public class RepetitionConfig : ScriptableObject
    {
        /// <summary>
        /// Assets/Project/Resources/RepetitionConfig.asset
        /// </summary>
        public const string ResourcePath = "RepetitionConfig";

        [Tooltip("Hours after the last answer when the key needs repetition, the element index is the mark. 24 = 1 day")]
        [SerializeField] private float[] intervalHours = { 0, 4, 12, 24, 48, 96, 168, 336, 720, 1440, 2880 };

        [Tooltip("Keys amount in a repetition lesson")]
        [SerializeField, Min(1)] private int lessonAmount = 10;

        [Tooltip("Lesson keys are taken randomly from the top priority keys, pool size = lesson amount * multiplier")]
        [SerializeField, Min(1)] private int poolMultiplier = 3;

        [Tooltip("Words amount in a mixed lesson is random in the range, the rest are sentences")]
        [SerializeField, Min(0)] private int mixedWordsMin = 3;
        [SerializeField, Min(0)] private int mixedWordsMax = 7;

        [Tooltip("Played keys amount required to make a repetition lesson")]
        [SerializeField, Min(1)] private int minAvailableAmount = 1;

        public int LessonAmount => lessonAmount;
        public int PoolMultiplier => poolMultiplier;
        public int MixedWordsMin => mixedWordsMin;
        public int MixedWordsMax => mixedWordsMax;
        public int MinAvailableAmount => minAvailableAmount;

        public float GetIntervalHours(int mark)
        {
            return intervalHours[Math.Clamp(mark, 0, intervalHours.Length - 1)];
        }

        private void OnValidate()
        {
            int marksAmount = ProjectConstants.MARK_MAX + 1;
            if (intervalHours.Length != marksAmount)
            {
                Array.Resize(ref intervalHours, marksAmount);
            }

            mixedWordsMax = Math.Min(mixedWordsMax, lessonAmount);
            mixedWordsMin = Math.Min(mixedWordsMin, mixedWordsMax);
        }
    }
}

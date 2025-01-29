using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(fileName = "LessonConfig", menuName = "Chang/GameBook/Lesson Config", order = 1)]
    public class LessonConfig : SerializedScriptableObject
    {
        [field: SerializeField] public bool GenerateQuestMatchWordsData { get; set; } = true;

        [SerializeReference] [InlineEditor(Expanded = true)] [ListDrawerSettings(ShowIndexLabels = true)]
        public List<QuestionConfig> Questions;

        public void OnValidate()
        {
            foreach (var question in Questions)
            {
                question?.Init();
            }
        }
    }
}
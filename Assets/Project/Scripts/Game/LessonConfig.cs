using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(fileName = "LessonConfig", menuName = "Chang/GameBook/Lesson Config", order = 1)]
    public class LessonConfig : SerializedScriptableObject
    {
        //s[ListDrawerSettings(Expanded = true)]
        //[InlineEditor(Expanded = true)]
        public List<Question> Questions;

        public void OnValidate()
        {
            foreach (var question in Questions)
            {
                question.Init();
            }
        }
    }
}
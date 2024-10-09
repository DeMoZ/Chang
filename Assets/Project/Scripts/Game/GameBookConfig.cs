using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(fileName = "GameBookConfig", menuName = "Chang/GameBook/GameBook Config", order = 0)]
    public class GameBookConfig : ScriptableObject
    {
        [InlineEditor(Expanded = true)]
        public List<LessonConfig> Lessons;
    }
}
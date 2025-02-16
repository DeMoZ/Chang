using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(fileName = "GameBookConfig", menuName = "Chang/GameBook/GameBook Config", order = 0)]
    public class GameBookConfig : ScriptableObject
    {
        [field: SerializeField] public Languages Language { get; set; } = Languages.English;
        [field: SerializeField] public string Section { get; set; } = string.Empty;
        [field: SerializeField] public string Name { get; set; } = string.Empty;

        [InlineEditor(Expanded = true)] public List<LessonConfig> Lessons;
    }
}
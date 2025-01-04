using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/Question Config", fileName = "QuestionConfig")]
    public class QuestionConfig : ScriptableObject
    {
        [field: SerializeField] public QuestionType QuestionType { get; private set; }
        [field: SerializeField, ReadOnly] public string Info { get; private set; } = string.Empty;
        [SerializeReference] public QuestBase QuestionData;

        public void Init() => OnValidate();

        public void OverrideType(QuestionType type)
        {
            QuestionType = type;
            QuestionData.OverrideType(type);
        }

        private void OnValidate()
        {
            // if (QuestionData != null)
            //     QuestionType = QuestionData.QuestionType;
            // else
            //     QuestionType = QuestionType.None;

            Info = QuestionData == null ? string.Empty : QuestionData.EditorInfo();
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/Question Config", fileName = "QuestionConfig")]
    public class QuestionConfig : ScriptableObject
    {
        [field: SerializeField] public QuestionType QuestionType { get; private set; }
        [field: SerializeField, ReadOnly] public string Info { get; private set; } = string.Empty;
        [FormerlySerializedAs("QuestionData")] [SerializeReference] public QuestBase Question;

        public void Init() => OnValidate();

        private void OnValidate()
        {
            Info = Question == null ? string.Empty : Question.EditorInfo();
        }
        
        public QuestDataBase GetQuestData()
        {
            return Question?.GetQuestData();
        }
    }
}
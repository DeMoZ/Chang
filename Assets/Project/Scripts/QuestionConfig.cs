using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang
{
    [CreateAssetMenu(menuName = "Chang/Question Config", fileName = "QuestionConfig")]
    public class QuestionConfig : ScriptableObject
    {
         [field: SerializeField, ReadOnly] public QuestionType QuestionType { get; private set; }
         [field: SerializeField, ReadOnly] public string Info { get; private set; } = string.Empty;
         [SerializeReference] public QuestBase QuestionData;

         public void Init() => OnValidate();
         
         private void OnValidate()
         {
             QuestionType = QuestionData?.QuestionType ?? QuestionType.None;
             Info = QuestionData == null ? string.Empty : QuestionData.EditorInfo();
         }
    }
}
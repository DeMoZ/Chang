using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities
{
    [CreateAssetMenu(fileName = "BookToJsonUtility", menuName = "Chang/GameBook/BookToJsonUtility", order = 0)]
    public class BookToJsonUtility : ScriptableObject
    {
        private const string WordPath = ""; // "Word/";

        public GameBookConfig GameBookConfig;
        public TextAsset GameBookJson;

        [Button("Make Book Json")]
        private void MakeJsonDepth()
        {
            Debug.Log($"{nameof(MakeJsonDepth)} Start");
            var bookData = new BookData
            {
                FileName = GameBookConfig.name,
                Lessons = new List<LessonData>()
            };

            foreach (var lesson in GameBookConfig.Lessons)
            {
                var lessonData = new LessonData();

                if (lesson != null)
                {
                    lessonData.FileName = lesson.name;
                    lessonData.Questions = GetQuestions(lesson.Questions);
                }

                bookData.Lessons.Add(lessonData);
            }

            var jSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var json = JsonConvert.SerializeObject(bookData, jSettings);
            File.WriteAllText(AssetDatabase.GetAssetPath(GameBookJson), json);

            AssetDatabase.Refresh();
            Debug.Log($"{nameof(MakeJsonDepth)} Done");

            return;

            List<IQuestionData> GetQuestions(List<QuestionConfig> questions)
            {
                List<IQuestionData> questionData = new();

                foreach (var question in questions)
                {
                    switch (question.QuestionType)
                    {
                        case QuestionType.DemonstrationWord:
                            throw new NotImplementedException();
                        
                        case QuestionType.SelectWord:
                            var qData = question.Question as QuestSelectWord;
                            var data = new QuestSelectWordData
                            {
                                FileName = question.name,
                                CorrectWordFileName = $"{WordPath}{qData.CorrectWord.Key}",
                                MixWordsFileNames = qData.MixWords.Select(c => $"{WordPath}{c.Key}").ToList()
                            };
                            questionData.Add(data);
                            break;
                        
                        case QuestionType.MatchWords:
                            throw new NotImplementedException();
                        
                        case QuestionType.DemonstrationDialogue:
                            throw new NotImplementedException();
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return questionData;
            }
        }

        #region MakeJsonDepth

        [Serializable]
        public struct BookData
        {
            public string FileName;
            public List<LessonData> Lessons;
        }
        
        [Serializable]
        public struct LessonData
        {
            public string FileName;
            public List<IQuestionData> Questions;
        }

        public interface IQuestionData
        {
            QuestionType QuestionType { get; }
            string FileName { get; set; }
        }

        public class QuestSelectWordData : IQuestionData
        {
            public QuestionType QuestionType => QuestionType.SelectWord;
            public string FileName { get; set; }
            
            public string CorrectWordFileName;
            public List<string> MixWordsFileNames;
        }

        #endregion
    }
}
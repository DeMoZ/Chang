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

        //[Button("Make Json of lesson names")]
        [Obsolete]
        private void MakeJsonOfLessonNames()
        {
            Debug.Log($"{nameof(MakeJsonOfLessonNames)} Start");
            var result = new List<LessonData>();

            foreach (var lesson in GameBookConfig.Lessons)
            {
                var assetPath = AssetDatabase.GetAssetPath(lesson);
                var fileName = Path.GetFileNameWithoutExtension(assetPath);

                result.Add(new LessonData()
                {
                    FileName = fileName,
                });
            }

            var jSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var json = JsonConvert.SerializeObject(result, jSettings);

            File.WriteAllText(AssetDatabase.GetAssetPath(GameBookJson), json);

            AssetDatabase.Refresh();
            Debug.Log($"{nameof(MakeJsonOfLessonNames)} Done");
        }

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
                            var qData = question.QuestionData as QuestSelectWord;
                            var data = new QuestSelectWordData
                            {
                                QuestionType = qData!.QuestionType,
                                FileName = question.name,
                                CorrectWordFileName = $"{WordPath}{qData.CorrectWord.name}",
                                MixWordsFileNames = qData.MixWords.Select(c => $"{WordPath}{c.name}").ToList()
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
            QuestionType QuestionType { get; set; }
            string FileName { get; set; }
        }

        public class QuestSelectWordData : IQuestionData
        {
            public QuestionType QuestionType { get; set; }
            public string FileName { get; set; }
            
            public string CorrectWordFileName;
            public List<string> MixWordsFileNames;
        }

        #endregion
    }
}
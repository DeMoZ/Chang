using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities
{
    [CreateAssetMenu(fileName = "BookToJsonUtility", menuName = "Chang/GameBook/BookToJsonUtility", order = 0)]
    public class BookToJsonUtility : ScriptableObject
    {
        private const string WordPath = ""; // "Word/";

        [SerializeField] private TextAsset gameBookJson;
        [SerializeField] private GameBookConfig[] gameBookConfigs;

        JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        [Button("Make Book Json")]
        private void MakeJsonDepth()
        {
            Debug.LogWarning("Start");
            List<BookData> booksData = new();
            List<string> jsons = new();

            foreach (var config in gameBookConfigs)
            {
                Debug.Log($"BookData for {config.name}");
                var bookData = CreateBookData(config);
                booksData.Add(bookData);
            }

            var lessons = booksData.SelectMany(b => b.Lessons).ToList();
            var allBooksData = new BookData()
            {
                FileName = "AllBooks",
                Lessons = lessons
            };

            Debug.Log($"json for {allBooksData.FileName}");
            var json = JsonConvert.SerializeObject(allBooksData, _jSettings);

            Debug.Log("Save GameBookJson");
            File.WriteAllText(AssetDatabase.GetAssetPath(gameBookJson), json);

            AssetDatabase.Refresh();
            Debug.LogWarning("End");
        }

        private BookData CreateBookData(GameBookConfig config)
        {
            var bookData = new BookData
            {
                FileName = config.name,
                Lessons = new List<LessonData>()
            };

            foreach (var lesson in config.Lessons)
            {
                var lessonData = new LessonData();

                if (lesson != null)
                {
                    lessonData.FileName = lesson.name;
                    lessonData.GenerateQuestMatchWordsData = lesson.GenerateQuestMatchWordsData;
                    lessonData.Questions = GetQuestions(lesson.Questions);
                    bookData.Lessons.Add(lessonData);
                }
            }

            return bookData;
        }

        private List<IQuestionData> GetQuestions(List<QuestionConfig> questions)
        {
            List<IQuestionData> questionData = new();

            foreach (var question in questions)
            {
                switch (question.QuestionType)
                {
                    case QuestionType.DemonstrationWord:
                        throw new NotImplementedException();

                    case QuestionType.SelectWord:
                        var selectWord = question.Question as QuestSelectWord;
                        QuestSelectWordData selectWordData = new QuestSelectWordData
                        {
                            FileName = question.name,
                            CorrectWordFileName = $"{WordPath}{selectWord.CorrectWord.Key}",
                            MixWordsFileNames = selectWord.MixWords.Select(c => $"{WordPath}{c.Key}").ToList()
                        };
                        questionData.Add(selectWordData);
                        break;

                    case QuestionType.MatchWords:
                        var matchWords = question.Question as QuestMatchWords;
                        QuestMatchWordsData matchWordsData = new QuestMatchWordsData
                        {
                            FileName = question.name,
                            MatchWordsFileNames = matchWords.MatchWords.Select(c => $"{WordPath}{c.Key}").ToList()
                        };
                        questionData.Add(matchWordsData);
                        break;

                    case QuestionType.DemonstrationDialogue:
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return questionData;
        }

        #region Book Classes

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
            public bool GenerateQuestMatchWordsData;
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

        public class QuestMatchWordsData : IQuestionData
        {
            public QuestionType QuestionType => QuestionType.MatchWords;
            public string FileName { get; set; }

            public List<string> MatchWordsFileNames;
        }

        #endregion
    }
}
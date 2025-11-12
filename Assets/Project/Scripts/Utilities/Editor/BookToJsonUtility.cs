using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Resources;
using Chang.Vocabulary;
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
        private const string WORDS = AssetPaths.Utilities.WordsFolder;

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
            List<VocabularyBookData> booksData = new();
            List<string> jsons = new();

            foreach (var config in gameBookConfigs)
            {
                Debug.Log($"BookData for {config.name}");
                var bookData = CreateBookData(config);
                booksData.Add(bookData);
            }

            var sections = booksData.SelectMany(b => b.Sections).ToList();
            var allBooksData = new VocabularyBookData()
            {
                FileName = "AllBooks",
                Language = booksData[0].Language, // todo chang may be some changes in future if contains different languages
                Sections = sections
            };

            Debug.Log($"json for {allBooksData.FileName}");
            var json = JsonConvert.SerializeObject(allBooksData, _jSettings);

            Debug.Log("Save GameBookJson");
            File.WriteAllText(AssetDatabase.GetAssetPath(gameBookJson), json);

            AssetDatabase.Refresh();
            Debug.LogWarning("End");
        }

        private VocabularyBookData CreateBookData(GameBookConfig config)
        {
            var bookData = new VocabularyBookData
            {
                FileName = config.name,
                Sections = new List<SectionData>(),
                Language = config.Language,
            };

            int cnt = 0;
            SectionData sectionData = new SectionData();
            foreach (var lesson in config.Lessons)
            {
                var lessonData = new LessonData();

                if (lesson != null)
                {
                    lessonData.FileName = lesson.name;
                    lessonData.SectionName = lesson.Section; // todo chang temp. probably i2l key, also include the number?
                    lessonData.Name = $"{lesson.Section} {++cnt}"; // todo chang temp, change to cnt
                    lessonData.GenerateQuestMatchWordsData = lesson.GenerateQuestMatchWordsData;
                    lessonData.Questions = GetQuestions(lesson.Questions);

                    if (string.IsNullOrEmpty(sectionData.Section) || !sectionData.Section.Equals(lessonData.SectionName))
                    {
                        sectionData = new SectionData
                        {
                            Section = lessonData.SectionName,
                            Lessons = new List<LessonData>()
                        };

                        bookData.Sections.Add(sectionData);
                    }

                    sectionData.Lessons.Add(lessonData);
                }
            }

            return bookData;
        }

        private List<IQuestion> GetQuestions(List<QuestionConfig> questions)
        {
            List<IQuestion> questionData = new();

            foreach (var question in questions)
            {
                switch (question.QuestionType)
                {
                    case QuestionType.DemonstrationWord:
                        throw new NotImplementedException();

                    case QuestionType.SelectWord:
                        var selectWord = question.Question as QuestSelectWord;
                        Vocabulary.QuestSelectWord selectWordData = new Vocabulary.QuestSelectWord
                        {
                            FileName = question.name,
                            CorrectWordFileName = Path.Combine(
                                selectWord.CorrectWord.Language.ToString(),
                                WORDS,
                                selectWord.CorrectWord.Section,
                                selectWord.CorrectWord.Key),
                            MixWordsFileNames = selectWord.MixWords
                                .Select(c => Path.Combine(
                                    c.Language.ToString(),
                                    WORDS,
                                    c.Section,
                                    c.Key))
                                .ToList()
                        };
                        questionData.Add(selectWordData);
                        break;

                    case QuestionType.MatchWords:
                        var matchWords = question.Question as QuestMatchWords;
                        Vocabulary.QuestMatchWords matchWordsData = new Vocabulary.QuestMatchWords
                        {
                            FileName = question.name,
                            MatchWordsFileNames = matchWords.MatchWords.Select(c => Path.Combine(
                                    c.Language.ToString(),
                                    WORDS,
                                    c.Section,
                                    c.Key))
                                .ToList()
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
    }
}
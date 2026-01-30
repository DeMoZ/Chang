using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Debug = DMZ.DebugSystem.DMZLogger;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    public class VocabularySheetToJson
    {
        #region Subclasses

        [Serializable]
        public class Book
        {
            public Languages Language;
            public List<Sheet> Sheets;
        }

        [Serializable]
        public class Sheet
        {
            public SheetProperties Properties;
            public List<Word> Word;
        }

        public class SheetProperties
        {
            public Languages Language;
            public QuestionType Type;
            public string Title;
        }

        // todo chang move it into separate file and keep in NOT EDITOR FOLDER
        public class Word
        {
            public string PathKey;
            public string PathImageKey;
            public string PathSoundKey;
            public string Key;

            public string LearnWord;
            public string Phonetics;
            public string DefaultTranslation;
            public string Description;
        }

        #endregion


        private const string JsonCredentials = "chang_gcloudconsole_credentials.json";
        private const string SheetType = "Vocabulary";

        private readonly Languages _language;

        private string SpreadSheetIdFileName => $"{_language}VocabularyAndSentences_ids.json";
        private string JsonCredentialsPath => Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, JsonCredentials);


        public VocabularySheetToJson(Languages language)
        {
            _language = language;
        }

        public async UniTask<Book> Get()
        {
            SpreadSheetInfoProvider provider = new SpreadSheetInfoProvider(SpreadSheetIdFileName, JsonCredentialsPath);
            await provider.InitAsync();

            SpreadSheetInfo spreadSheet = await provider.GetBookAsync();
            List<SheetInfo> sheets = spreadSheet.Sheets.Where(s => s.Type == SheetType && s.Language == _language).ToList();

            Book book = new()
            {
                Language = _language,
                Sheets = new List<Sheet>()
            };

            foreach (var sheet in sheets)
            {
                Debug.Log($"Sheet: {sheet.Title}, type: {sheet.Type}, language: {sheet.Language}");
                if (!Enum.TryParse(sheet.Type, true, out QuestionType type))
                {
                    Debug.LogError($"Sheet type is not recognised {sheet.Type}");
                }

                SheetProperties properties = new SheetProperties
                {
                    Language = sheet.Language,
                    Type = type,
                    Title = sheet.Title
                };

                string dataRange = $"{sheet.Title}!B6:I";
                IList<IList<object>> data = await provider.GetSheetDataAsync(dataRange);
                List<Word> words = new List<Word>();

                foreach (IList<object> entity in data)
                {
                    Word word = new()
                    {
                        PathKey = SpreadSheetUtils.SafeGetValue(entity, 0),
                        PathImageKey = SpreadSheetUtils.SafeGetValue(entity, 1),
                        PathSoundKey = SpreadSheetUtils.SafeGetValue(entity, 2),
                        Key = SpreadSheetUtils.SafeGetValue(entity, 3),
                        LearnWord = SpreadSheetUtils.SafeGetValue(entity, 4),
                        Phonetics = SpreadSheetUtils.SafeGetValue(entity, 5),
                        DefaultTranslation = SpreadSheetUtils.SafeGetValue(entity, 6),
                        Description = SpreadSheetUtils.SafeGetValue(entity, 7)
                    };

                    words.Add(word);
                }

                book.Sheets.Add(new Sheet
                {
                    Properties = properties,
                    Word = words
                });
            }

            return book;
        }
    }
}
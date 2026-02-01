using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    public class SentencesSheetsProcess
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
            public List<Sentence> Sentences;
        }
        
        #endregion

        private const string JsonCredentials = "chang_gcloudconsole_credentials.json";
        private const string SheetType = "Sentences";

        private readonly Languages _language;

        private string SpreadSheetIdFileName => $"{_language}VocabularyAndSentences_ids.json";
        private string JsonCredentialsPath => Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, JsonCredentials);
        
        public SentencesSheetsProcess(Languages language)
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
                DMZLogger.Log($"Sheet: {sheet.Title}, type: {sheet.Type}, language: {sheet.Language}");
                if (!Enum.TryParse(sheet.Type, true, out QuestionType type))
                {
                    DMZLogger.LogError($"Sheet type is not recognised {sheet.Type}");
                }

                SheetProperties properties = new SheetProperties
                {
                    Language = sheet.Language,
                    Type = type,
                    Title = sheet.Title,
                };

                string dataRange = $"{sheet.Title}!C5:O";
                IList<IList<object>> data = await provider.GetSheetDataAsync(dataRange);
                List<Sentence> sentences = new ();

                foreach (IList<object> entity in data)
                {
                    Sentence sentence = new()
                    {
                        Language = sheet.Language,
                        Section = sheet.Section,
                        
                        // PathKey = SpreadSheetUtilities.SafeGetValue(entity, 0),
                        // PathImageKey = SpreadSheetUtilities.SafeGetValue(entity, 1),
                        // PathSoundKey = SpreadSheetUtilities.SafeGetValue(entity, 2),
                        // Key = SpreadSheetUtilities.SafeGetValue(entity, 3),
                        // LearnWord = SpreadSheetUtilities.SafeGetValue(entity, 4),
                        // Phonetics = SpreadSheetUtilities.SafeGetValue(entity, 5),
                        // DefaultTranslation = SpreadSheetUtilities.SafeGetValue(entity, 6),
                        // DefaultDescription = SpreadSheetUtilities.SafeGetValue(entity, 7)
                    };

                    sentences.Add(sentence);
                }

                book.Sheets.Add(new Sheet
                {
                    Properties = properties,
                    Sentences = sentences
                });
            }

            return book;
        }
    }
}
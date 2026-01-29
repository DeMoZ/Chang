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
            // List<SheetInfo> vocabularySheets = spreadSheet.Sheets.Where(sheetInfo => string.Equals(sheetInfo.Type, SheetType)).ToList();

            Book book = new ()
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
                
                string dataRange = $"{sheet.Title}!B6:I"; // Get all the range from the sheet 
                IList<IList<object>> data = await provider.GetSheetDataAsync(dataRange);
                List<Word> words = new List<Word>();
                
                foreach (IList<object> entity in data)
                {
                    Word word = new()
                    {
                        PathKey = entity[0].ToString(),
                        PathImageKey = entity[1].ToString(),
                        PathSoundKey = entity[2].ToString(),
                        Key = entity[3].ToString(),
                        LearnWord = entity[4].ToString(),
                        Phonetics = entity[5].ToString(),
                        DefaultTranslation = entity[6].ToString(),
                        Description = entity[7].ToString()
                    };
                    
                    words.Add(word);
                }
                
                book.Sheets.Add(new Sheet
                {
                    Properties = properties,
                    Word = words
                });
            }
            
//             foreach (var sheet in sheets)
//             {
//
//              
//                 string dataRange = $"{sheetTitle}!B4:Z";
//                 var dataRequest = service.Spreadsheets.Values.Get(spreadsheetId, dataRange);
//                 var dataResponse = await dataRequest.ExecuteAsync();
//                 var values = dataResponse.Values;
//
//                 if (values != null && values.Count > 0)
//                 {
//                     var newSheet = new Sheet
//                     {
//                         Title = sheetTitle,
//                         Rows = values.Select(r => new Row
//                         {
//                             LearnWord = r.Count > 0 ? r[0]?.ToString() : string.Empty,
//                             Phonetics = r.Count > 1 ? r[1]?.ToString() : string.Empty,
//                             Meaning = r.Count > 2 ? r[2]?.ToString() : string.Empty,
//                             Image = r.Count > 3 ? r[3]?.ToString() : string.Empty,
//                             Sound = r.Count > 4 ? r[4]?.ToString() : string.Empty,
//                         }).ToList()
//                     };
//                     //book.Sheets.Add(newSheet);
//                     Debug.Log($"Лист '{sheetTitle}' обработан.");
//                 }
//             }

            return book;
        }
    }
}
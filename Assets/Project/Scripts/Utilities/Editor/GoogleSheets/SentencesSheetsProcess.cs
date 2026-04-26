using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

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
        private const int ChunkSize = 8;

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
                Debug.Log($"Sheet: {sheet.Title}, type: {sheet.Type}, language: {sheet.Language}");
                if (!Enum.TryParse(sheet.Type, true, out ChangTypes type))
                {
                    Debug.LogError($"Sheet type is not recognised {sheet.Type}");
                }

                SheetProperties properties = new SheetProperties
                {
                    Language = sheet.Language,
                    Type = type,
                    Title = sheet.Title,
                };

                string dataRange = $"{sheet.Title}!C5:O";
                IList<IList<object>> data = await provider.GetSheetDataAsync(dataRange);
                List<Sentence> sentences = new();

                for (int i = 0; i < data.Count; i += ChunkSize)
                {
                    if (data.Count < i + ChunkSize)
                    {
                        break;
                    }

                    List<IList<object>> chunk = data.Skip(i).Take(ChunkSize).ToList();

                    var sentenceWords = new List<SentenceWord>();
                    for (int j = 0; j < ChunkSize; j++)
                    {
                        string wordKey = SpreadSheetUtilities.SafeGetValue(chunk, 7, j);

                        if (string.IsNullOrEmpty(wordKey))
                        {
                            break;
                        }

                        SentenceWord sentenceWord = new SentenceWord
                        {
                            WordKey = wordKey
                        };

                        sentenceWord.SetModifiers(SpreadSheetUtilities.SafeGetValue(chunk, 5, j));
                        sentenceWords.Add(sentenceWord);
                    }

                    Sentence sentence = new Sentence
                    {
                        Language = properties.Language,
                        Section = sheet.Section,

                        Key = SpreadSheetUtilities.SafeGetValue(chunk, 0, 0),
                        SentenceKey = SpreadSheetUtilities.SafeGetValue(chunk, 1, 0),
                        ImageKey = SpreadSheetUtilities.SafeGetValue(chunk, 2, 0),
                        SoundKey = SpreadSheetUtilities.SafeGetValue(chunk, 3, 0),
                        DefaultTranslation = SpreadSheetUtilities.SafeGetValue(chunk, 4, 0),
                        SentenceWords = sentenceWords
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
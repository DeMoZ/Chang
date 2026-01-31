using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    public class VocabularyBookSheetsProcess
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
            public List<VocabularyBookSection> Sections;
        }

        [Serializable]
        public class SheetProperties
        {
            public Languages Language;
            public QuestionType Type;
            public string Title;
        }

        #endregion

        private const string JsonCredentials = "chang_gcloudconsole_credentials.json";
        private const string SheetType = "VocabularyBook";
        private const string SectionWord = "Section";

        private readonly Languages _language;

        private string SpreadSheetIdFileName => $"{_language}VocabularyAndSentences_ids.json";
        private string JsonCredentialsPath => Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, JsonCredentials);

        public VocabularyBookSheetsProcess(Languages language)
        {
            _language = language;
        }

public async UniTask<Book> Get()
        {
            SpreadSheetInfoProvider provider = new SpreadSheetInfoProvider(SpreadSheetIdFileName, JsonCredentialsPath);
            await provider.InitAsync();
        
            SpreadSheetInfo spreadSheet = await provider.GetBookAsync();
            List<SheetInfo> sheetsInfo = spreadSheet.Sheets.Where(s => s.Type == SheetType && s.Language == _language).ToList();
        
            Book book = new()
            {
                Language = _language,
                Sheets = new List<Sheet>()
            };
        
            foreach (var sheetInfo in sheetsInfo)
            {
                Debug.Log($"Sheet: {sheetInfo.Title}, type: {sheetInfo.Type}, language: {sheetInfo.Language}");
                if (!Enum.TryParse(sheetInfo.Type, true, out QuestionType type))
                {
                    Debug.LogError($"Sheet type is not recognised {sheetInfo.Type}");
                    continue;
                }
        
                Sheet currentSheet = new Sheet
                {
                    Properties = new SheetProperties
                    {
                        Language = sheetInfo.Language,
                        Type = type,
                        Title = sheetInfo.Title,
                    },
                    Sections = new List<VocabularyBookSection>()
                };
                book.Sheets.Add(currentSheet);
        
                string dataRange = $"{sheetInfo.Title}!A4:C";
                IList<IList<object>> data = await provider.GetSheetDataAsync(dataRange);
        
                VocabularyBookSection currentSection = null;
                SectionLesson currentLesson = null;
        
                foreach (var row in data)
                {
                    string colA = SpreadSheetUtilities.SafeGetValue(row, 0);
                    string colB = SpreadSheetUtilities.SafeGetValue(row, 1);
                    string colC = SpreadSheetUtilities.SafeGetValue(row, 2);
        
                    // start a new section
                    if (string.Equals(colA, SectionWord, StringComparison.InvariantCultureIgnoreCase))
                    {
                        currentSection = new VocabularyBookSection
                        {
                            Language = sheetInfo.Language,
                            Section = colB,
                            TitleKey = $"{sheetInfo.Language}/{sheetInfo.Type}/{colB}",
                            SectionLessons = new List<SectionLesson>()
                        };
                        currentSheet.Sections.Add(currentSection);
                        currentLesson = null; // Reset current lesson when a new section starts
                        continue;
                    }
        
                    if (currentSection == null) continue; // Skin row until a section is defined
        
                    // start a new lesson
                    if (!string.IsNullOrEmpty(colB))
                    {
                        currentLesson = new SectionLesson
                        {
                            Language = sheetInfo.Language,
                            Section = currentSection.Section,
                            WordKeys = new List<string>()
                        };
                        currentSection.SectionLessons.Add(currentLesson);
                    }
        
                    // add word key to the current lesson
                    if (!string.IsNullOrEmpty(colC))
                    {
                        if (currentLesson == null)
                        {
                            currentLesson = new SectionLesson { WordKeys = new List<string>() };
                            currentSection.SectionLessons.Add(currentLesson);
                        }
                        currentLesson.WordKeys.Add(colC);
                    }
                }
            }
        
            return book;
        }
    }
}
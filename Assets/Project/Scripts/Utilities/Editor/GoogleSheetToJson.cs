using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Chang.Utilities
{
    public class GoogleSheetToJson
    {
        public async Task<IList<IList<object>>> TryGetWords()
        {
            ChangSpreadSheetService changSpreadSheetService = new ChangSpreadSheetService();
            Spreadsheet spreadsheet = await changSpreadSheetService.GetSpreadsheet();

            var firstSheet = spreadsheet.Sheets.FirstOrDefault();

            var rows = new List<IList<object>>();
            if (firstSheet == null)
            {
                Debug.LogError($"There are no sheets in the document");
                return rows;
            }

            var firstSheetName = firstSheet.Properties.Title;
            Debug.Log($"{nameof(firstSheetName)}: {firstSheetName}\nnow we use only first sheet");
            var rangeRequest = $"{firstSheetName}"; // all the data from the page
            var range = changSpreadSheetService.GetRange(rangeRequest);
            rows = range.Values.ToList();
            return rows;
        }

        public async Task<Book> TryGetBook()
        {
            ChangSpreadSheetService changSpreadSheetService = new ChangSpreadSheetService();
            Spreadsheet spreadsheet = await changSpreadSheetService.GetSpreadsheet();

            var sheets = spreadsheet.Sheets;
            var book = new Book();

            foreach (var sheet in sheets)
            {
                string propertiesRangeRequest = $"{sheet.Properties.Title}!A1:O2"; // Properties range on every sheet
                ValueRange propertiesRange = changSpreadSheetService.GetRange(propertiesRangeRequest);

                var keys = propertiesRange.Values[0];
                var values = propertiesRange.Values[1];
                var properties = new Dictionary<string, string>();

                for (int i = 0; i < keys.Count; i++)
                {
                    properties[keys[i].ToString()] = values[i].ToString();
                }

                properties.TryGetValue("Skip", out var skip);

                string dataRangeRequest = $"{sheet.Properties.Title}!B4:Z"; // Properties range on every sheet
                ValueRange dataRange = changSpreadSheetService.GetRange(dataRangeRequest);
                Sheet dataSheet = new Sheet();
                dataSheet.Title = sheet.Properties.Title;
                dataSheet.Properties = new SheepProperties
                {
                    Skip = !string.IsNullOrEmpty(skip),
                    Language = Languages.Thai,
                    Type = GetQuestionType(properties["Type"]),
                    Name = sheet.Properties.Title
                };

                if (!string.IsNullOrEmpty(skip))
                {
                    Debug.Log($"Sheet: {sheet.Properties.Title} is <color=yellow>skipped</color>\n");
                    continue;
                }

                dataSheet.Rows = dataRange.Values.Select(x => new Row
                {
                    LearnWord = x[0]?.ToString(),
                    Phonetics = x[1]?.ToString(),
                    Meaning = x[2]?.ToString(),
                    // Image = x[3]?.ToString(),
                    // Sound = x[4]?.ToString(),
                }).ToList();

                Debug.Log($"Sheet: {sheet.Properties.Title} finished");
                
                book.Sheets.Add(dataSheet);
                
                // test
                // return book;
            }

            return book;
        }

        private QuestionType GetQuestionType(string type)
        {
            return Enum.TryParse(type, out QuestionType questionType) ? questionType : QuestionType.None;
        }
    }

    [Serializable]
    public class Book
    {
        public List<Sheet> Sheets = new();
    }

    [Serializable]
    public class Sheet
    {
        public string Title;
        public SheepProperties Properties;
        public List<Row> Rows;
    }

    [Serializable]
    public class SheepProperties
    {
        public bool Skip;
        public Languages Language;
        public QuestionType Type;
        public string Name;
    }

    public class Row
    {
        public string LearnWord;
        public string Phonetics;
        public string Meaning;
        public string Image;
        public string Sound;
    }
}
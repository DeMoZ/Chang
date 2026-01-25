using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    public class VocabularySheetToJson
    {
        #region Subclasses

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

        #endregion

        private const string ApplicationName = "Google Sheets API Unity";
        //private string TokenFileName => $"{_language}VocabularyAndSentences_gcloud_client_token.json";
        private string SpreadSheetIdFileName => $"{_language}VocabularyAndSentences_ids.json";

        private const string JsonCredentials = "chang_gcloudconsole_credentials.json";

        private readonly string _jsonCredentialsPath =
            Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, JsonCredentials);

        private readonly Languages _language;

        public VocabularySheetToJson(Languages language)
        {
            _language = language;
        }

        public async UniTask<Book> Get()
        {
            string spreadsheetId = await SpreadSheetInfoProvider.GetSpreadSheetIdAsync(SpreadSheetIdFileName);
            GoogleCredential credential = await SpreadSheetInfoProvider.GetCredentialsAsync(JsonCredentials);

            if (string.IsNullOrEmpty(spreadsheetId) || credential == null)
            {
                Debug.LogError($"Spreadsheet Id {spreadsheetId} or credential was not loaded from json files.");
                return null;
            }

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            string range = "VOCABULARY!C2:C11";

            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            return null;
        }

        public async UniTask<Book> _Get()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(_jsonCredentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "My Google Sheets App",
            });

            string range = "VOCABULARY!C2:C11";
            // Запрос данных
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get("1ygcHjslvaO4C_DrLOeRNmIylLt_6Oo9PSxk4fkM28XI", range);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            // if (values != null && values.Count > 0)
            // {
            //     foreach (var row in values)
            //     {
            //         var row1 = row.Count>1 ? row[1].ToString() : "";
            //         // Вывод первого и второго столбца
            //         Debug.Log($"{row[0]}, {row1}");
            //     }
            // }
            // else
            // {
            //     Console.WriteLine("Данные не найдены.");
            // }

            return null;
        }
/*
        public async UniTask<Book> _Get()
        {
            ChangSpreadSheetService changSpreadSheetService = new ChangSpreadSheetService(ApplicationName, TokenFileName, IdFileName);
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
                // dataSheet.Properties = new Depricated.SheepProperties
                // {
                //     Skip = !string.IsNullOrEmpty(skip),
                //     Language = Languages.Thai,
                //     Type = GetQuestionType(properties["Type"]),
                //     Name = sheet.Properties.Title
                // };
                //
                // if (!string.IsNullOrEmpty(skip))
                // {
                //     Debug.Log($"Sheet: {sheet.Properties.Title} is <color=yellow>skipped</color>\n");
                //     continue;
                // }
                //
                // dataSheet.Rows = dataRange.Values.Select(x => new Depricated.Row
                // {
                //     LearnWord = x[0]?.ToString(),
                //     Phonetics = x[1]?.ToString(),
                //     Meaning = x[2]?.ToString(),
                //     // Image = x[3]?.ToString(),
                //     // Sound = x[4]?.ToString(),
                // }).ToList();

                // Debug.Log($"Sheet: {sheet.Properties.Title} finished");
                //
                // book.Sheets.Add(dataSheet);

                // test
                // return book;
            }

            return book;
        }*/
    }
}
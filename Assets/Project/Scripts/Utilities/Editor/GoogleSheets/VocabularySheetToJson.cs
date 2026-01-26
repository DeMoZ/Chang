using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    /// <summary>
    /// Contains information about the book obtained from Google Sheets.
    /// </summary>
    [CreateAssetMenu(fileName = "BookInfo", menuName = "Chang/Utilities/Google Sheets/Book Info", order = 0)]
    public class BookInfo : SerializedScriptableObject
    {
        public List<SpreadSheetInfo> SpreadsheetInfos = new();

        [Button("Collect Info")]
        public void Collect()
        {
            // собрать информацию о книге
        }
    }

    public class SpreadSheetInfo
    {
        [ReadOnly] public string BookName;
        [ReadOnly] public Languages Language;
        [ReadOnly] public List<SheetInfo> SheetInfos = new();
    }

    public class SheetInfo
    {
        [ReadOnly] public string Title;
        [ReadOnly] public Languages Language;
        [ReadOnly] public string Type;
        [ReadOnly] public string Section;
    }

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

            // SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            //
            // ValueRange response = request.Execute();
            // IList<IList<object>> values = response.Values;

            // load scriptable object <BookInfo> by this path Assets/Project/Configs/BookInfo
            string path = "Assets/Project/Configs/BookInfo.asset";
            if (!AssetDatabase.AssetPathExists(path))
            {
                Debug.Log($"BookInfo asset not found at path: {path}");
                BookInfo newBookInfo = ScriptableObject.CreateInstance<BookInfo>();
                AssetDatabase.CreateAsset(newBookInfo, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Created new BookInfo asset at path: {path}");
            }

            var bookInfo = AssetDatabase.LoadAssetAtPath<BookInfo>(path);
            if (bookInfo == null)
            {
                Debug.LogError("Failed to load BookInfo asset.");
                return null;
            }

            // Получаем информацию о всех листах в таблице
            var spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
            var sheets = spreadsheet.Sheets;

            // сравнить информацию с bookInfo и если есть хоть одно различие, надо полностью перечитать все листы
            
            
            
            
            foreach (var sheet in sheets)
            {
                var sheetTitle = sheet.Properties.Title;
                var checkRange = $"{sheetTitle}!B2";
                var checkRequest = service.Spreadsheets.Values.Get(spreadsheetId, checkRange);
                var checkResponse = await checkRequest.ExecuteAsync();
                var checkValues = checkResponse.Values;

//  !!! На этом этапе я знаю какое назначение у листа и могу выбрать.
//  !!! дальнейший кодо пойдет отличным для разного типа SheetToJson
                // Проверяем значение в A1, чтобы решить, пропускать ли лист
                if (checkValues != null && checkValues.Count > 0 && checkValues[0].Count > 0)
                {
                    var a1Value = checkValues[0][0].ToString();
                    if (string.Equals(a1Value, "Skip", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"Пропускаем лист: {sheetTitle}, так как в A1 указано 'Skip'.");
                        continue;
                    }
                }

                // Если лист не пропущен, читаем нужные данные
                // Замените "B4:Z" на ваш реальный диапазон данных
                string dataRange = $"{sheetTitle}!B4:Z";
                var dataRequest = service.Spreadsheets.Values.Get(spreadsheetId, dataRange);
                var dataResponse = await dataRequest.ExecuteAsync();
                var values = dataResponse.Values;

                if (values != null && values.Count > 0)
                {
                    var newSheet = new Sheet
                    {
                        Title = sheetTitle,
                        Rows = values.Select(r => new Row
                        {
                            LearnWord = r.Count > 0 ? r[0]?.ToString() : string.Empty,
                            Phonetics = r.Count > 1 ? r[1]?.ToString() : string.Empty,
                            Meaning = r.Count > 2 ? r[2]?.ToString() : string.Empty,
                            Image = r.Count > 3 ? r[3]?.ToString() : string.Empty,
                            Sound = r.Count > 4 ? r[4]?.ToString() : string.Empty,
                        }).ToList()
                    };
                    //book.Sheets.Add(newSheet);
                    Debug.Log($"Лист '{sheetTitle}' обработан.");
                }
            }

            return null;
        }

        public async UniTask<Book> _Get()
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
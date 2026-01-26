using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    public class SpreadSheetIdsData
    {
        public string spreadsheetId;
    }

    public class SpreadSheetInfoProvider
    {
        private const string ApplicationName = "Google Sheets API Unity";
        private const string Path = "Assets/Project/Configs/BookInfo.asset";

        private readonly string _spreadSheetIdFileName;
        private readonly string _jsonCredentialsFileName;

        public SheetsService Service { get; private set; }

        public SpreadSheetInfoProvider(string spreadSheetIdFileName, string jsonCredentialsFileName)
        {
            _spreadSheetIdFileName = spreadSheetIdFileName;
            _jsonCredentialsFileName = jsonCredentialsFileName;
        }

        /// <summary>
        ///  Compare book info cache with Google Sheets and update if necessary.
        /// </summary>
        public async UniTask<BookInfo> GetBookAsync()
        {
            string spreadsheetId = await GetSpreadSheetIdAsync(_spreadSheetIdFileName);
            GoogleCredential credential = await GetCredentialsAsync(_jsonCredentialsFileName);

            if (string.IsNullOrEmpty(spreadsheetId) || credential == null)
            {
                Debug.LogError($"Spreadsheet Id {spreadsheetId} or credential was not loaded from json files.");
                return null;
            }

            Service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            if (!AssetDatabase.AssetPathExists(Path))
            {
                Debug.Log($"BookInfo asset not found at path: {Path}");
                BookInfo newBookInfo = ScriptableObject.CreateInstance<BookInfo>();
                AssetDatabase.CreateAsset(newBookInfo, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Created new BookInfo asset at path: {Path}");
            }

            BookInfo bookInfo = AssetDatabase.LoadAssetAtPath<BookInfo>(Path);
            if (bookInfo == null)
            {
                Debug.LogError("Failed to load BookInfo asset.");
                return null;
            }

            // Получаем информацию о всех листах в таблице
            Spreadsheet spreadsheet = await Service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
            IList<Sheet> sheets = spreadsheet.Sheets;

            SpreadSheetInfo book = bookInfo.SpreadsheetInfos.FirstOrDefault(s => s.Title == spreadsheet.Properties.Title);
            // if dont have this book
            if (book == null)
            {
                Languages language = await GetBookLanguageAsync(spreadsheetId);
                List<SheetInfo> sheetInfos = await GetSheetsInfoAsync(spreadsheetId, sheets);
                // just populate info with that book
                var spreadSheetInfo = new SpreadSheetInfo
                {
                    Title = spreadsheet.Properties.Title,
                    Language = language,
                    Sheets = sheetInfos,
                };
                bookInfo.SpreadsheetInfos.Add(spreadSheetInfo);
            }

            // так же нужно проверить если книга есть, но возможно есть изменения на листах


            // сравнить информацию с bookInfo и если есть хоть одно различие, надо полностью перечитать все листы
            // foreach (Google.Apis.Sheets.v4.Data.Sheet sheet in sheets)
            // {
            //     var 
            // }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Updated BookInfo asset at path: {Path}");
            return bookInfo;
        }

        private async Task<Languages> GetBookLanguageAsync(string spreadsheetId)
        {
            string checkRange = "BookInfo!B1";
            SpreadsheetsResource.ValuesResource.GetRequest checkRequest = Service.Spreadsheets.Values.Get(spreadsheetId, checkRange);
            ValueRange checkResponse = await checkRequest.ExecuteAsync();
            IList<IList<object>> checkValues = checkResponse.Values;

            var value = checkValues[0][0].ToString();
            if (Enum.TryParse<Languages>(value, true, out var language))
            {
                return language;
            }

            Debug.LogError($"Error fetching spreadsheet language: {value}");
            return Languages.English;
        }

        private async UniTask<List<SheetInfo>> GetSheetsInfoAsync(string spreadsheetId, IList<Sheet> sheets)
        {
            List<SheetInfo> sheetsInfo = new();

            foreach (var sheet in sheets)
            {
                SheetInfo sheetInfo = new SheetInfo();
                sheetInfo.Title = sheet.Properties.Title;

                string checkRange = $"{sheetInfo.Title}!A1:B3";
                SpreadsheetsResource.ValuesResource.GetRequest checkRequest = Service.Spreadsheets.Values.Get(spreadsheetId, checkRange);
                ValueRange checkResponse = await checkRequest.ExecuteAsync();
                IList<IList<object>> checkValues = checkResponse.Values;

                try
                {
                    var value = checkValues[0][1].ToString();
                    if (Enum.TryParse<Languages>(value, true, out var language))
                    {
                        sheetInfo.Language = language;
                    }

                    sheetInfo.Type = checkValues[1][1].ToString();
                    sheetInfo.Section = checkValues[2][1].ToString();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                sheetsInfo.Add(sheetInfo);
            }

            return sheetsInfo;
        }
        
        private async UniTask<string> GetSpreadSheetIdAsync(string spreadSheetIdFileName)
        {
            string spreadsheetId;
            string spreadSheetIdsPath = System.IO.Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, spreadSheetIdFileName);

            try
            {
                await using FileStream idsStream = new FileStream(spreadSheetIdsPath, FileMode.Open, FileAccess.Read);
                string idsString = await new StreamReader(idsStream).ReadToEndAsync();
                SpreadSheetIdsData idsData = JsonUtility.FromJson<SpreadSheetIdsData>(idsString);
                spreadsheetId = idsData.spreadsheetId;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading spreadsheet ID: {e.Message}");
                throw;
            }

            return spreadsheetId;
        }

        private async UniTask<GoogleCredential> GetCredentialsAsync(string jsonCredentialsFileName)
        {
            GoogleCredential credential;
            string idsFullPath = System.IO.Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, jsonCredentialsFileName);

            try
            {
                await using var stream = new FileStream(idsFullPath, FileMode.Open, FileAccess.Read);
                credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading credentials: {e.Message}");
                throw;
            }

            return credential;
        }
    }
}
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    public class SpreadSheetIdsData
    {
        public string spreadsheetId;
    }

    public class SpreadSheetInfoProvider
    {
        public static async UniTask<string> GetSpreadSheetIdAsync(string spreadSheetIdFileName)
        {
            string spreadsheetId;
            string spreadSheetIdsPath = Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, spreadSheetIdFileName);

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

        public static async UniTask<GoogleCredential> GetCredentialsAsync(string jsonCredentialsFileName)
        {
            GoogleCredential credential;
            string idsFullPath = Path.Combine(Application.dataPath, UtilitiesConstants.RelativePath, jsonCredentialsFileName);

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
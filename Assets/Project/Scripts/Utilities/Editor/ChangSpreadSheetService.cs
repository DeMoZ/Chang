using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using UnityEngine;


namespace Chang.Utilities
{
    public class ChangSpreadSheetService : IDisposable
    {
        private const string ApplicationName = "Google Sheets API Unity";
        private const string TokenFileName = "gcloud_client_token.json";
        private const string IdFileName = "ids_ThaiVocabularyFood.json";

        private SheetsService _service;
        private string _spreadsheetId;
        private string[] Scopes { get; } = { SheetsService.Scope.SpreadsheetsReadonly };
        
        public void Dispose()
        {
            _service?.Dispose();
        }

        public async Task<Spreadsheet> GetSpreadsheet()
        {
            if (_service == null)
            {
                await InitService();
            }
            
            string IdsFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath, IdFileName);
            await using var idsStream = new FileStream(IdsFullPath, FileMode.Open, FileAccess.Read);
            var idsString = await new StreamReader(idsStream).ReadToEndAsync();
            var idsData = JsonUtility.FromJson<IdsData>(idsString);

            _spreadsheetId = idsData.spreadsheetId;
            // Sheet info
            return await _service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
        }

        public ValueRange GetRange(string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, range);

            // Get data from the sheet
            ValueRange response = request.Execute();
            return response;
        }
        
        private async Task InitService()
        {
            Debug.LogWarning($"Need to finish authorization in the browser");
            UserCredential credentialTask = await AuthorizeAsync();
            Debug.LogWarning($"Authorization finished");
            // Create Google Sheets API service.
            
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentialTask,
                ApplicationName = ApplicationName,
            });
        }
        
        private async Task<UserCredential> AuthorizeAsync()
        {
            UserCredential credentialTask;
            try
            {
                string secretFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath,
                    VocabularyUtilitiesConstants.SecretFileName);
                string tokenFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath, TokenFileName);

                // Load credentials from file
                await using var stream = new FileStream(secretFullPath, FileMode.Open, FileAccess.Read);
                var secretsTask = await GoogleClientSecrets.FromStreamAsync(stream);
                var secrets = secretsTask.Secrets;

                credentialTask = await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, Scopes, "user",
                    CancellationToken.None, new FileDataStore(tokenFullPath, true));

                Debug.Log($"Credential file saved to: {TokenFileName} at {tokenFullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to authorize: {e.Message}");
                throw;
            }

            return credentialTask;
        }
    }
}
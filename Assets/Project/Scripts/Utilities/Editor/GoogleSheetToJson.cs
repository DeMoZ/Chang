using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using UnityEngine;

public class GoogleSheetToJson
{
    protected string[] Scopes { get; } = { SheetsService.Scope.SpreadsheetsReadonly };

    static string ApplicationName = "Google Sheets API Unity";
    protected string TokenFileName = "gcloud_client_token.json";
    private string IdFileName = "ids.json";


    public async Task<IList<IList<object>>> TryGetWords()
    {
        var rows = new List<IList<object>>();

        Debug.LogWarning($"Need to finish authorization in the browser");
        UserCredential credentialTask = await AuthorizeAsync();
        Debug.LogWarning($"Authorization finished");
        // Create Google Sheets API service.
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentialTask,
            ApplicationName = ApplicationName,
        });

        string IdsFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath, IdFileName);
        await using var idsStream = new FileStream(IdsFullPath, FileMode.Open, FileAccess.Read);
        var idsString = await new StreamReader(idsStream).ReadToEndAsync();
        var idsData = JsonUtility.FromJson<IdsData>(idsString);

        var spreadsheetId = idsData.spreadsheetId;
        // Sheet info
        Spreadsheet spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
        var firstSheet = spreadsheet.Sheets.FirstOrDefault();
        if (firstSheet == null)
        {
            Debug.LogError($"There are no sheets in the document");
            return rows;
        }

        var firstSheetName = firstSheet.Properties.Title;
        Debug.Log(
            $"{nameof(firstSheetName)}: {firstSheetName}\nnow we use only first sheet");
        //var range = $"{firstSheetName}!A1:D10"; // Cells range on the page
        var range = $"{firstSheetName}"; // all the data from the page
        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);

        // Get data from the sheet
        ValueRange response = request.Execute();
        rows = response.Values.ToList();
        return rows;
    }

    protected async Task<UserCredential> AuthorizeAsync()
    {

        string secretFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath, VocabularyUtilitiesConstants.SecretFileName);
        string tokenFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath, TokenFileName);

        // Load credentials from file
        await using var stream = new FileStream(secretFullPath, FileMode.Open, FileAccess.Read);
        var secretsTask = await GoogleClientSecrets.FromStreamAsync(stream);
        var secrets = secretsTask.Secrets;

        UserCredential credentialTask = await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, Scopes, "user",
            CancellationToken.None, new FileDataStore(tokenFullPath, true));

        Debug.Log($"Credential file saved to: {TokenFileName} at {tokenFullPath}");
        return credentialTask;
    }
}
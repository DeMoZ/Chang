using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

using UnityEditor;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

public class GrabVocabFromGoogleSheetsJoufullThai
{
    static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    static string ApplicationName = "Google Sheets API Unity";
    private static string RelativePath = "Project/Scripts/Utilities/ChangExternal";
    private static string SecretFileName = "gcloud_client_secret.json";
    private static string TokenFileName = "gcloud_client_token.json";
    private static string IdFileName = "ids.json";
    private static int countLetters = 0;

    [MenuItem("Chang/Utilities/Create Words Json From Google Sheet")]
    public static async void ReadGoogleSheet()
    {
        countLetters = 0;

        string secretFullPath = Path.Combine(Application.dataPath, RelativePath, SecretFileName);
        string tokenFullPath = Path.Combine(Application.dataPath, RelativePath, TokenFileName);
        string IdsFullPath = Path.Combine(Application.dataPath, RelativePath, IdFileName);

        // Load credentials from file
        await using var stream = new FileStream(secretFullPath, FileMode.Open, FileAccess.Read);
        var secretsTask = await GoogleClientSecrets.FromStreamAsync(stream);
        var secrets = secretsTask.Secrets;

        Debug.LogWarning($"[{nameof(ReadGoogleSheet)}] need to finish authorization in the browser");
        var credentialTask = await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, Scopes, "user",
            CancellationToken.None,
            new FileDataStore(tokenFullPath, true));
        Debug.Log($"Credential file saved to: {TokenFileName} at {tokenFullPath}");
        Debug.LogWarning($"[{nameof(ReadGoogleSheet)}] authorization finished");
        // Create Google Sheets API service.
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentialTask,
            ApplicationName = ApplicationName,
        });

        await using var idsStream = new FileStream(IdsFullPath, FileMode.Open, FileAccess.Read);
        var idsString = await new StreamReader(idsStream).ReadToEndAsync();
        var idsData = JsonUtility.FromJson<IdsData>(idsString);

        var spreadsheetId = idsData.spreadsheetId;
        // Sheet info
        Spreadsheet spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
        var firstSheet = spreadsheet.Sheets.FirstOrDefault();
        if (firstSheet == null)
        {
            Debug.LogError($"[{nameof(ReadGoogleSheet)}] there are no sheets in the document");
            return;
        }

        var firstSheetName = firstSheet.Properties.Title;
        Debug.Log(
            $"[{nameof(ReadGoogleSheet)}] {nameof(firstSheetName)}: {firstSheetName}\nnow we use only first sheet");
        //var range = $"{firstSheetName}!A1:D10"; // Cells range on the page
        var range = $"{firstSheetName}"; // all the data from the page
        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);

        // Get data from the sheet
        ValueRange response = request.Execute();
        IList<IList<object>> rows = response.Values;

        if (rows is { Count: > 0 })
        {
            var data = FilterData(rows);
            WordConfigFileCreator.CreateJson(Languages.Thai, data);
        }
        else
        {
            Debug.Log("No data found.");
        }

        Debug.LogWarning($"[{nameof(ReadGoogleSheet)}] --- Done --- count letters: {countLetters}");
    }

    [MenuItem("Chang/Utilities/Create Word Configs from Json")]
    public static void CreateWordConfigs()
    {
        WordConfigFileCreator.ReadJsongAndCreateConfigs(Languages.Thai);
        Debug.LogWarning($"[{nameof(ReadGoogleSheet)}] --- Done ---");
    }

    private static List<PhraseData> FilterData(IList<IList<object>> rows)
    {
        var currentSection = string.Empty;
        var phrases = new List<PhraseData>();

        try
        {
            // var index = 0;
            foreach (var columns in rows)
            {
                // // todo remove test limit
                // if (index++ >= 10)
                //     break;

                var data = $"count coluns {columns.Count}; {string.Join(", ", columns)}";

                var cnt = columns.Count;
                if (cnt == 0)
                {
                    // skip this empty row
                    Debug.Log($"ignored : {data}");
                }
                else if (cnt == 1) // probably section
                {
                    Debug.Log($"new section : {data}");
                    currentSection = columns[0].ToString();
                }
                else if (cnt < 4)
                {
                    // "Not all fields
                    Debug.LogWarning($"ignored, Not all fields: {data}");
                }
                else
                {
                    if (string.IsNullOrEmpty(columns[1].ToString()) ||
                        string.IsNullOrEmpty(columns[2].ToString()) ||
                        string.IsNullOrEmpty(columns[3].ToString()))
                    {
                        Debug.LogError($"Empty value for {string.Join(",", columns)}");
                        continue;
                    }

                    var isNum = int.TryParse(columns[0].ToString(), out var num);
                    if (isNum) // so probably word in thar row
                    {
                        Debug.Log($"create config : {data}");
                        phrases.Add(CreatePhraseData(Languages.Thai, currentSection, columns));
                        countLetters += columns[1].ToString().Length;
                    }
                    else
                    {
                        // "No Thi Phonetic meaning" header for section and skip it
                        Debug.LogWarning($"ignored : {data}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"error : {e}");
        }

        return phrases;
    }

    private static PhraseData CreatePhraseData(Languages language, string currentSection, IList<object> columns)
    {
        var key = Regex.Replace(columns[3].ToString(), "[^a-zA-Z0-9-]", "_");
        // make the key shorter and unic
        if (key.Length > 20)
        {
            key = key.Substring(0, 15);
            var uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 5);
            key = $"{key}_{uniqueSuffix}";
        }

        return new PhraseData
        {
            Key = key,
            Language = language,
            Section = currentSection,
            Word = columns[1].ToString(),
            Phonetic = columns[2].ToString(),
            Meaning = columns[3].ToString(),
        };
    }
}

[Serializable]
public class PhraseData
{
    public string Key { get; set; }
    public Languages Language { get; set; }
    public string Section { get; set; } // Verbs, Objects, Quesction, Preposition...
    public string Word { get; set; }
    public string Phonetic { get; set; }
    public string Meaning { get; set; }
    public AudioClip AudioClip { get; set; }
}

public class IdsData
{
    public string spreadsheetId;
}
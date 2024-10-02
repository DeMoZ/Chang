using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

using System.Net.Http;

public class VocabularyUtilities
{
    private static int countLetters = 0;

    [MenuItem("Chang/Utilities/Create Words Json From Google Sheet")]
    public static async void ReadGoogleSheetAsync()
    {
        var gSheetsToJson = new GoogleSheetToJson();
        var result = gSheetsToJson.TryGetWords();
        await result;

        if (!result.Result.Any())
        {
            Debug.LogError($"[{nameof(ReadGoogleSheetAsync)}] --- Failed ---");
        }

        countLetters = 0;
        var rows = result.Result;
        if (rows is { Count: > 0 })
        {
            var data = FilterData(rows);
            WordConfigFileCreator.CreateJson(Languages.Thai, data);
        }
        else
        {
            Debug.Log("No data found.");
        }

        Debug.LogWarning($"[{nameof(ReadGoogleSheetAsync)}] --- Done --- count letters: {countLetters}");
    }

    [MenuItem("Chang/Utilities/Create Word Configs from Json")]
    public static void CreateWordConfigs()
    {
        Debug.LogWarning($"[{nameof(CreateWordConfigs)}] --- Start ---");
        WordConfigFileCreator.ReadJsongAndCreateConfigs(Languages.Thai);
        Debug.LogWarning($"[{nameof(CreateWordConfigs)}] --- Done ---");
    }

    [MenuItem("Chang/Utilities/Create Audio from Json")]
    public static async void CreateAudioFilesAsync()
    {
        Debug.LogWarning($"[{nameof(CreateAudioFilesAsync)}] --- Start ---");
        List<(string filename, string word)> dataset = await WordConfigFileCreator.GetDatasetForAudio(Languages.Thai);
        var textToVoice = new TextToVoice();
        // 2. from dataset to audio with saving each audio        
        //-------------------------------------
        var data = dataset[0];
        var filePath = Path.Combine(Application.dataPath, "Project", "Languages", "Thai", "WordsAudio", "New", data.filename + ".mp3");
        var result = await textToVoice.TryGetAudioAsync(data.word, "th-TH", filePath);
        if (!result)
        {
            Debug.LogError($"[{nameof(CreateAudioFilesAsync)}] --- Failed ---");
        }
        //-------------------------------------      

        Debug.LogWarning($"[{nameof(CreateAudioFilesAsync)}] --- Done ---");
    }



    public async Task GenerateAudioAsync(string thaiText)
    {
        // Use another TTS engine (e.g., AWS Polly or Azure Speech)
        // Assume we have an endpoint that accepts text and generates audio for Thai
        string ttsServiceUrl = "https://your-tts-service.com/api/generate";

        using var client = new HttpClient();
        var content = new StringContent($"{{ \"text\": \"{thaiText}\" }}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(ttsServiceUrl, content);

        if (response.IsSuccessStatusCode)
        {
            // Process the audio returned from the TTS service
            var audioStream = await response.Content.ReadAsStreamAsync();
            // Save or play the audio as needed
        }
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
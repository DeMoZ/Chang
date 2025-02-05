using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities
{
    public class VocabularyUtilities
    {
        private static int countLetters = 0;

        /// <summary>
        /// Reads Google book from Google Sheet and creates Json files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Create Sheets Jsons", false, 0)]
        public static async void ReadGoogleBookAsync()
        {
            var gSheetsToJson = new GoogleSheetToJson();
            var book = await gSheetsToJson.TryGetBook();

            foreach (var sheet in book.Sheets)
            {
                if (sheet.Properties.Skip || sheet.Rows.Count == 0)
                {
                    continue;
                }

                Debug.Log($"Sheet: {sheet.Title}");

                // Validation - remove rows with empty value in any cell
                sheet.Rows = sheet.Rows
                    .Where(row =>
                    {
                        if (string.IsNullOrEmpty(row.LearnWord) || string.IsNullOrEmpty(row.Phonetics) || string.IsNullOrEmpty(row.Meaning))
                        {
                            Debug.LogWarning($"Sheet: {sheet.Title}, Empty value for {row.LearnWord} {row.Phonetics} {row.Meaning}");
                            return false;
                        }

                        return true;
                    })
                    .ToList();

                WordConfigFileCreator.CreateSheetJson(sheet);
            }

            Debug.LogWarning($"[{nameof(ReadGoogleBookAsync)}] --- Done --- count letters: {countLetters}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // [MenuItem("Chang/Utilities/Create Word Configs from Json", false, 1)]
        // public static void CreateWordConfigs()
        // {
        //     Debug.LogWarning($"[{nameof(CreateWordConfigs)}] --- Start ---");
        //     WordConfigFileCreator.ReadJsonAndCreateConfigs(Languages.Thai);
        //     Debug.LogWarning($"[{nameof(CreateWordConfigs)}] --- Done ---");
        // }

        [MenuItem("Chang/Utilities/Create Audio from Json, false", false, 2)]
        public static async void CreateAudioFilesAsync()
        {
            Debug.LogWarning($"[{nameof(CreateAudioFilesAsync)}] --- Start ---");
            List<(string filename, string word)> dataset = await WordConfigFileCreator.GetDatasetForAudio(Languages.Thai);
            var textToVoice = new TextToVoice();
            // 2. from dataset to audio with saving each audio        
            //-------------------------------------
            var data = dataset[0];
            var filePath = Path.Combine(Application.dataPath, "Project", "Configs", "Thai", "WordsAudio", "New", data.filename + ".mp3");
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
                var uniqueSuffix = GetHash(key);
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

        private static int GetHash(string str)
        {
            unchecked
            {
                var hash = 23;

                foreach (var c in str)
                {
                    hash = (hash * 31) + c;
                    hash ^= (hash << 5) | (hash >> 3);
                }

                return hash < 0 ? -hash : hash; // Ensure the hash is positive
            }
        }
    }

    [Serializable]
    public class PhraseData
    {
        public string Key { get; set; }
        public Languages Language { get; set; }
        public string Section { get; set; } // Verbs, Objects, Question, Preposition...
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string Meaning { get; set; }
        public AudioClip AudioClip { get; set; }
    }

    public class IdsData
    {
        public string spreadsheetId;
    }
}
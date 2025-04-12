using System;
using System.Linq;
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

                ConfigFileCreator.CreateSheetJson(sheet);
            }

            Debug.LogWarning($"[{nameof(ReadGoogleBookAsync)}] --- Done --- count letters: {countLetters}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
/*
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
*/
    }

    [Serializable]
    public class PhraseData
    {
        public string Key { get; set; }
        public Languages Language { get; set; }
        public string Section { get; set; } // Food, Animals, Colors, Clothes etc
        public string Word { get; set; }
        public string Phonetics { get; set; }
        public string Meaning { get; set; }
    }

    public class IdsData
    {
        public string spreadsheetId;
    }
}
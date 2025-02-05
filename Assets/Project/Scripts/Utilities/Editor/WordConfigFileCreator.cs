using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities
{
    public static class WordConfigFileCreator
    {
        private static JsonSerializerSettings JsonSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        /// <summary>
        /// create json file for sheet
        /// </summary>
        /// <param name="data"></param>
        public static void CreateSheetJson(Sheet data)
        {
            var languagePrefix = data.Properties.Language.ToString();
            var path = Path.Combine(Application.dataPath,
                ChangUtilitiesConstants. RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.JsonFolder}",
                $"{languagePrefix}{ChangUtilitiesConstants.JsonFolder}{ChangUtilitiesConstants.NewFolder}",
                $"{data.Properties.Name}.json");
            
            var jsonData = JsonConvert.SerializeObject(data, JsonSettings);
            
            File.WriteAllText(path, jsonData);
        }

        // public static void CreateJson(Languages language, List<PhraseData> data)
        // {
        //     var path = GetWordsJsonFilePath(language);
        //     var jsonData = JsonConvert.SerializeObject(data, JsonSettings);
        //     File.WriteAllText(path, jsonData);
        // }

        public static async void ReadSheetsJsonsAndCreateConfigs()
        {
            // var languagePrefix = Languages.Thai.ToString();
            // var fromPath = Path.Combine(Application.dataPath,
            //     RelativePath,
            //     languagePrefix,
            //    
            //     $"{data.Properties.Name}.json");
            //     
            // if (!File.Exists(path))
            // {
            //     Debug.LogError($"File {path} does not exist");
            //     return;
            // }
            //
            // await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            // var dataString = await new StreamReader(stream).ReadToEndAsync();
            // var data = JsonConvert.DeserializeObject<List<PhraseData>>(dataString);
            //
            // try
            // {
            //     foreach (PhraseData phraseData in data)
            //     {
            //         Debug.Log($"{phraseData.Section}, {phraseData.Word}, {phraseData.Phonetic}, {phraseData.Meaning}");
            //         CreateConfig(language, phraseData.Key, phraseData);
            //     }
            // }
            // catch (Exception e)
            // {
            //     Debug.LogError($"{e}");
            // }
            // finally
            // {
            //     AssetDatabase.SaveAssets();
            //     AssetDatabase.Refresh();
            // }
        }
        
        
        public static async void ReadJsonAndCreateConfigs(Languages language)
        {
            var path = String.Empty; // GetWordsJsonFilePath(language);
            if (!File.Exists(path))
            {
                Debug.LogError($"File {path} does not exist");
                return;
            }

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var dataString = await new StreamReader(stream).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<List<PhraseData>>(dataString);

            try
            {
                foreach (PhraseData phraseData in data)
                {
                    Debug.Log($"{phraseData.Section}, {phraseData.Word}, {phraseData.Phonetic}, {phraseData.Meaning}");
                    CreateConfig(language, phraseData.Key, phraseData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{e}");
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static async Task<List<(string filename, string word)>> GetDatasetForAudio(Languages language)
        {
            var path = string.Empty;// GetWordsJsonFilePath(language);
            List<(string filename, string word)> dataset = new();
            if (!File.Exists(path))
            {
                Debug.LogError($"File {path} does not exist");
                return dataset;
            }

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var dataString = await new StreamReader(stream).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<List<PhraseData>>(dataString);

            // todo roman create dataset for audio
            foreach (PhraseData phraseData in data)
            {
                dataset.Add((phraseData.Key, phraseData.Word));
            }

            return dataset;
        }

        private static void CreateConfig(Languages language, string name, PhraseData phraseData, bool withDirtyAndSafe = false)
        {
            var meanings = new List<Translation>
            {
                new()
                {
                    Language = Languages.English,
                    Meaning = phraseData.Meaning
                }
            };

            var word = new WordConfig
            {
                Key = name,
                Word = phraseData.Word,
                Phonetic = phraseData.Phonetic,
                Meanings = meanings
            };

            var dataAsset = ScriptableObject.CreateInstance<PhraseConfig>();
            if (withDirtyAndSafe)
                EditorUtility.SetDirty(dataAsset);

            dataAsset.Key = phraseData.Key;
            dataAsset.Language = phraseData.Language;
            dataAsset.AudioClip = phraseData.AudioClip;
            dataAsset.Word = word;

            var relativePath = Path.Combine(
                ChangUtilitiesConstants.RelativePath,
                language.ToString(), 
                $"{language.ToString()}{ChangUtilitiesConstants.WordsFolder}",
                ChangUtilitiesConstants.NewFolder,
                $"{name}.asset");
            var assetRelativePath = Path.Combine(ChangUtilitiesConstants.AssetsFolder, relativePath);

            var path = Path.Combine(Application.dataPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(dataAsset, assetRelativePath);

            if (withDirtyAndSafe)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        // private static string GetWordsJsonFilePath(Languages language)
        // {
        //     var relativePath = Path.Combine(RelativePath, language.ToString(), $"{language.ToString()}{JsonFileName}");
        //     var path = Path.Combine(Application.dataPath, relativePath);
        //     return path;
        // }
    }
}
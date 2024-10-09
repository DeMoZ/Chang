using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Chang
{
    public static class WordConfigFileCreator
    {
        private static string RelativePath = "Project/Languages";
        private static string AssetsFolder = "Assets";
        private static string WordsFolder = "Words";
        private static string NewFolder = "New";

        private static string JsonFileName = "Words.json";

        public static void CreateJson(Languages language, List<PhraseData> data)
        {
            var path = GetWordsJsonFilePath(language);
            var jsonData = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, jsonData);
        }

        public async static void ReadJsongAndCreateConfigs(Languages language)
        {
            var path = GetWordsJsonFilePath(language);
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

        public async static Task<List<(string filename, string word)>> GetDatasetForAudio(Languages language)
        {
            var path = GetWordsJsonFilePath(language);
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

        public static void CreateConfig(Languages language, string name, PhraseData phraseData, bool withDirtyAndSafe = false)
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
                EngWord = name,
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

            var relativePath = Path.Combine(RelativePath, language.ToString(), WordsFolder, NewFolder, $"{name}.asset");
            var assetRelativePath = Path.Combine(AssetsFolder, relativePath);

            var path = Path.Combine(Application.dataPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(dataAsset, assetRelativePath);

            if (withDirtyAndSafe)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static string GetWordsJsonFilePath(Languages language)
        {
            var relativePath = Path.Combine(RelativePath, language.ToString(), JsonFileName);
            var path = Path.Combine(Application.dataPath, relativePath);
            return path;
        }
    }
}
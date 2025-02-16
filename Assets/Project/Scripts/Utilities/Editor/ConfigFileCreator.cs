using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities
{
    public static class ConfigFileCreator
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
            var jsonData = JsonConvert.SerializeObject(data, JsonSettings);
            string path = GetSheetJsonSystemFilePath(data.Properties.Language, data.Properties.Name);

            File.WriteAllText(path, jsonData);
        }

        public static async UniTask CreateWordConfigsFromSheet(Sheet data)
        {
            try
            {
                foreach (var row in data.Rows)
                {
                    Debug.Log($"{row.LearnWord}, {row.Phonetics}, {row.Meaning}");

                    var phraseData = CreatePhraseData(
                        data.Properties.Language,
                        data.Properties.Name,
                        row.LearnWord,
                        row.Phonetics,
                        row.Meaning);

                    CreateConfig(data.Properties.Language, phraseData.Key, phraseData);
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

        public static void CreateQuestSelectWordConfig(string name, QuestionConfig dataAsset)
        {
            var languagePrefix = dataAsset.Language.ToString();
            var fileName =
                $"{languagePrefix}{ChangUtilitiesConstants.Question}{ChangUtilitiesConstants.Select}{ChangUtilitiesConstants.WordsFolder}{name}";

            var pathOnly = Path.Combine(
                ChangUtilitiesConstants.AssetsFolder,
                ChangUtilitiesConstants.RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.Question}",
                $"{languagePrefix}{ChangUtilitiesConstants.Question}{ChangUtilitiesConstants.Select}{ChangUtilitiesConstants.WordsFolder}",
                $"{languagePrefix}{ChangUtilitiesConstants.Question}{ChangUtilitiesConstants.Select}{ChangUtilitiesConstants.WordsFolder}{ChangUtilitiesConstants.NewFolder}",
                dataAsset.Section);

            CreateFolders(pathOnly);
            AssetDatabase.CreateAsset(dataAsset, $"{pathOnly}/{fileName}.asset");
        }
        
        public static void CreateLessonConfig(string section, LessonConfig dataAsset)
        {
            var languagePrefix = dataAsset.Language.ToString();
            var fileName = $"{languagePrefix}{ChangUtilitiesConstants.Lesson}{dataAsset.Name}";

            var pathOnly = Path.Combine(
                ChangUtilitiesConstants.AssetsFolder,
                ChangUtilitiesConstants.RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.Lesson}",
                $"{languagePrefix}{ChangUtilitiesConstants.Lesson}{ChangUtilitiesConstants.NewFolder}",
                section);

            CreateFolders(pathOnly);
            AssetDatabase.CreateAsset(dataAsset, $"{pathOnly}/{fileName}.asset");
        }
        
        public static void CreateBookConfig(GameBookConfig dataAsset)
        {
            var languagePrefix = dataAsset.Language.ToString();
            var fileName = $"{languagePrefix}{ChangUtilitiesConstants.Book}{dataAsset.Name}";

            var pathOnly = Path.Combine(
                ChangUtilitiesConstants.AssetsFolder,
                ChangUtilitiesConstants.RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.Book}",
                $"{languagePrefix}{ChangUtilitiesConstants.Book}{ChangUtilitiesConstants.NewFolder}");

            CreateFolders(pathOnly);
            AssetDatabase.CreateAsset(dataAsset, $"{pathOnly}/{fileName}.asset");
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
                Phonetic = phraseData.Phonetics,
                Meanings = meanings
            };

            var dataAsset = ScriptableObject.CreateInstance<PhraseConfig>();
            if (withDirtyAndSafe)
                EditorUtility.SetDirty(dataAsset);

            dataAsset.Key = phraseData.Key;
            dataAsset.Language = phraseData.Language;
            dataAsset.Section = phraseData.Section;
            dataAsset.AudioClip = phraseData.AudioClip;
            dataAsset.Word = word;

            var folderSystemPath = GetNewWordFolderSystemPath(language, phraseData.Section);
            Directory.CreateDirectory(folderSystemPath);

            var assetFilePath = GetNewWordFilePath(language, phraseData.Section, name);
            AssetDatabase.CreateAsset(dataAsset, assetFilePath);

            if (withDirtyAndSafe)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static string GetSheetJsonSystemFilePath(Languages language, string name)
        {
            var languagePrefix = language.ToString();
            var path = Path.Combine(
                Application.dataPath,
                ChangUtilitiesConstants.RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.JsonFolder}",
                $"{languagePrefix}{ChangUtilitiesConstants.JsonFolder}{ChangUtilitiesConstants.NewFolder}",
                $"{name}.json");

            return path;
        }

        private static void CreateFolders(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string[] folders = path.Split('/');

            if (folders.Length == 0)
            {
                return;
            }

            if (!string.Equals(folders[0], "Assets"))
            {
                Debug.LogError($"Path should start with 'Assets'. Path:\n{path}");
                return;
            }

            string currentPath = folders[0];

            foreach (string folder in folders.Skip(1))
            {
                string folderPath = Path.Combine(currentPath, folder);
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folder);
                    Debug.Log($"Folders created at:\n{path}");
                    RestartEditAssetDatabase();
                }

                currentPath = folderPath;
            }
        }
        
        private static void RestartEditAssetDatabase()
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.StartAssetEditing();
        }

        private static void StopEditAssetDatabase()
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetNewWordFolderSystemPath(Languages language, string section)
        {
            var languagePrefix = language.ToString();
            var pathOnly = Path.Combine(
                Application.dataPath,
                ChangUtilitiesConstants.RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.WordsFolder}",
                $"{languagePrefix}{ChangUtilitiesConstants.WordsFolder}{ChangUtilitiesConstants.NewFolder}",
                section);

            return pathOnly;
        }

        private static string GetNewWordFilePath(Languages language, string section, string name)
        {
            var languagePrefix = language.ToString();
            var pathOnly = Path.Combine(
                ChangUtilitiesConstants.AssetsFolder,
                ChangUtilitiesConstants.RelativePath,
                languagePrefix,
                $"{languagePrefix}{ChangUtilitiesConstants.WordsFolder}",
                $"{languagePrefix}{ChangUtilitiesConstants.WordsFolder}{ChangUtilitiesConstants.NewFolder}",
                section,
                $"{name}.asset");

            return pathOnly;
        }

        public static PhraseData CreatePhraseData(Languages language, string currentSection, string word, string phonetics, string meaning)
        {
            var key = GetKey(meaning);
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
                Word = word,
                Phonetics = phonetics,
                Meaning = meaning,
            };
        }

        private static string GetKey(string meaning) => Regex.Replace(meaning, "[^a-zA-Z0-9-]", "_");

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

        /*
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
        */
    }
}
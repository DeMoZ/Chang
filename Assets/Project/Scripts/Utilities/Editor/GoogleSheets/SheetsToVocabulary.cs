using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Chang.Core;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    // get vocabulary data from Google sheets
    public class SheetsToVocabulary : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later

        private static int CountLetters = 0;
        private static string Path = $"Assets/Project/Configs/{Language}/Vocabulary.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary JSON", false, 0)]
        public static async void ReadAsync()
        {
            string methodName = nameof(ReadAsync);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {Path}");

            VocabularySheetToJson gSheetsToJson = new(Language);
            VocabularySheetToJson.Book book;

            try
            {
                book = await gSheetsToJson.Get();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching Google Sheets data: {ex.Message}");
                return;
            }

            if (!AssetDatabase.AssetPathExists(Path))
            {
                Debug.Log($"[{methodName}] BookInfo asset not found at path: {Path}");

                string folderPath = System.IO.Path.GetDirectoryName(Path);
                SpreadSheetUtilities.CreateFoldersRecursively(folderPath);

                VocabularyInfo asset = ScriptableObject.CreateInstance<VocabularyInfo>();
                AssetDatabase.CreateAsset(asset, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {Path}");
            }

            VocabularyInfo vocabularyInfo = AssetDatabase.LoadAssetAtPath<VocabularyInfo>(Path);
            if (vocabularyInfo == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyInfo.Language = book.Language;
            vocabularyInfo.Words = book.Sheets.SelectMany(sheet => sheet.Word).ToList();

            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done ---");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // get vocabulary book data from Google sheets (lessons)
    public class SheetsToVocabularyBook : IReadSheets
    {
        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary book JSON", false, 0)]
        public async void ReadAsync()
        {
            throw new NotImplementedException();
        }
    }

    // get sentences data from Google sheets
    public class SheetsToSentences : IReadSheets
    {
        public void ReadAsync()
        {
            throw new NotImplementedException();
        }
    }

    // get sentences book data from Google sheets (lessons)
    public class SheetsToSentencesBook : IReadSheets
    {
        public void ReadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
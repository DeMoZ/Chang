using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Chang.Core;
using Cysharp.Threading.Tasks;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    // get vocabulary data from Google sheets
    public class SheetsToVocabulary : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later

        private static string Path = $"Assets/Project/Configs/{Language}/Vocabulary.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary config", false, 0)]
        public static async UniTaskVoid ReadAsync()
        {
            string methodName = nameof(ReadAsync);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {Path}");

            VocabularySheetsProcess gSheetsToJson = new(Language);
            VocabularySheetsProcess.Book book;

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
            vocabularyInfo.Words = book.Sheets.SelectMany(sheet => sheet.Words).ToList();

            EditorUtility.SetDirty(vocabularyInfo);
            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done --- path: {Path}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
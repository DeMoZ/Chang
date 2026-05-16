using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Chang.Core;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    // get vocabulary data from Google sheets
    public class SheetsToVocabulary
    {
        private static string Path (Languages language)=> $"Assets/Project/Configs/{language}/Vocabulary.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        public static async Task ReadAsync(Languages language)
        {
            string methodName = nameof(ReadAsync);
            string path = Path(language);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {path}");

            VocabularySheetsProcess gSheetsToJson = new(language);
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

            if (!AssetDatabase.AssetPathExists(path))
            {
                Debug.Log($"[{methodName}] BookInfo asset not found at path: {path}");

                string folderPath = System.IO.Path.GetDirectoryName(path);
                SpreadSheetUtilities.CreateFoldersRecursively(folderPath);

                VocabularyInfo asset = ScriptableObject.CreateInstance<VocabularyInfo>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {path}");
            }

            VocabularyInfo vocabularyInfo = AssetDatabase.LoadAssetAtPath<VocabularyInfo>(path);
            if (vocabularyInfo == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyInfo.Language = book.Language;
            vocabularyInfo.Words = book.Sheets.SelectMany(sheet => sheet.Words).ToList();

            EditorUtility.SetDirty(vocabularyInfo);
            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done --- path: {path}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
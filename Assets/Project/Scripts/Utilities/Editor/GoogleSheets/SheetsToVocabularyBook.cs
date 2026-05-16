using System;
using System.Linq;
using System.Threading.Tasks;
using Chang.Core;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    // get vocabulary book data from Google sheets (lessons)
    public class SheetsToVocabularyBook
    {
        private static string Path(Languages language) => $"Assets/Project/Configs/{language}/VocabularyBook.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        public static async Task ReadAsync(Languages language)
        {
            string methodName = nameof(ReadAsync);
            string path = Path(language);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {path}");

            VocabularyBookSheetsProcess gSheetsToJson = new(language);
            VocabularyBookSheetsProcess.Book book;

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

                VocabularyBookData asset = ScriptableObject.CreateInstance<VocabularyBookData>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {path}");
            }

            VocabularyBookData vocabularyBookData = AssetDatabase.LoadAssetAtPath<VocabularyBookData>(path);
            if (vocabularyBookData == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyBookData.Language = book.Language;
            vocabularyBookData.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(vocabularyBookData);
            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done --- path: {path}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
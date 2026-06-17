using System;
using System.Linq;
using System.Threading.Tasks;
using Chang.GoogleSheets;
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

                VocabularyBook asset = ScriptableObject.CreateInstance<VocabularyBook>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {path}");
            }

            VocabularyBook vocabularyBook = AssetDatabase.LoadAssetAtPath<VocabularyBook>(path);
            if (vocabularyBook == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyBook.Language = book.Language;
            vocabularyBook.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(vocabularyBook);
            UnityEngine.Debug.LogWarning(
                $"[{nameof(SheetsToVocabularyBook)}][{nameof(ReadAsync)}] --- Done --- path: {path}",
                vocabularyBook);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
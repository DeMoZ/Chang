using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Chang.GoogleSheets;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    // get sentences book data from Google sheets (lessons)
    public class SheetsToSentencesBook
    {
        private static string Path(Languages language) => $"Assets/Project/Configs/{language}/SentencesBook.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        public static async Task ReadAsync(Languages language)
        {
            string methodName = nameof(ReadAsync);
            string path = Path(language);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {path}");

            SentencesBookSheetsProcess gSheetsToJson = new(language);
            SentencesBookSheetsProcess.Book book;

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

                SentencesBook asset = ScriptableObject.CreateInstance<SentencesBook>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {path}");
            }

            SentencesBook sentencesBook = AssetDatabase.LoadAssetAtPath<SentencesBook>(path);
            if (sentencesBook == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            sentencesBook.Language = book.Language;
            sentencesBook.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(sentencesBook);
            UnityEngine.Debug.LogWarning(
                $"[{nameof(SheetsToVocabulary)}][{nameof(ReadAsync)}] --- Done --- path: {path}", sentencesBook);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
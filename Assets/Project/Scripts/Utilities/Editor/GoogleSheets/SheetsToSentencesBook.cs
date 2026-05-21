using System;
using System.Linq;
using System.Threading.Tasks;
using Chang.Core;
using UnityEditor;
using UnityEngine;
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

                SentencesBookData asset = ScriptableObject.CreateInstance<SentencesBookData>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {path}");
            }

            SentencesBookData sentencesBookData = AssetDatabase.LoadAssetAtPath<SentencesBookData>(path);
            if (sentencesBookData == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            sentencesBookData.Language = book.Language;
            sentencesBookData.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(sentencesBookData);
            UnityEngine.Debug.LogWarning(
                $"[{nameof(SheetsToVocabulary)}][{nameof(ReadAsync)}] --- Done --- path: {path}", sentencesBookData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
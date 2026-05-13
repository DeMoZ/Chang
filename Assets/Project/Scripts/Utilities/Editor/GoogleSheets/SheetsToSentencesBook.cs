using System;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    // get sentences book data from Google sheets (lessons)
    public class SheetsToSentencesBook : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later

        private static string Path = $"Assets/Project/Configs/{Language}/SentencesBook.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Sentences Book config", false, 3)]
        public static async UniTaskVoid ReadAsync()
        {
            string methodName = nameof(ReadAsync);
            DMZLogger.Log($"[{methodName}] Start. SpreadSheet provided: {Path}");

            SentencesBookSheetsProcess gSheetsToJson = new(Language);
            SentencesBookSheetsProcess.Book book;

            try
            {
                book = await gSheetsToJson.Get();
            }
            catch (Exception ex)
            {
                DMZLogger.LogError($"Error fetching Google Sheets data: {ex.Message}");
                return;
            }

            if (!AssetDatabase.AssetPathExists(Path))
            {
                DMZLogger.Log($"[{methodName}] BookInfo asset not found at path: {Path}");

                string folderPath = System.IO.Path.GetDirectoryName(Path);
                SpreadSheetUtilities.CreateFoldersRecursively(folderPath);

                SentencesBookData asset = ScriptableObject.CreateInstance<SentencesBookData>();
                AssetDatabase.CreateAsset(asset, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DMZLogger.Log($"[{methodName}] Created new BookInfo asset at path: {Path}");
            }

            SentencesBookData vocabularyBookData = AssetDatabase.LoadAssetAtPath<SentencesBookData>(Path);
            if (vocabularyBookData == null)
            {
                DMZLogger.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyBookData.Language = book.Language;
            vocabularyBookData.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(vocabularyBookData);
            DMZLogger.LogWarning($"[{nameof(ReadAsync)}] --- Done --- path: {Path}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();        }
    }
}
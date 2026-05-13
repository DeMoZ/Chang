using System;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    // get vocabulary book data from Google sheets (lessons)
    public class SheetsToVocabularyBook : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later

        private static string Path = $"Assets/Project/Configs/{Language}/VocabularyBook.asset";
        
        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary book config", false, 1)]
        public static async UniTaskVoid ReadAsync()
        {
            string methodName = nameof(ReadAsync);
            DMZLogger.Log($"[{methodName}] Start. SpreadSheet provided: {Path}");

            VocabularyBookSheetsProcess gSheetsToJson = new(Language);
            VocabularyBookSheetsProcess.Book book;

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

                VocabularyBookData asset = ScriptableObject.CreateInstance<VocabularyBookData>();
                AssetDatabase.CreateAsset(asset, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DMZLogger.Log($"[{methodName}] Created new BookInfo asset at path: {Path}");
            }

            VocabularyBookData vocabularyBookData = AssetDatabase.LoadAssetAtPath<VocabularyBookData>(Path);
            if (vocabularyBookData == null)
            {
                DMZLogger.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyBookData.Language = book.Language;
            vocabularyBookData.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(vocabularyBookData);
            DMZLogger.LogWarning($"[{nameof(ReadAsync)}] --- Done --- path: {Path}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
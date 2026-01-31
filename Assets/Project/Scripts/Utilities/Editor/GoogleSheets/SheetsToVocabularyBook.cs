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
        [MenuItem("Chang/Utilities/Create Vocabulary book JSON", false, 0)]
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

                VocabularyBookInfo asset = ScriptableObject.CreateInstance<VocabularyBookInfo>();
                AssetDatabase.CreateAsset(asset, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DMZLogger.Log($"[{methodName}] Created new BookInfo asset at path: {Path}");
            }

            VocabularyBookInfo vocabularyBookInfo = AssetDatabase.LoadAssetAtPath<VocabularyBookInfo>(Path);
            if (vocabularyBookInfo == null)
            {
                DMZLogger.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyBookInfo.Language = book.Language;
            vocabularyBookInfo.Sections = book.Sheets.SelectMany(sheet => sheet.Sections).ToList();

            EditorUtility.SetDirty(vocabularyBookInfo);
            DMZLogger.LogWarning($"[{nameof(ReadAsync)}] --- Done ---");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
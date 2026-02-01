using System;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    // get sentences data from Google sheets
    public class SheetsToSentences : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later

        private static string Path = $"Assets/Project/Configs/{Language}/Sentences.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/[NOT Implemented] Create Sentences JSON", false, 2)]
        public static async UniTaskVoid ReadAsync()
        {
            string methodName = nameof(ReadAsync);
            DMZLogger.Log($"[{methodName}] Start. SpreadSheet provided: {Path}");

            SentencesSheetsProcess gSheetsToJson = new(Language);
            SentencesSheetsProcess.Book book;

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

                SentencesInfo asset = ScriptableObject.CreateInstance<SentencesInfo>();
                AssetDatabase.CreateAsset(asset, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DMZLogger.Log($"[{methodName}] Created new BookInfo asset at path: {Path}");
            }

            SentencesInfo vocabularyInfo = AssetDatabase.LoadAssetAtPath<SentencesInfo>(Path);
            if (vocabularyInfo == null)
            {
                DMZLogger.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            vocabularyInfo.Language = book.Language;
            // vocabularyInfo.Sentences = book.Sheets.SelectMany(sheet => sheet.Sentences).ToList(); // todo chang uncomment and implement when ready

            EditorUtility.SetDirty(vocabularyInfo);
            DMZLogger.LogWarning($"[{nameof(ReadAsync)}] --- Done ---");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
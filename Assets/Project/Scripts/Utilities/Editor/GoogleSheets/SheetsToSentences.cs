using System;
using System.Linq;
using Chang.Core;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

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
        [MenuItem("Chang/Utilities/Create Sentences JSON", false, 2)]
        public static async UniTaskVoid ReadAsync()
        {
            string methodName = nameof(ReadAsync);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {Path}");

            SentencesSheetsProcess gSheetsToJson = new(Language);
            SentencesSheetsProcess.Book book;

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

                SentencesInfo asset = ScriptableObject.CreateInstance<SentencesInfo>();
                AssetDatabase.CreateAsset(asset, Path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {Path}");
            }

            SentencesInfo sentenceInfo = AssetDatabase.LoadAssetAtPath<SentencesInfo>(Path);
            if (sentenceInfo == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            sentenceInfo.Language = book.Language;
            sentenceInfo.Sentences = book.Sheets.SelectMany(sheet => sheet.Sentences).ToList();

            EditorUtility.SetDirty(sentenceInfo);
            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done ---");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
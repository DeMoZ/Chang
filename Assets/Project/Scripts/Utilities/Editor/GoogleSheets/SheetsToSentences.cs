using System;
using System.Linq;
using System.Threading.Tasks;
using Chang.Core;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    // get sentences data from Google sheets
    public class SheetsToSentences
    {
        private static string Path(Languages language) => $"Assets/Project/Configs/{language}/Sentences.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        public static async Task ReadAsync(Languages language)
        {
            string methodName = nameof(ReadAsync);
            string path = Path(language);
            Debug.Log($"[{methodName}] Start. SpreadSheet provided: {path}");

            SentencesSheetsProcess gSheetsToJson = new(language);
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

            if (!AssetDatabase.AssetPathExists(path))
            {
                Debug.Log($"[{methodName}] BookInfo asset not found at path: {path}");

                string folderPath = System.IO.Path.GetDirectoryName(path);
                SpreadSheetUtilities.CreateFoldersRecursively(folderPath);

                SentencesInfo asset = ScriptableObject.CreateInstance<SentencesInfo>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[{methodName}] Created new BookInfo asset at path: {path}");
            }

            SentencesInfo sentenceInfo = AssetDatabase.LoadAssetAtPath<SentencesInfo>(path);
            if (sentenceInfo == null)
            {
                Debug.LogError($"[{methodName}] Failed to load BookInfo asset.");
            }

            sentenceInfo.Language = book.Language;
            sentenceInfo.Sentences = book.Sheets.SelectMany(sheet => sheet.Sentences).ToList();

            EditorUtility.SetDirty(sentenceInfo);
            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done --- path: {path}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    // get vocabulary data from Google sheets
    public class SheetsToVocabulary : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later
        
        private static int CountLetters = 0;

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary JSON", false, 0)]
        public static async void ReadAsync()
        {
            VocabularySheetToJson gSheetsToJson = new(Language);
            VocabularySheetToJson.Book book;

            try
            {
                book = await gSheetsToJson.Get();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching Google Sheets data: {ex.Message}");
                return;
            }

            foreach (var sheet in book.Sheets)
            {
                //     if (sheet.Properties.Skip || sheet.Rows.Count == 0)
                //     {
                //         continue;
                //     }
                //
                //     Debug.Log($"Sheet: {sheet.Title}");
                //
                //     // Validation - remove rows with empty value in any cell
                //     sheet.Rows = sheet.Rows
                //         .Where(row =>
                //         {
                //             if (string.IsNullOrEmpty(row.LearnWord) || string.IsNullOrEmpty(row.Phonetics) || string.IsNullOrEmpty(row.Meaning))
                //             {
                //                 Debug.LogWarning($"Sheet: {sheet.Title}, Empty value for {row.LearnWord} {row.Phonetics} {row.Meaning}");
                //                 return false;
                //             }
                //
                //             return true;
                //         })
                //         .ToList();
                //
                //     ConfigFileCreator.CreateSheetJson(sheet);
            }

            Debug.LogWarning($"[{nameof(ReadAsync)}] --- Done ---");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // get vocabulary book data from Google sheets (lessons)
    public class SheetsToVocabularyBook : IReadSheets
    {
        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary book JSON", false, 0)]
        public async void ReadAsync()
        {
            throw new NotImplementedException();
        }
    }

    // get sentences data from Google sheets
    public class SheetsToSentences : IReadSheets
    {
        public void ReadAsync()
        {
            throw new NotImplementedException();
        }
    }

    // get sentences book data from Google sheets (lessons)
    public class SheetsToSentencesBook : IReadSheets
    {
        public void ReadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Chang.Utilities.GoogleSheets
{
    // get sentences data from Google sheets
    public class SheetsToSentences : IReadSheets
    {
        private const Languages Language = Languages.Thai; // todo chang. for now only Thai. Implement selection later

        private static string Path = $"Assets/Project/Configs/{Language}/VocabularyBook.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/[NOT Implemented] Create Sentences JSON", false, 2)]
        public static async UniTaskVoid ReadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
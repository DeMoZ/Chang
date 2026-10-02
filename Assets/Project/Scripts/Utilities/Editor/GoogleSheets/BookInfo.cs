using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    /// <summary>
    /// Contains information about the book obtained from Google Sheets.
    /// </summary>
    [CreateAssetMenu(fileName = "BookInfo", menuName = "Chang/Utilities/Google Sheets/Book Info", order = 0)]
    public class BookInfo : SerializedScriptableObject
    {
        public List<SpreadSheetInfo> SpreadsheetInfos = new();
    }

    public class SpreadSheetInfo
    {
        [ReadOnly] public string Title;
        [ReadOnly] public Languages Language;
        [ReadOnly] public List<SheetInfo> Sheets = new();
    }

    public class SheetInfo
    {
        [ReadOnly] public string Title;
        [ReadOnly] public Languages Language;
        [ReadOnly] public string Type;
        [ReadOnly] public string Section;
    }
}
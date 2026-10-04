using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Chang.Utilities.GoogleSheets
{
    /// <summary>
    /// Contains information about the book obtained from Google Sheets.
    /// </summary>
    [CreateAssetMenu(fileName = "BookInfo", menuName = "Chang/Utilities/Google Sheets/Book Info", order = 0)]
    public class BookInfo : ScriptableObject
    {
        public List<SpreadSheetInfo> SpreadsheetInfos = new();
    }

    [Serializable]
    public class SpreadSheetInfo
    {
        [ReadOnly] public string Title;
        [ReadOnly] public Languages Language;
        [ReadOnly] public List<SheetInfo> Sheets = new();
    }

    [Serializable]
    public class SheetInfo
    {
        [ReadOnly] public string Title;
        [ReadOnly] public Languages Language;
        [ReadOnly] public string Type;
        [ReadOnly] public string Section;
    }
}
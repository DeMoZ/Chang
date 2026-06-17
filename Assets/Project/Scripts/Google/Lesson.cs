using System.Collections.Generic;

namespace Chang.GoogleSheets
{
    /// <summary>
    /// Book contanis lessons that determined by sections in Google Sheets page.
    /// </summary>
    public class Lesson
    {
        public Languages Language;
        public string Section;
        public List<string> Keys;
    }
}
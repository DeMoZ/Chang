using System.Collections.Generic;

namespace Chang.Core
{
    /// <summary>
    /// Book contanis lessons that determined by sections in Google Sheets page.
    /// </summary>
    public class SectionLesson
    {
        public Languages Language;
        public string Section; 
        public List<string> Keys;
    }
}
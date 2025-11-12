using System.Collections.Generic;

namespace Chang.Sentences
{
    public class SentencesBookData
    {
        // public string FileName; // json field
        public List<Section> Sections;
        // public Languages Language;
        //
        // private Dictionary<string, SimpleSection> _sectionsDict;
        // public Dictionary<string, SimpleSection> SectionsDict => _sectionsDict ??= Sections.ToDictionary(s => s.Section);
    }
    
    public class Section
    {
        public string SectionName;
        public List<LessonData> Lessons;
    }

    public class LessonData
    {
        
    }
}
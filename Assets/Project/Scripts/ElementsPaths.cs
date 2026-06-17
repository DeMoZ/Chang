namespace Chang.Core
{
    public static class ElementsPaths
    {
        public static string VocabularySectionKey(Languages language, string sectionName) =>
            $"{language}/VocabularyBook/{sectionName}";
        
        public static string LessonKey(Languages language, string sectionName, int lessonIndex) =>
            $"{language}Lesson{sectionName}_{lessonIndex}";
        
    }
}
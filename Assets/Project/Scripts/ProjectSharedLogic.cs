using System.Collections.Generic;

public static class ProjectSharedLogic
{
    public static string SENTENCE_LESSON_KEY(string learnLanguage, string sectionName, int index) => $"{learnLanguage}.{sectionName}.{index}";
    public static string SENTENCE_QUESTION_KEY(string learnLanguage, string sectionName, List<string> words) => $"{learnLanguage}/{sectionName}/{string.Join("_", words)}";
}
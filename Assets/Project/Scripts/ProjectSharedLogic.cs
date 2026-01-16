using System.Collections.Generic;

/// <summary>
/// VOCABULARY_QUESTION_LOCALIZATION_KEY and SENTENCE_QUESTION_LOCALIZATION_KEY are used for i2Localization keys.
/// </summary>

public static class ProjectSharedLogic
{
    public static string VOCABULARY_QUESTION_LOCALIZATION_KEY(string learnLanguage, string sectionName, string word) => $"{learnLanguage}/Vocabulary/{sectionName}/{word}";
    
    public static string SENTENCE_LESSON_KEY(string learnLanguage, string sectionName, int index) => $"{learnLanguage}.{sectionName}.{index}";
    public static string SENTENCE_QUESTION_KEY(string learnLanguage, string sectionName, List<string> words) => $"{learnLanguage}/{sectionName}/{string.Join("_", words)}";
    public static string SENTENCE_QUESTION_LOCALIZATION_KEY(string learnLanguage, string sectionName, List<string> words) => $"{learnLanguage}/Sentences/{sectionName}/{string.Join("_", words)}";
}
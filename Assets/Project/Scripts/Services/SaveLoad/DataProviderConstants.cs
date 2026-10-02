namespace Chang.Services.DataProvider
{
    public class DataProviderConstants
    {
        public const string ProfileDataKey = "ProfileData";
        public const string VocabularyProgressDataKey = "VocabularyProgressData";
        public const string SentencesProgressDataKey = "SentencesProgressData";

        public static string VocabularyProgressKey(Languages language) => $"{language}_{VocabularyProgressDataKey}";
        public static string SentencesProgressKey(Languages language) => $"{language}_{SentencesProgressDataKey}";
    }
}

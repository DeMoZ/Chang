namespace Chang.Services.DataProvider
{
    public class DataProviderConstants
    {
        public const string ProfileDataKey = "ProfileData";
        public const string VocabularyProgressDataKey = "VocabularyProgressData";
        public const string SentencesProgressDataKey = "SentencesProgressData";
        /// <summary>local only: the data of a guest who signed in to an existing account, see <c>ProfileService</c></summary>
        public const string PendingGuestDataKey = "PendingGuestData";

        public static string VocabularyProgressKey(Languages language) => $"{language}_{VocabularyProgressDataKey}";
        public static string SentencesProgressKey(Languages language) => $"{language}_{SentencesProgressDataKey}";
    }
}

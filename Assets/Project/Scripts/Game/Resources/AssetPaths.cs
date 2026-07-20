namespace Chang.Resources
{
    public static class AssetPaths
    {
        public static class Constants
        {
            public const string Vocabulary = "Vocabulary";
            public const string Sentences = "Sentences";
            public const string VocabularyBook = "VocabularyBook";
            public const string SentencesBook = "SentencesBook";
        }
        
        public static class Utilities
        {
            public const string AssetsFolder = "Assets";
            public const string RelativePath = "Project/Configs";
            public const string WordsFolder = "Words";
            public const string NewFolder = "New";
            public const string JsonFolder = "Jsons";

            public const string Question = "Quest";
            public const string Select = "Select";

            public const string Lesson = "Lesson";

            public const string Book = "Book";
            public const string Sound = "Sound";
            
            public const string Root = "Assets/Project/Configs/";
            
            public static string VocabularyPath(Languages language) =>
                $"{Root}{language}/{Constants.Vocabulary}.asset";

            public static string SentencesPath(Languages language) => 
                $"{Root}{language}/{Constants.Sentences}.asset";
            
            public static string VocabularyBookPath(Languages language) =>
                $"{Root}{language}/{Constants.VocabularyBook}.asset";

            public static string SentencesBookPath(Languages language) =>
                $"{Root}{language}/{Constants.SentencesBook}.asset";
        }

        public static class Addressables
        {
            public const string Root = "Assets/Project/Resources_Bundled/";
            public const string Words = "Words/";
            public const string SoundWords = "SoundWords/";
            public const string ImageWords = "ImageWords/";

            public const string EmptyWordPlaceHolder = "EmptyWordPlaceHolder.asset";
            public const string EmptyWordPlaceHolderPath = Root + "EmptyWordPlaceHolder.asset";

            public static string VocabularyPath(Languages language) =>
                $"{Root}{language}/{Constants.Vocabulary}.asset";

            public static string SentencesPath(Languages language) => 
                $"{Root}{language}/{Constants.Sentences}.asset";
            
            public static string VocabularyBookPath(Languages language) =>
                $"{Root}{language}/{Constants.VocabularyBook}.asset";

            public static string SentencesBookPath(Languages language) =>
                $"{Root}{language}/{Constants.SentencesBook}.asset";
        }

        public static class Resources
        {
            public const string CreateAssetMenuName = "Chang/";
            public const string MissingSpriteLinkHolder = "MissingSpriteLinkHolder";
        }
    }
}
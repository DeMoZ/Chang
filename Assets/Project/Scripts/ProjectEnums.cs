namespace Chang
{
    public enum GenderType
    {
        No,
        Female,
        Male,
    }

    public enum ChangTypes
    {
        None,
        Vocabulary,
        Sentences,
        VocabularyBook,
        SentencesBook,
        
        DemonstrationWord,
        SelectWord,
        MatchWords,
        DemonstrationDialogue,
        SentenceSelectWords,
        
        Result = 100,
    }

    public enum PreloadType
    {
        None,
        Boot,       // Run game, preload all that need for the game on bootstrap
        LessonData,
    }

    /// <summary>
    /// Names match the localization sheet columns and SystemLanguage where it has the language.
    /// Serialized as int in the content assets, add new values to the end
    /// </summary>
    public enum Languages
    {
        English,
        Spanish,
        Russian,
        ChineseSimplified,
        Hindi,
        French,
        Thai,
        German,
        ChineseTraditional,
        Malay,
        Indonesian,
        Korean,
        Japanese,
        Lao,
        Vietnamese,
    }

    /// <summary>
    /// Voice of the word sounds, the name is the folder in SoundWords/{Language}/{Voice}/
    /// </summary>
    public enum SoundVoices
    {
        Female,
        Male,
    }

    public enum MainTabType
    {
        None,
        Vocabulary,
        Sentences,
        Repetition,
        Profile,
    }

    /// <summary>
    /// What is this game. Is it learning or repetition?
    /// </summary>
    public enum GameType
    {
        Learn,
        Repetition,
    } 
}
namespace Chang
{
    public enum GenderType
    {
        None,
        Male,
        Female,
    }

    public enum QuestionType
    {
        None,
        DemonstrationWord,
        SelectWord,
        MatchWords,
        DemonstrationDialogue,
    }

    // todo roman use system languages
    public enum PreloadType
    {
        None,
        Boot,       // Run game, preload all that need for the game on bootstrap
        // Lobby,      // Enter the lobby, load book
        LessonConfig,     // Start lesson, load LessonConfig file // todo roman refactor to use preloader in a different way. May be set the model with content names to donwload
        QuestConfigs,     // Start lesson, load Question Configs files separately
    }

    public enum Languages
    {
        English,
        Spanish,
        Russian,
        Chinese,
        Indian,
        Franche,
        Thai
    }
}
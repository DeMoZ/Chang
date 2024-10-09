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

    public enum PreloadType
    {
        None,
        Boot,       // Run game, preload all that need for the game on bootstrap
        // Lobby,      // Enter the lobby, load book
        Lesson,     // Start lesson, load all media for lesson
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
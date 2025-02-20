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
        
        Result = 100,
    }

    // todo chang use system languages
    public enum PreloadType
    {
        None,
        Boot,       // Run game, preload all that need for the game on bootstrap
        LessonData,
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

    public enum MainTabType
    {
        None,
        Lessons,
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
using System.Collections.Generic;

namespace Chang
{
    public class GameBus
    {
        public SimpleBookData SimpleBookData { get; set; }

        /// <summary>
        /// Runtime collection to get Lesson data by name.
        /// </summary>
        public Dictionary<string, SimpleLessonData> SimpleLessons { get; set; }
        
        public Lesson CurrentLesson { get; set; }
        // public PreloadType PreloadFor { get; set; }
        public GameType GameType { get; set; }
        public Languages CurrentLanguage { get; set; } = Languages.Thai; // todo chang need to select language in game
    }
}
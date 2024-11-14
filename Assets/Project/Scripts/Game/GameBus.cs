using System.Collections.Generic;

namespace Chang
{
    public class GameBus
    {
        public SimpleBookData SimpleBookData { get; set; }
        public Dictionary<string, SimpleLessonData> Lessons { get; set; }
        public Lesson CurrentLesson { get; set; } = new();
        public PreloadType PreloadFor { get; set; }
    }
}
using System.Collections.Generic;

namespace Chang
{
    public class GameModel
    {
        public List<LessonName> LessonNames { get; set; }
        public PreloadType PreloadType { get; set; }
        public int NextLessonIndex { get; set; }
    }
}
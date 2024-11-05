using System.Collections.Generic;
using Zenject;

namespace Chang
{
    public class GameBus
    {
        public ScreenManager ScreenManager { get; }
        public SimplifiedBookData SimplifiedBookData { get; set; }
        public Dictionary<string, SimplifiedLessonData> Lessons { get; set; }
        public Lesson CurrentLesson { get; set; } = new();
        public PreloadType PreloadFor { get; set; }
        
        [Inject]
        public GameBus(ScreenManager screenManager)
        {
            ScreenManager = screenManager;
        }
    }
}
using System.Collections.Generic;
using Zenject;

namespace Chang
{
    public class GameBus
    {
        public ScreenManager ScreenManager { get; }
        public SimpleBookData SimpleBookData { get; set; }
        public Dictionary<string, SimpleLessonData> Lessons { get; set; }
        public Lesson CurrentLesson { get; set; } = new();
        public PreloadType PreloadFor { get; set; }
        
        [Inject]
        public GameBus(ScreenManager screenManager)
        {
            ScreenManager = screenManager;
        }
    }
}
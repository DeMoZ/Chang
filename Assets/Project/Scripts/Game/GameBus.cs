using System.Collections.Generic;
using Zenject;

namespace Chang
{
    public class GameBus
    {
        public ScreenManager ScreenManager { get; }
        public List<LessonName> LessonNames { get; set; }
        public PreloadType PreloadFor { get; set; }
        public int ClickedLessonIndex { get; set; }
        public LessonConfig ClickedLessonConfig { get; set; }

        public string ClickedLesson => LessonNames[ClickedLessonIndex].FileName;

        [Inject]
        public GameBus(ScreenManager screenManager)
        {
            ScreenManager = screenManager;
        }
    }
}
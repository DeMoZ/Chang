using System.Collections.Generic;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.GameBook
{
    public class GameBookController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly GameBookView _view;
        private Dictionary<string, SimpleLessonData> _lessons = new();

        [Inject]
        public GameBookController(GameBus gameBus, MainScreenBus mainScreenBus, GameBookView view)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
        }

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Set()
        {
            _lessons.Clear();
            _view.Clear();

            foreach (var section in _gameBus.SimpleBookData.Sections)
            {
                var sectionItem = _view.InstantiateSection();
                sectionItem.Init(section.Section); // todo cahng button?
                sectionItem.name = section.Section;

                for (int i = 0; i < section.Lessons.Count; i++)
                {
                    var key = $"{section.Section}_{i + 1}";
                    _lessons[key] = section.Lessons[i];

                    var lessonItem = _view.InstantiateLesson();
                    lessonItem.Init(key, (i + 1).ToString(), 0, OnItemClick);
                }
            }

            // todo chang top section need to resubscribe on items pass it 
        }

        private void OnItemClick(string key)
        {
            Debug.Log($"Clicked on item {key}");
            _mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[key].FileName);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class GameBookController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly GameBookView _view;
        private List<SimpleLessonData> _lessons;

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
            _view.Init(OnItemClick);
        }

        public void Set()
        {
            _lessons = _gameBus.SimpleBookData.Lessons;
            var fileNames = _lessons.Select(n => n.FileName).ToList();
            _view.Set(fileNames);
        }

        private void OnItemClick(int index)
        {
            Debug.Log($"Clicked on item {index}");

            _mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[index].FileName);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}
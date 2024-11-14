using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class GameBookController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private GameBookView _view;
        private List<SimpleLessonData> _lessons;
        
        private Action<string> _onItemClick;
        private bool _isLoading;

        [Inject]
        public GameBookController(GameBus gameBus, MainScreenBus mainScreenBus, GameBookView view)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
        }

        public void Init(List<SimpleLessonData> lessons, Action<string> onItemClick)
        {
            _lessons = lessons;
            _onItemClick = onItemClick;

            var fileNames = lessons.Select(n => n.FileName).ToList();
            _view.Init(OnItemClick);
            _view.Set(fileNames);
        }

        private void OnItemClick(int index)
        {
            Debug.Log($"Clicked on item {index}");
            
            _onItemClick?.Invoke(_lessons[index].FileName);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(List<SimpleLessonData> lessonNames, object onLessonClick)
        {
            throw new NotImplementedException();
        }

         // todo roman this from GameBookController
         private async UniTaskVoid OnLessonClickAsync(string name)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);
            _gameBus.CurrentLesson.SetFileName(name);
            _gameBus.CurrentLesson.SetSimpQuesitons(_gameBus.Lessons[name].Questions);

            _gameBus.PreloadFor = PreloadType.LessonConfig;
            
            _isLoading = false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
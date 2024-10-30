using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class GameBookController : IViewController
    {
        private GameBookView _view;
        private List<SimplifiedLessonData> _lessons;
        
        private Action<string> _onItemClick;

        [Inject]
        public GameBookController(GameBookView view)
        {
            _view = view;
        }

        public void Init(List<SimplifiedLessonData> lessons, Action<string> onItemClick)
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

        internal void Init(List<SimplifiedLessonData> lessonNames, object onLessonClick)
        {
            throw new NotImplementedException();
        }
    }
}
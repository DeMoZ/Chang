using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class GameBookController
    {
        private GameBookView _view;
        private Action<int> _onItemClick;

        [Inject]
        public GameBookController(GameBookView view)
        {
            _view = view;
        }

        public void Init(List<LessonName> names, Action<int> onItemClick)
        {
            _onItemClick = onItemClick;

            var fileNames = names.Select(n => n.FileName).ToList();
            _view.Init(OnItemClick);
            _view.Set(fileNames);
        }

        private void OnItemClick(int index)
        {
            Debug.Log($"Clicked on item {index}");
            _onItemClick?.Invoke(index);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        internal void Init(List<LessonName> lessonNames, object onLessonClick)
        {
            throw new NotImplementedException();
        }
    }
}
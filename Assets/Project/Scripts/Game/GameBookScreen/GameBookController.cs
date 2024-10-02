using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class GameBookController
    {
        private GameBookView _view;

        [Inject]
        public GameBookController(GameBookView gameBookView)
        {
            _view = gameBookView;
        }

        public void Init(List<LessonName> names)
        {
            var fileNames = names.Select(n => n.FileName).ToList();
            _view.Init(OnItemClick);
            _view.Set(fileNames);
        }

        private void OnItemClick(int index)
        {
            Debug.Log($"Clicked on item {index}");
        }

        public void SetViewActive(bool acive)
        {
            _view.gameObject.SetActive(true);
        }
    }
}
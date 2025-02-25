using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
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
        private List<GameBookSection> _sectionItems = new();
        private GameBookSection _topSection;
        private CancellationTokenSource _cancellationTokenSource;

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
            _sectionItems.Clear();
            _lessons.Clear();
            _view.Clear();

            _topSection = _view.TopSection;

            foreach (var section in _gameBus.SimpleBookData.Sections)
            {
                var sectionItem = _view.InstantiateSection();
                sectionItem.Init(section.Section); // todo chang button?
                sectionItem.name = $"Section {section.Section}";
                _sectionItems.Add(sectionItem);

                for (int i = 0; i < section.Lessons.Count; i++)
                {
                    var key = $"{section.Section}_{i + 1}";
                    _lessons[key] = section.Lessons[i];

                    var lessonItem = _view.InstantiateLesson();
                    lessonItem.Init(key, (i + 1).ToString(), 0, OnItemClick);
                    lessonItem.name = $"Item {key}";
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

            if (active)
            {
                _view.TopSection.StartCoroutine(CalculateTopSectionRoutine());
            }
            else
            {
                // todo chang need to unsubscribe from top section
            }
        }

        private IEnumerator CalculateTopSectionRoutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                int left = 0;
                int right = _sectionItems.Count - 1;

                if (_sectionItems.Count == 0)
                {
                    continue;
                }

                GameBookSection lastSection = _sectionItems[0];
                float point = _topSection.transform.position.y - _topSection.GetComponent<RectTransform>().rect.height;

                while (left <= right)
                {
                    int mid = (left + right) / 2;
                    if (_sectionItems[mid].transform.position.y >= point)
                    {
                        lastSection = _sectionItems[mid];
                        left = mid + 1;
                    }
                    else
                    {
                        right = mid - 1;
                    }
                }

                _topSection.Init(lastSection.LabelText);
                // todo chang need to copy button subscribtion too
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
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
        private Coroutine _topSectionRoutine;

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
                var sectionBlock = _view.InstantiateSectionBlock(out var sectionItem);
                sectionItem.Init(section.Section, OnSectionRepetitionClick);
                sectionItem.name = $"Section {section.Section}";
                _sectionItems.Add(sectionItem);

                RectTransform row = null;
                int count = -1;
                for (int i = 0; i < section.Lessons.Count; i++)
                {
                    if (i / 6 > count)
                    {
                        count++;
                        row = _view.InstantiateRow(sectionBlock);
                    }

                    var key = $"{section.Section}_{i + 1}";
                    _lessons[key] = section.Lessons[i];

                    var lessonItem = i % 2 == 0
                        ? _view.InstantiateUpLesson(row)
                        : _view.InstantiateDownLesson(row);

                    lessonItem.Init(key, (i + 1).ToString(), 0, OnLessonClick);
                    lessonItem.name = $"Item {key}";
                }
            }
        }

        private void OnSectionRepetitionClick(string key)
        {
            Debug.Log($"OnSectionRepetitionClick key: {key}");
            _mainScreenBus.OnGameBookSectionRepeatClicked?.Invoke(key);
        }

        private void OnLessonClick(string key)
        {
            Debug.Log($"Clicked on item {key}");
            _mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[key].FileName);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);

            if (active)
            {
                if (_topSectionRoutine != null)
                {
                    _view.TopSection.StopCoroutine(_topSectionRoutine);
                }

                _topSectionRoutine = _view.TopSection.StartCoroutine(TopSectionRoutine());
            }
            else
            {
                if (_topSectionRoutine != null)
                {
                    _view.TopSection.StopCoroutine(_topSectionRoutine);
                    _topSectionRoutine = null;
                }
            }
        }

        private IEnumerator TopSectionRoutine()
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

                _topSection.Init(lastSection.LabelText, OnSectionRepetitionClick);
            }
        }
    }
}
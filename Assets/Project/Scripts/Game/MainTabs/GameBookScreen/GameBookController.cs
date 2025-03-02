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

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
        
        public void Set()
        {
            _sectionItems.Clear();
            _lessons.Clear();
            _view.Clear();

            for (var i = 0; i < _gameBus.SimpleBookData.Sections.Count; i++)
            {
                var baseColor = _view.GetNextColor(i);
                var section = _gameBus.SimpleBookData.Sections[i];
                
                var sectionBlock = _view.InstantiateSectionBlock(out var sectionItem);
                sectionBlock.SetBaseColor(baseColor);
                sectionItem.name = $"SectionBlock_{section.Section}";
                
                sectionItem.Init(section.Section, OnSectionRepetitionClick);
                sectionItem.name = $"Section_{section.Section}";
                sectionItem.SetBaseColor(baseColor);
                _sectionItems.Add(sectionItem);

                RectTransform row = null;
                int count = -1;
                for (int m = 0; m < section.Lessons.Count; m++)
                {
                    if (m / 6 > count)
                    {
                        count++;
                        row = _view.InstantiateRow(sectionBlock.Container);
                    }

                    var key = $"{section.Section}_{m + 1}";
                    _lessons[key] = section.Lessons[m];

                    var lessonItem = m % 2 == 0
                        ? _view.InstantiateUpLesson(row)
                        : _view.InstantiateDownLesson(row);

                    lessonItem.Init(key, (m + 1).ToString(), 0, OnLessonClick);
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
    }
}
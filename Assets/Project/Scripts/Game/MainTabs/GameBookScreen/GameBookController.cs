using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Services;
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
        private readonly ProfileService _profileService;

        private Dictionary<string, SimpleLessonData> _lessons = new();
        private List<GameBookSection> _sectionItems = new();
        private CancellationTokenSource _cancellationTokenSource;

        [Inject]
        public GameBookController(GameBus gameBus, MainScreenBus mainScreenBus, GameBookView view, ProfileService profileService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
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
                Color baseColor = _view.GetNextColor(i);
                SimpleSection section = _gameBus.SimpleBookData.Sections[i];

                SectionBlock sectionBlock = _view.InstantiateSectionBlock(out var sectionItem);
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
                    lessonItem.SetColor(GetLessonColor(section.Lessons[m].Questions));
                }
            }
        }

        private Color GetLessonColor(List<ISimpleQuestion> questions)
        {
            int sum = 0;
            int total = questions.Count * 10;
            foreach (ISimpleQuestion question in questions)
            {
                if (question is SimpleQuestSelectWord selectWord)
                {
                    sum += _profileService.GetMark(selectWord.CorrectWordFileName);
                }
                else
                {
                    throw new NotImplementedException($"Question type {question.QuestionType} is not implemented");
                }
            }

            return _view.GetLessonColor((float)sum / total);
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
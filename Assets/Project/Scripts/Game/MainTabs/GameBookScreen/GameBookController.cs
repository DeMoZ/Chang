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
        private Dictionary<string, SectionBlock> _sectionBlocks = new();
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
            _sectionBlocks.Clear();
            _lessons.Clear();
            _view.Clear();

            for (var i = 0; i < _gameBus.SimpleBookData.Sections.Count; i++)
            {
                Color baseColor = _view.GetNextColor(i);
                SimpleSection section = _gameBus.SimpleBookData.Sections[i];

                SectionBlock sectionBlock = _view.InstantiateSectionBlock(out var sectionItem);
                sectionBlock.SetBaseColor(baseColor);
                sectionItem.name = $"SectionBlock_{section.Section}";
                _sectionBlocks.Add(section.Section, sectionBlock);

                sectionItem.Init(section.Section, OnSectionSortClick, OnSectionRepetitionClick);
                sectionItem.name = $"Section_{section.Section}";
                sectionItem.SetBaseColor(baseColor);

                PopulateSection(section, sectionBlock);
            }
        }

        private Color GetLessonColor(List<ISimpleQuestion> questions)
        {
            float sum = 0;

            foreach (ISimpleQuestion question in questions)
            {
                if (question is SimpleQuestSelectWord selectWord)
                {
                    sum += (float)_profileService.GetMark(selectWord.CorrectWordFileName) / ProjectConstants.MARK_MAX;
                }
                else
                {
                    throw new NotImplementedException($"Question type {question.QuestionType} is not implemented");
                }
            }

            return _view.GetLessonColor(sum);
        }

        private void OnSectionSortClick(string key)
        {
            Debug.Log($"OnSectionSortClick key: {key}");
            SimpleSection section = _gameBus.SimpleBookData.Sections.Find(s => s.Section == key);

            if (_profileService.ReorderedSections.TryGetValue(_profileService.ReorderedSectionKey(section.Section), out _))
            {
                _profileService.ReorderedSections.Remove(_profileService.ReorderedSectionKey(section.Section));
            }
            else
            {
                _profileService.ReorderSection(_gameBus.CurrentLanguage, section);
            }

            SectionBlock sectionBlock = _sectionBlocks[key];

            foreach (Transform child in sectionBlock.Container)
            {
                if (!child.name.Contains("Section"))
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }

            PopulateSection(section, sectionBlock);
        }

        private void PopulateSection(SimpleSection section, SectionBlock sectionBlock)
        {
            if (_profileService.ReorderedSections.TryGetValue(_profileService.ReorderedSectionKey(section.Section), out var reorderedSection))
            {
                section = reorderedSection;
            }

            RectTransform row = null;
            int count = -1;
            for (int m = 0; m < section.Lessons.Count; m++)
            {
                if (m / 6 > count)
                {
                    count++;
                    row = _view.InstantiateRow(sectionBlock.Container);
                }

                string sectionName = section.Section;
                int lessonIndex = m + 1;
                string key = $"{section.Section}_{m + 1}";
                _lessons[key] = section.Lessons[m];

                GameBookItem lessonItem = m % 2 == 0
                    ? _view.InstantiateUpLesson(row)
                    : _view.InstantiateDownLesson(row);

                lessonItem.Init((m + 1).ToString(), 0, () => OnLessonClick(sectionName, lessonIndex));
                lessonItem.name = $"Item {key}";
                lessonItem.SetColor(GetLessonColor(section.Lessons[m].Questions));
            }
        }

        private void OnSectionRepetitionClick(string key)
        {
            Debug.Log($"OnSectionRepetitionClick key: {key}");
            _mainScreenBus.OnGameBookSectionRepeatClicked?.Invoke(key);
        }

        private void OnLessonClick(string sectionName, int lessonIndex)
        {
            Debug.Log($"Clicked on item {sectionName}_{lessonIndex}");
            _mainScreenBus.OnGameBookLessonClicked?.Invoke(sectionName, lessonIndex);
        }
    }
}
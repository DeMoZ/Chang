using System;
using System.Collections.Generic;
using Chang.Profile;
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
        private readonly RepetitionService _repetitionService;

        private Dictionary<string, SimpleLessonData> _lessons = new();
        private Dictionary<string, SectionBlock> _sectionBlocks = new();

        [Inject]
        public GameBookController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            GameBookView view,
            ProfileService profileService,
            RepetitionService repetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _repetitionService = repetitionService;
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
                SimpleSection simpleSection = _gameBus.SimpleBookData.Sections[i];

                SectionBlock sectionBlock = _view.InstantiateSectionBlock();
                sectionBlock.SetBaseColor(baseColor);
                sectionBlock.SectionView.name = $"SectionBlock_{simpleSection.Section}";
                _sectionBlocks.Add(simpleSection.Section, sectionBlock);

                sectionBlock.SectionView.Init(simpleSection.Section,
                    () => OnSectionSortClick(simpleSection.Section),
                    () => OnSectionRepetitionClick(simpleSection.Section));

                sectionBlock.SectionView.name = $"Section_{simpleSection.Section}";
                sectionBlock.SectionView.SetBaseColor(baseColor);

                PopulateSection(simpleSection, sectionBlock);
            }
        }

        private Color GetLessonColor(SimpleLessonData lessonData)
        {
            float sum = 0;

            foreach (ISimpleQuestion question in lessonData.Questions)
            {
                if (question is SimpleQuestSelectWord selectWord)
                {
                    sum += (float)_profileService.GetMark(selectWord.CorrectWordFileName) / (ProjectConstants.MARK_MAX * lessonData.Questions.Count);
                }
                else
                {
                    throw new NotImplementedException($"Question type {question.QuestionType} is not implemented");
                }
            }

            // Debug.Log($"GetLessonColor for {lessonData.Section}, {lessonData.Name} sum: {sum}");
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
                _profileService.ReorderSection(section);
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
            List<QuestLog> repetitions = _repetitionService.GetSectionRepetition(ProjectConstants.SECTION_REPETITION_AMOUNT, section.Section);

            int repetitionsCount = repetitions.Count;
            string reorderedSectionKey = _profileService.ReorderedSectionKey(section.Section);

            sectionBlock.SectionView.SetSortToggle(
                repetitionsCount > 0 && _profileService.ReorderedSections.ContainsKey(reorderedSectionKey),
                repetitionsCount > 0);

            sectionBlock.SectionView.SetInteractableRepeatButton(repetitionsCount >= ProjectConstants.SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT);

            if (_profileService.ReorderedSections.TryGetValue(reorderedSectionKey, out var reorderedSection))
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
                var color = GetLessonColor(section.Lessons[m]);
                lessonItem.SetColor(color);
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
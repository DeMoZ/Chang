using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Profile;
using Chang.Services;
using Chang.GameBook;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Vocabulary
{
    public class VocabularyController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly BookVocabularyView _view;
        private readonly ProfileService _profileService;
        private readonly VocabularyRepetitionService _repetitionService;

        private Dictionary<string, LessonData> _lessons = new();
        private Dictionary<string, SectionBlock> _sectionBlocks = new();
        private CancellationTokenSource _cts;

        [Inject]
        public VocabularyController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            BookVocabularyView view,
            ProfileService profileService,
            VocabularyRepetitionService repetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _repetitionService = repetitionService;

            _cts = new CancellationTokenSource();
        }

        public void Init()
        {
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public async UniTask SetAsync(CancellationToken ct)
        {
            _sectionBlocks.Clear();
            _lessons.Clear();
            _view.Clear();

            for (var i = 0; i < _gameBus.VocabularyBookData.Sections.Count; i++)
            {
                Color baseColor = _view.GetNextColor(i);
                SectionData sectionData = _gameBus.VocabularyBookData.Sections[i];

                SectionBlock sectionBlock = _view.InstantiateSectionBlock();
                sectionBlock.SetBaseColor(baseColor);
                sectionBlock.SectionView.name = $"SectionBlock_{sectionData.Section}";
                _sectionBlocks.Add(sectionData.Section, sectionBlock);

                sectionBlock.SectionView.Init(sectionData.Section,
                    () => OnSectionSortClick(sectionData.Section),
                    () => OnSectionRepetitionClick(sectionData.Section));

                sectionBlock.SectionView.name = $"Section_{sectionData.Section}";
                sectionBlock.SectionView.SetBaseColor(baseColor);

                await PopulateSectionAsync(sectionData, sectionBlock, ct);
            }

            await UniTask.Yield();

            SetScrollPosition();
        }

        private Color GetLessonColor(LessonData lessonData)
        {
            float sum = 0;

            foreach (IQuestion question in lessonData.Questions)
            {
                if (question is Vocabulary.QuestSelectWord selectWord)
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
            SectionData sectionData = _gameBus.VocabularyBookData.Sections.Find(s => s.Section == key);

            if (_profileService.ReorderedSections.TryGetValue(_profileService.ReorderedSectionKey(sectionData.Section), out _))
            {
                _profileService.ReorderedSections.Remove(_profileService.ReorderedSectionKey(sectionData.Section));
            }
            else
            {
                _profileService.ReorderSection(sectionData);
            }

            SectionBlock sectionBlock = _sectionBlocks[key];

            foreach (Transform child in sectionBlock.Container)
            {
                if (!child.name.Contains("Section"))
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }

            PopulateSectionAsync(sectionData, sectionBlock, _cts.Token).Forget();
        }

        private async UniTask PopulateSectionAsync(SectionData sectionData, SectionBlock sectionBlock, CancellationToken ct)
        {
            List<QuestLog> repetitions = await _repetitionService
                .GetSectionRepetitionAsync(ProjectConstants.SECTION_REPETITION_AMOUNT, sectionData.Section, ct);

            int repetitionsCount = repetitions.Count;
            string reorderedSectionKey = _profileService.ReorderedSectionKey(sectionData.Section);

            sectionBlock.SectionView.SetSortToggle(
                repetitionsCount > 0 && _profileService.ReorderedSections.ContainsKey(reorderedSectionKey),
                repetitionsCount > 0);

            sectionBlock.SectionView.SetInteractableRepeatButton(repetitionsCount >= ProjectConstants.SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT);

            if (_profileService.ReorderedSections.TryGetValue(reorderedSectionKey, out var reorderedSection))
            {
                sectionData = reorderedSection;
            }

            RectTransform row = null;
            int count = -1;
            for (int m = 0; m < sectionData.Lessons.Count; m++)
            {
                if (m / 6 > count)
                {
                    count++;
                    row = _view.InstantiateRow(sectionBlock.Container);
                }

                string sectionName = sectionData.Section;
                int lessonIndex = m + 1;
                string key = $"{sectionData.Section}_{m + 1}";
                _lessons[key] = sectionData.Lessons[m];

                GameBookItem lessonItem = m % 2 == 0
                    ? _view.InstantiateUpLesson(row)
                    : _view.InstantiateDownLesson(row);

                lessonItem.Init((m + 1).ToString(), 0, () => OnLessonClick(sectionName, lessonIndex));
                lessonItem.name = $"Item {key}";
                var color = GetLessonColor(sectionData.Lessons[m]);
                lessonItem.SetColor(color);
            }
        }

        private void OnSectionRepetitionClick(string key)
        {
            Debug.Log($"OnSectionRepetitionClick key: {key}");
            SaveScrollPosition();
            _mainScreenBus.OnWordsSectionRepeatClicked?.Invoke(key);
        }

        private void OnLessonClick(string sectionName, int lessonIndex)
        {
            Debug.Log($"Clicked on item {sectionName}_{lessonIndex}");
            SaveScrollPosition();
            _mainScreenBus.OnWordsLessonClicked?.Invoke(sectionName, lessonIndex);
        }

        private void SaveScrollPosition()
        {
            _profileService.ProgressData.GameBookScrollPosition = _view.ScrollPosition;
            Debug.Log($"Load gamebook scroll position: {_profileService.ProgressData.GameBookScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private void SetScrollPosition()
        {
            _view.ScrollPosition = _profileService.ProgressData.GameBookScrollPosition;
            Debug.Log($"Load gamebook scroll position: {_profileService.ProgressData.GameBookScrollPosition}, scroll position: {_view.ScrollPosition}");
        }
    }
}
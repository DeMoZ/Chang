using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Profile;
using Chang.Services;
using Chang.GameBook;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Vocabulary
{
    public class VocabularyController : IViewController, IBookController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly BookVocabularyView _view;
        private readonly ProfileService _profileService;
        private readonly RepetitionService _repetitionService;
        private readonly RepetitionLessonBuilder _repetitionLessonBuilder;
        private readonly SectionSortService _sectionSortService;

        private Dictionary<string, Lesson> _lessons = new();
        private Dictionary<string, SectionBlock> _sectionBlocks = new();
        private CancellationTokenSource _cts;
        private Action _onLobbyExitState;

        [Inject]
        public VocabularyController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            BookVocabularyView view,
            ProfileService profileService,
            RepetitionService repetitionService,
            RepetitionLessonBuilder repetitionLessonBuilder,
            SectionSortService sectionSortService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _repetitionService = repetitionService;
            _repetitionLessonBuilder = repetitionLessonBuilder;
            _sectionSortService = sectionSortService;

            _cts = new CancellationTokenSource();
        }

        public void Init(Action onLobbyExitState)
        {
            _onLobbyExitState = onLobbyExitState;
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


            for (int i = 0; i < _gameBus.VocabularyBook.Sections.Count; i++)
            {
                Color baseColor = _view.GetNextColor(i);
                VocabularySection sectionData = _gameBus.VocabularyBook.Sections[i];

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

            await UniTask.Yield(ct);

            SetScrollPosition();
        }

        public void OnGeneralRepeatClicked()
        {
            OnGeneralRepeatClickedAsync(_cts.Token).Forget();
        }

        /// <summary>Sum of the question marks ÷ (questions × max mark), 0…1.</summary>
        private float GetLessonProgress(Lesson lessonData)
        {
            float sum = 0;

            foreach (IQuestion question in lessonData.Questions)
            {
                if (question is Chang.Core.QuestSelectWord selectWord)
                {
                    sum += (float)_profileService.GetVocabularyMark(selectWord.Key) /
                           (ProjectConstants.MARK_MAX * lessonData.Questions.Count);
                }
                else
                {
                    throw new NotImplementedException($"Question type {question.Type} is not implemented");
                }
            }

            // Debug.Log($"GetLessonProgress for {lessonData.Section}, {lessonData.Name} sum: {sum}");
            return sum;
        }

        private void OnSectionSortClick(string key)
        {
            Debug.Log($"OnSectionSortClick key: {key}");
            VocabularySection section = _gameBus.VocabularyBook.Sections.Find(s => s.Section == key);
            _sectionSortService.ToggleSort(section);

            SectionBlock sectionBlock = _sectionBlocks[key];

            foreach (Transform child in sectionBlock.Container)
            {
                if (!child.name.Contains("Section"))
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }

            PopulateSectionAsync(section, sectionBlock, _cts.Token).Forget();
        }

        private async UniTask PopulateSectionAsync(
            VocabularySection sectionData,
            SectionBlock sectionBlock,
            CancellationToken ct)
        {
            await UniTask.Yield(ct);

            string reorderedSectionKey = _profileService.ReorderedSectionKey(sectionData.Section);

            bool canSort = _sectionSortService.CanSort(sectionData);
            sectionBlock.SectionView.SetSortToggle(canSort && _sectionSortService.IsSorted(sectionData), canSort);

            sectionBlock.SectionView.SetInteractableRepeatButton(_repetitionService.CanRepeatVocabulary(sectionData));

            if (_profileService.ReorderedVocabularySections.TryGetValue(reorderedSectionKey,
                    out VocabularySection reorderedSection))
            {
                sectionData = reorderedSection;
            }

            RectTransform row = null;
            int count = -1;
            float progressSum = 0;
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
                float progress = GetLessonProgress(sectionData.Lessons[m]);
                lessonItem.SetColor(_view.GetLessonColor(progress));
                lessonItem.SetProgress(progress);
                progressSum += progress;
            }

            int lessonsCount = sectionData.Lessons.Count;
            sectionBlock.SectionView.SetProgress(lessonsCount > 0 ? progressSum / lessonsCount : 0, lessonsCount);
        }

        private void OnSectionRepetitionClick(string key)
        {
            Debug.Log($"OnSectionRepetitionClick key: {key}");
            SaveScrollPosition();
            OnSectionRepeatClickedAsync(key, _cts.Token).Forget();
        }

        private void OnLessonClick(string sectionName, int lessonIndex)
        {
            Debug.Log($"Clicked on item {sectionName}_{lessonIndex}");
            SaveScrollPosition();
            OnLessonClicked(sectionName, lessonIndex);
        }

        private void SaveScrollPosition()
        {
            _profileService.VocabularyProgress.ScrollPosition = _view.ScrollPosition;
            Debug.Log(
                $"Load gamebook scroll position: {_profileService.VocabularyProgress.ScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private void SetScrollPosition()
        {
            _view.ScrollPosition = _profileService.VocabularyProgress.ScrollPosition;
            Debug.Log(
                $"Load gamebook scroll position: {_profileService.VocabularyProgress.ScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private void OnLessonClicked(string sectionName, int lessonIndex)
        {
            if (_mainScreenBus.IsLoading)
            {
                return;
            }

            _mainScreenBus.IsLoading = true;
            try
            {
                Lesson lesson;
                string key = _profileService.ReorderedSectionKey(sectionName);

                if (_profileService.ReorderedVocabularySections.TryGetValue(key, out VocabularySection section))
                {
                    lesson = section.Lessons[lessonIndex - 1];
                }
                else
                {
                    string sectionKey = ElementsPaths.VocabularySectionKey(_profileService.ProfileData.LearnLanguage, sectionName);

                    if (!_gameBus.VocabularySections.TryGetValue(sectionKey, out section))
                    {
                        throw new IndexOutOfRangeException($"Section not found with key: {sectionKey}");
                    }

                    lesson = section.Lessons[lessonIndex - 1];
                }

                _gameBus.SetLesson(lesson);
                _gameBus.GameType = GameType.Learn;
            }
            finally
            {
                _mainScreenBus.IsLoading = false;
            }

            _onLobbyExitState?.Invoke();
        }

        private async UniTaskVoid OnSectionRepeatClickedAsync(string section, CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;

            VocabularySection sectionData = _gameBus.VocabularyBook.Sections.Find(s => s.Section == section);
            List<RepetitionCandidate> repetitions = _repetitionService.GetVocabularyRepetition(sectionData);
            await MakeRepetitionAsync(repetitions, ct);
        }

        private async UniTaskVoid OnGeneralRepeatClickedAsync(CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;

            List<RepetitionCandidate> repetitions = _repetitionService.GetVocabularyRepetition();
            await MakeRepetitionAsync(repetitions, ct);
        }

        private async UniTask MakeRepetitionAsync(List<RepetitionCandidate> repetitions, CancellationToken ct)
        {
            if (repetitions.Count == 0)
            {
                Debug.LogWarning("No played keys for repetition");
                return;
            }

            _mainScreenBus.IsLoading = true;
            try
            {
                await UniTask.Yield(ct);

                _gameBus.SetLesson(_repetitionLessonBuilder.Build(repetitions));
                _gameBus.GameType = GameType.Repetition;
            }
            finally
            {
                _mainScreenBus.IsLoading = false;
            }

            _onLobbyExitState?.Invoke();
        }
    }
}

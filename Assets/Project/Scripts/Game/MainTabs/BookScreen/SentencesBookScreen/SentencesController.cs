using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Core;
using Chang.Services;
using Chang.GameBook;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Sentences
{
    public class SentencesController : IViewController, IBookController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly BookSentencesView _view;
        private readonly ProfileService _profileService;
        private readonly RepetitionService _repetitionService;
        private readonly RepetitionLessonBuilder _repetitionLessonBuilder;
        private readonly SectionSortService _sectionSortService;

        private Dictionary<string, Lesson> _lessons = new();
        private Dictionary<string, SectionBlock> _sectionBlocks = new();
        private CancellationTokenSource _cts;
        private Action _onLobbyExitState;

        [Inject]
        public SentencesController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            BookSentencesView view,
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

            for (int i = 0; i < _gameBus.SentencesBook.Sections.Count; i++)
            {
                Color baseColor = _view.GetNextColor(i);
                SentencesSection section = _gameBus.SentencesBook.Sections[i];
                SectionBlock sectionBlock = _view.InstantiateSectionBlock();
                sectionBlock.SetBaseColor(baseColor);
                sectionBlock.SectionView.name = $"SectionBlock_{section.Section}";
                _sectionBlocks.Add(section.Section, sectionBlock);

                sectionBlock.SectionView.Init(section.Section,
                    () => OnSectionSortClick(section.Section),
                    () => OnSectionRepetitionClick(section.Section));

                sectionBlock.SectionView.name = $"Section_{section.Section}";
                sectionBlock.SectionView.SetBaseColor(baseColor);

                await PopulateSectionAsync(section, sectionBlock, ct);
            }

            await UniTask.Yield(ct);

            SetScrollPosition();
        }

        public void OnGeneralRepeatClicked()
        {
            OnGeneralRepeatClickedAsync(_cts.Token).Forget();
        }

        private Color GetLessonColor(Lesson lesson)
        {
            float sum = 0;

            foreach (IQuestion question in lesson.Questions)
            {
                if (question is SentenceSelectWords selectWord)
                {
                    sum += _profileService.GetSentencesMark(selectWord.Key) /
                           (ProjectConstants.MARK_MAX * lesson.Questions.Count);
                }
                else
                {
                    throw new NotImplementedException($"Question type {question.Type} is not implemented");
                }
            }

            return _view.GetLessonColor(sum);
        }

        private void OnSectionSortClick(string key)
        {
            Debug.Log($"OnSectionSortClick key: {key}");
            SentencesSection section = _gameBus.SentencesBook.Sections.Find(s => s.Section == key);
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

        private async UniTask PopulateSectionAsync(SentencesSection section, SectionBlock sectionBlock,
            CancellationToken ct)
        {
            await UniTask.Yield(ct);

            string reorderedSectionKey = _profileService.ReorderedSectionKey(section.Section);

            bool canSort = _sectionSortService.CanSort(section);
            sectionBlock.SectionView.SetSortToggle(canSort && _sectionSortService.IsSorted(section), canSort);

            sectionBlock.SectionView.SetInteractableRepeatButton(_repetitionService.CanRepeatSentences(section));

            if (_profileService.ReorderedSentencesSections.TryGetValue(reorderedSectionKey,
                    out SentencesSection reorderedSection))
            {
                section = reorderedSection;
            }

            RectTransform row = null;
            int count = -1;
            for (int m = 0; m < section.SectionLessons.Count; m++)
            {
                if (m / 6 > count)
                {
                    count++;
                    row = _view.InstantiateRow(sectionBlock.Container);
                }

                string sectionName = section.SectionKey;
                int lessonIndex = m + 1;
                string key = $"{section.Section}_{m + 1}";
                _lessons[key] = section.SectionLessons[m];

                GameBookItem lessonItem = m % 2 == 0
                    ? _view.InstantiateUpLesson(row)
                    : _view.InstantiateDownLesson(row);

                lessonItem.Init((m + 1).ToString(), 0, () => OnLessonClick(sectionName, lessonIndex));
                lessonItem.name = $"Item {key}";
                Color color = GetLessonColor(section.SectionLessons[m]);
                lessonItem.SetColor(color);
            }
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

        private void OnLessonClicked(string sectionKey, int lessonIndex)
        {
            if (_mainScreenBus.IsLoading)
            {
                return;
            }

            _mainScreenBus.IsLoading = true;
            try
            {
                SentencesSection section = _gameBus.SentencesSections[sectionKey];
                string reorderedSectionKey = _profileService.ReorderedSectionKey(section.Section);

                if (_profileService.ReorderedSentencesSections.TryGetValue(reorderedSectionKey, out SentencesSection reorderedSection))
                {
                    section = reorderedSection;
                }

                Lesson lesson = section.SectionLessons[lessonIndex - 1];

                lesson.SetQuestions(lesson.Questions);
                InitQuestions(lesson);
                _gameBus.SetLesson(lesson);
                _gameBus.GameType = GameType.Learn;
            }
            finally
            {
                _mainScreenBus.IsLoading = false;
            }

            _onLobbyExitState?.Invoke();
        }

        private void InitQuestions(Lesson lesson)
        {
            foreach (IQuestion question in lesson.Questions)
            {
                var quest = question as SentenceSelectWords;
                if (quest == null)
                {
                    Debug.LogWarning($"Question is not of type SentenceSelectWords.");
                    throw new InvalidOperationException($"Question is not of type SentenceSelectWords.");
                }

                if (!_gameBus.Sentences.TryGetValue(quest.Key, out Sentence sentence))
                {
                    Debug.LogWarning($"Sentence with key {quest.Key} not found in Sentences.");
                    throw new KeyNotFoundException($"Sentence with key {quest.Key} not found in Sentences.");
                }

                quest.Sentence = sentence;
            }
        }

        private async UniTaskVoid OnSectionRepeatClickedAsync(string section, CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
            {
                return;
            }

            SentencesSection sectionData = _gameBus.SentencesBook.Sections.Find(s => s.Section == section);
            List<RepetitionCandidate> repetitions = _repetitionService.GetSentencesRepetition(sectionData);
            await MakeRepetitionAsync(repetitions, ct);
        }

        private async UniTaskVoid OnGeneralRepeatClickedAsync(CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
            {
                return;
            }

            List<RepetitionCandidate> repetitions = _repetitionService.GetSentencesRepetition();
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

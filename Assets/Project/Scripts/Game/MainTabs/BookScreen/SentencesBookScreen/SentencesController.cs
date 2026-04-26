using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SentencesRepetitionService _repetitionService;

        private Dictionary<string, Deprecated.LessonData> _lessons = new();
        private Dictionary<string, SectionBlock> _sectionBlocks = new();
        private CancellationTokenSource _cts;
        private Action _onLobbyExitState;

        [Inject]
        public SentencesController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            BookSentencesView view,
            ProfileService profileService,
            SentencesRepetitionService sentencesRepetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _repetitionService = sentencesRepetitionService;

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

            for (int i = 0; i < _gameBus.SentencesBookData.Sections.Count; i++)
            {
                Color baseColor = _view.GetNextColor(i);
                SectionData sectionData = _gameBus.SentencesBookData.Sections[i];

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

        public void OnGeneralRepeatClicked()
        {
            throw new NotImplementedException();
        }

        private Color GetLessonColor(LessonData lessonData)
        {
            float sum = 0;

            foreach (IQuestion question in lessonData.Questions)
            {
                if (question is SentenceSelectWords selectWord)
                {
                    sum += _profileService.GetSentencesMark(selectWord.LogKey) / (ProjectConstants.MARK_MAX * lessonData.Questions.Count);
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
            SectionData sectionData = _gameBus.SentencesBookData.Sections.Find(s => s.Section == key);

            if (_profileService.ReorderedVocabularySections.TryGetValue(_profileService.ReorderedSectionKey(sectionData.Section), out _))
            {
                _profileService.ReorderedVocabularySections.Remove(_profileService.ReorderedSectionKey(sectionData.Section));
            }
            else
            {
                _profileService.ReorderSentencesSection(sectionData);
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
            List<SentenceQuestLog> repetitions = await _repetitionService
                .GetSectionRepetitionAsync(ProjectConstants.SECTION_REPETITION_AMOUNT, sectionData.Section, ct);

            int repetitionsCount = repetitions.Count;
            string reorderedSectionKey = _profileService.ReorderedSectionKey(sectionData.Section);

            sectionBlock.SectionView.SetSortToggle(
                repetitionsCount > 0 && _profileService.ReorderedSentencesSections.ContainsKey(reorderedSectionKey),
                repetitionsCount > 0);

            sectionBlock.SectionView.SetInteractableRepeatButton(repetitionsCount >= ProjectConstants.SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT);

            if (_profileService.ReorderedSentencesSections.TryGetValue(reorderedSectionKey, out SectionData reorderedSection))
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
                Color color = GetLessonColor(sectionData.Lessons[m]);
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
            OnLessonClickedAsync(sectionName, lessonIndex, _cts.Token).Forget();
        }

        private void SaveScrollPosition()
        {
            _profileService.VocabularyProgress.ScrollPosition = _view.ScrollPosition;
            Debug.Log($"Load gamebook scroll position: {_profileService.VocabularyProgress.ScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private void SetScrollPosition()
        {
            _view.ScrollPosition = _profileService.VocabularyProgress.ScrollPosition;
            Debug.Log($"Load gamebook scroll position: {_profileService.VocabularyProgress.ScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private async UniTaskVoid OnLessonClickedAsync(string sectionName, int lessonIndex, CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;

            _mainScreenBus.IsLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            {
                LessonData simpleLesson;
                string key = _profileService.ReorderedSectionKey(sectionName);

                if (_profileService.ReorderedSentencesSections.TryGetValue(key, out SectionData section))
                {
                    simpleLesson = section.Lessons[lessonIndex - 1];
                }
                else
                {
                    key = ProjectSharedLogic.SENTENCE_LESSON_KEY(_profileService.LearnLanguage.ToString(), sectionName, lessonIndex);
                    simpleLesson = _gameBus.SentencesLessons[key];
                }

                Lesson lesson = new Lesson();
                lesson.FileName = simpleLesson.FileName;
                lesson.SetSimpleQuestions(simpleLesson.Questions.ToList());

                // _gameBus.CurrentSentencesLesson = lesson;
                _gameBus.LessonProvider = lesson;
            }

            _mainScreenBus.IsLoading = false;

            _gameBus.GameType = GameType.Learn;
            _onLobbyExitState?.Invoke();
        }

        private async UniTaskVoid OnSectionRepeatClickedAsync(string section, CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;

            throw new NotImplementedException();
            // todo chang show loading animation ?
            // var repetitions = await _repetitionService.GetSectionRepetitionAsync(ProjectConstants.SECTION_REPETITION_AMOUNT, section, ct);
            // MakeRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        private async UniTaskVoid OnGeneralRepeatClickedAsync(CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;
            throw new NotImplementedException();
            // todo chang show loading animation ?
            // var repetitions = await _repetitionService.GetGeneralRepetitionAsync(ProjectConstants.GENERAL_REPETITION_AMOUNT, ct);
            // MakeRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        private async UniTaskVoid MakeRepetitionAsync(List<VocabularyQuestLog> repetitions, CancellationToken ct)
        {
            throw new NotImplementedException();
            if (repetitions.Count < ProjectConstants.SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT)
            {
                Debug.LogWarning("Not enough logs for general repetition");
                return;
            }

            _mainScreenBus.IsLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            List<IQuestion> questions = new List<IQuestion>();

            foreach (VocabularyQuestLog questLog in repetitions)
            {
                // switch (questLog.QuestionType)
                // {
                //     case QuestionType.SelectWord:
                //         var simpleQuest = new QuestSelectWord();
                //         simpleQuest.CorrectWordFileName = questLog.FileName;
                //         var words = repetitions
                //             .Where(r => r.QuestionType == QuestionType.SelectWord && r.FileName != simpleQuest.CorrectWordFileName)
                //             .ToList();
                //
                //         words.Shuffle();
                //
                //         simpleQuest.MixWordsFileNames = words.Take(ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE)
                //             .Select(w => w.FileName)
                //             .ToList();
                //
                //         questions.Add(simpleQuest);
                //         break;
                //
                //     default:
                //         throw new NotImplementedException($"Not implemented simple quest generation for type: {questLog.QuestionType}");
                // }
            }

            // var lesson = new Lesson();
            // lesson.GenerateQuestMatchWordsData = true;
            // lesson.SetSimpleQuestions(questions);
            //
            // _gameBus.CurrentVocabularyLesson = lesson;
            // _mainScreenBus.IsLoading = false;
            //
            // _gameBus.GameType = GameType.Repetition;
            _onLobbyExitState?.Invoke();
        }
    }
}
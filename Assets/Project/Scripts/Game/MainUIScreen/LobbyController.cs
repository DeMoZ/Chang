using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using Chang.Services;
using Chang.GameBook;
using Chang.Profile;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class LobbyController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly MainUiView _view;
        private readonly Vocabulary.VocabularyController _vocabularyController;
        private readonly VocabularyRepetitionController _vocabularyRepetitionController;
        private readonly VocabularyRepetitionService _vocabularyRepetitionService;
        private readonly Sentences.SentencesController _sentencesController;
        private readonly SentencesRepetitionController _sentencesRepetitionController;
        private readonly SentencesRepetitionService _sentencesRepetitionService;
        private readonly ProfileController _profileController;
        private readonly ProfileService _profileService;

        private bool _isLoading;
        private CancellationTokenSource _cts;

        /// <summary>
        /// should return to this tab after play any other game state
        /// </summary>
        private MainTabType _currentTabType = MainTabType.Vocabulary;

        private Action _onExitState;

        [Inject]
        public LobbyController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            MainUiView view,
            Vocabulary.VocabularyController vocabularyController,
            VocabularyRepetitionController vocabularyRepetitionController,
            VocabularyRepetitionService vocabularyRepetitionService,
            Sentences.SentencesController sentencesController,
            SentencesRepetitionController sentencesRepetitionController,
            SentencesRepetitionService sentencesRepetitionService,
            ProfileController profileController,
            ProfileService profileService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _vocabularyController = vocabularyController;
            _vocabularyRepetitionController = vocabularyRepetitionController;
            _vocabularyRepetitionService = vocabularyRepetitionService;
            _sentencesController = sentencesController;
            _sentencesRepetitionController = sentencesRepetitionController;
            _sentencesRepetitionService = sentencesRepetitionService;
            _profileController = profileController;
            _profileService = profileService;

            _mainScreenBus.OnWordsLessonClicked += OnVocabularyLessonClicked;
            _mainScreenBus.OnWordsSectionRepeatClicked += OnVocabularySectionRepeatClicked;
            _mainScreenBus.OnWordsRepeatClicked += OnGeneralVocabularyRepeatClicked;
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _mainScreenBus.OnWordsLessonClicked -= OnVocabularyLessonClicked;
            _mainScreenBus.OnWordsSectionRepeatClicked -= OnVocabularySectionRepeatClicked;
            _mainScreenBus.OnWordsRepeatClicked -= OnGeneralVocabularyRepeatClicked;
            _cts.Cancel();
            _cts.Dispose();
        }

        public void Init(Action onExitState)
        {
            _onExitState = onExitState;

            _view.Init(OnToggleSelected);
            _vocabularyController.Init();
            _vocabularyRepetitionController.Init();
            _sentencesController.Init();
            _sentencesRepetitionController.Init();
            _profileController.Init();
        }

        public void Enter()
        {
            SetViewActive(true);
            _view.Enter();

            OnToggleSelected(true, _currentTabType);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        private void OnToggleSelected(bool isOn, MainTabType tabType)
        {
            OnToggleSelectedAsync(isOn, tabType, _cts.Token).Forget();
        }

        private async UniTaskVoid OnToggleSelectedAsync(bool isOn, MainTabType tabType, CancellationToken ct)
        {
            if (_isLoading || !isOn)
                return;

            _vocabularyController.SetViewActive(tabType == MainTabType.Vocabulary);
            _sentencesController.SetViewActive(tabType == MainTabType.Sentences);
            _vocabularyRepetitionController.SetViewActive(tabType == MainTabType.Repetition);
            _profileController.SetViewActive(tabType == MainTabType.Profile);
            _currentTabType = tabType;

            // todo chang show loading animation ?
            switch (tabType)
            {
                case MainTabType.Vocabulary:
                    await _vocabularyController.SetAsync(ct);
                    break;

                case MainTabType.Sentences:
                    await _sentencesController.SetAsync(ct);
                    break;

                case MainTabType.Repetition:
                    await _vocabularyRepetitionController.SetAsync(ct);
                    break;

                case MainTabType.Profile:
                    await _profileController.SetAsync(ct);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null);
            }
        }

        // todo chang move into vocabulary controller
        private void OnVocabularyLessonClicked(string sectionName, int lessonIndex)
        {
            OnVocabularyLessonClickedAsync(sectionName, lessonIndex, _cts.Token).Forget();
        }

        // todo chang move into vocabulary controller
        private async UniTaskVoid OnVocabularyLessonClickedAsync(string sectionName, int lessonIndex, CancellationToken ct)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            Vocabulary.LessonData simpleLesson;
            string key = _profileService.ReorderedSectionKey(sectionName);
            if (_profileService.ReorderedVocabularySections.TryGetValue(key, out Vocabulary.SectionData section))
            {
                simpleLesson = section.Lessons[lessonIndex - 1];
            }
            else
            {
                key = $"{_profileService.ProfileData.LearnLanguage}Lesson{sectionName}_{lessonIndex}";
                simpleLesson = _gameBus.VocabularyLessons[key];
            }

            Vocabulary.Lesson lesson = new Vocabulary.Lesson();
            lesson.FileName = simpleLesson.FileName;
            lesson.GenerateQuestMatchWordsData = simpleLesson.GenerateQuestMatchWordsData;
            lesson.SetSimpleQuestions(simpleLesson.Questions.ToList());

            _gameBus.CurrentVocabularyLesson = lesson;
            _isLoading = false;

            _gameBus.GameType = GameType.Learn;
            _onExitState?.Invoke();
        }

        // todo chang move into vocabulary controller
        private void OnVocabularySectionRepeatClicked(string section)
        {
            OnVocabularySectionRepeatClickedAsync(section, _cts.Token).Forget();
        }

        // todo chang move into vocabulary controller
        private async UniTaskVoid OnVocabularySectionRepeatClickedAsync(string section, CancellationToken ct)
        {
            if (_isLoading)
                return;

            // todo chang show loading animation ?
            var repetitions = await _vocabularyRepetitionService.GetSectionRepetitionAsync(ProjectConstants.SECTION_REPETITION_AMOUNT, section, ct);
            MakeVocabularyRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        // todo chang move into vocabulary controller
        private void OnGeneralVocabularyRepeatClicked()
        {
            OnGeneralVocabularyRepeatClickedAsync(_cts.Token).Forget();
        }

        // todo chang move into vocabulary controller
        private async UniTaskVoid OnGeneralVocabularyRepeatClickedAsync(CancellationToken ct)
        {
            if (_isLoading)
                return;

            // todo chang show loading animation ?
            var repetitions = await _vocabularyRepetitionService.GetGeneralRepetitionAsync(ProjectConstants.GENERAL_REPETITION_AMOUNT, ct);
            MakeVocabularyRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        // todo chang move into vocabulary controller
        private async UniTaskVoid MakeVocabularyRepetitionAsync(List<VocabularyQuestLog> repetitions, CancellationToken ct)
        {
            if (repetitions.Count < ProjectConstants.SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT)
            {
                Debug.LogWarning("Not enough logs for general repetition");
                return;
            }

            _isLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            var questions = new List<Vocabulary.IQuestion>();

            foreach (var questLog in repetitions)
            {
                switch (questLog.QuestionType)
                {
                    case QuestionType.SelectWord:
                        var simpleQuest = new Vocabulary.QuestSelectWord();
                        simpleQuest.CorrectWordFileName = questLog.FileName;
                        var words = repetitions
                            .Where(r => r.QuestionType == QuestionType.SelectWord && r.FileName != simpleQuest.CorrectWordFileName)
                            .ToList();

                        words.Shuffle();

                        simpleQuest.MixWordsFileNames = words.Take(ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE)
                            .Select(w => w.FileName)
                            .ToList();

                        questions.Add(simpleQuest);
                        break;

                    default:
                        throw new NotImplementedException($"Not implemented simple quest generation for type: {questLog.QuestionType}");
                }
            }

            var lesson = new Vocabulary.Lesson();
            lesson.GenerateQuestMatchWordsData = true;
            lesson.SetSimpleQuestions(questions);

            _gameBus.CurrentVocabularyLesson = lesson;
            _isLoading = false;

            _gameBus.GameType = GameType.Repetition;
            _onExitState?.Invoke();
        }
    }
}
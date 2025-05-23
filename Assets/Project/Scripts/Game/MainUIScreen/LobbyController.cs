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
        private const int GeneralRepetitionAmount = 10;

        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly MainUiView _view;
        private readonly GameBookController _gameBookController;
        private readonly RepetitionController _repetitionController;
        private readonly ProfileController _profileController;
        private readonly ProfileService _profileService;
        private readonly RepetitionService _repetitionService;

        private bool _isLoading;
        private CancellationTokenSource _cts;

        /// <summary>
        /// should return to this tab after play any other game state
        /// </summary>
        private MainTabType _currentTabType = MainTabType.Lessons;

        private Action _onExitState;

        [Inject]
        public LobbyController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            MainUiView view,
            GameBookController gameBookController,
            RepetitionController repetitionController,
            ProfileController profileController,
            ProfileService profileService,
            RepetitionService repetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _gameBookController = gameBookController;
            _repetitionController = repetitionController;
            _profileController = profileController;
            _profileService = profileService;
            _repetitionService = repetitionService;

            _mainScreenBus.OnGameBookLessonClicked += OnGameBookLessonClicked;
            _mainScreenBus.OnGameBookSectionRepeatClicked += OnGameBookSectionRepeatClicked;
            _mainScreenBus.OnRepeatClicked += OnGeneralRepeatClicked;
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _mainScreenBus.OnGameBookLessonClicked -= OnGameBookLessonClicked;
            _mainScreenBus.OnGameBookSectionRepeatClicked -= OnGameBookSectionRepeatClicked;
            _mainScreenBus.OnRepeatClicked -= OnGeneralRepeatClicked;
            _cts.Cancel();
            _cts.Dispose();
        }

        public void Init(Action onExitState)
        {
            _onExitState = onExitState;

            _view.Init(OnToggleSelected);
            _gameBookController.Init();
            _repetitionController.Init();
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
            if (_isLoading || !isOn)
                return;

            switch (tabType)
            {
                case MainTabType.Lessons:
                    _gameBookController.Set();
                    break;

                case MainTabType.Repetition:
                    _repetitionController.Set();
                    break;

                case MainTabType.Profile:
                    _profileController.Set();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null);
            }

            _gameBookController.SetViewActive(tabType == MainTabType.Lessons);
            _repetitionController.SetViewActive(tabType == MainTabType.Repetition);
            _profileController.SetViewActive(tabType == MainTabType.Profile);
            _currentTabType = tabType;
        }

        private void OnGameBookLessonClicked(string sectionName, int lessonIndex)
        {
            OnGameBookLessonClickedAsync(sectionName, lessonIndex, _cts.Token).Forget();
        }

        private async UniTaskVoid OnGameBookLessonClickedAsync(string sectionName, int lessonIndex, CancellationToken ct)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            SimpleLessonData simpleLesson;
            string key = _profileService.ReorderedSectionKey(sectionName);
            if (_profileService.ReorderedSections.TryGetValue(key, out SimpleSection section))
            {
                simpleLesson = section.Lessons[lessonIndex - 1];
            }
            else
            {
                key = $"{_profileService.ProfileData.LearnLanguage}Lesson{sectionName}_{lessonIndex}";
                simpleLesson = _gameBus.SimpleLessons[key];
            }

            Lesson lesson = new Lesson();
            lesson.FileName = simpleLesson.FileName;
            lesson.GenerateQuestMatchWordsData = simpleLesson.GenerateQuestMatchWordsData;
            lesson.SetSimpleQuestions(simpleLesson.Questions.ToList());

            _gameBus.CurrentLesson = lesson;
            _isLoading = false;

            _gameBus.GameType = GameType.Learn;
            _onExitState?.Invoke();
        }

        private void OnGameBookSectionRepeatClicked(string section)
        {
            if (_isLoading)
                return;

            var repetitions = _repetitionService.GetSectionRepetition(_gameBus.CurrentLanguage, GeneralRepetitionAmount, section);
            MakeRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        private void OnGeneralRepeatClicked()
        {
            if (_isLoading)
                return;

            var repetitions = _repetitionService.GetGeneralRepetition(_gameBus.CurrentLanguage, GeneralRepetitionAmount);
            MakeRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        private async UniTaskVoid MakeRepetitionAsync(List<QuestLog> repetitions, CancellationToken ct)
        {
            if (repetitions.Count < 4)
            {
                Debug.LogWarning("Not enough logs for general repetition");
                return;
            }

            _isLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            var questions = new List<ISimpleQuestion>();

            foreach (var questLog in repetitions)
            {
                switch (questLog.QuestionType)
                {
                    case QuestionType.SelectWord:
                        var simpleQuest = new SimpleQuestSelectWord();
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

            var lesson = new Lesson();
            lesson.GenerateQuestMatchWordsData = true;
            lesson.SetSimpleQuestions(questions);

            _gameBus.CurrentLesson = lesson;
            _isLoading = false;

            _gameBus.GameType = GameType.Repetition;
            _onExitState?.Invoke();
        }
    }
}
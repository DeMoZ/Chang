using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly RepetitionService _repetitionService;

        private bool _isLoading;

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
            RepetitionService repetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _gameBookController = gameBookController;
            _repetitionController = repetitionController;
            _profileController = profileController;
            _repetitionService = repetitionService;

            _mainScreenBus.OnGameBookLessonClicked += OnGameBookLessonClicked;
            _mainScreenBus.OnGameBookSectionRepeatClicked += OnGameBookLessonRepeatClicked;
            _mainScreenBus.OnRepeatClicked += OnGeneralRepeatClicked;
        }

        public void Dispose()
        {
            _mainScreenBus.OnGameBookLessonClicked -= OnGameBookLessonClicked;
            _mainScreenBus.OnGameBookSectionRepeatClicked -= OnGameBookLessonRepeatClicked;
            _mainScreenBus.OnRepeatClicked -= OnGeneralRepeatClicked;
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

        private void OnGameBookLessonClicked(string name)
        {
            OnGameBookLessonClickedAsync(name).Forget();
        }

        private async UniTask OnGameBookLessonClickedAsync(string name)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);

            var simpleLesson = _gameBus.SimpleLessons[name];
            var lesson = new Lesson();
            lesson.FileName = simpleLesson.FileName;
            lesson.GenerateQuestMatchWordsData = simpleLesson.GenerateQuestMatchWordsData;
            lesson.SetSimpleQuestions(simpleLesson.Questions.ToList());

            _gameBus.CurrentLesson = lesson;
            _isLoading = false;

            _gameBus.GameType = GameType.Learn;
            _onExitState?.Invoke();
        }

        private void OnGameBookLessonRepeatClicked(string section)
        {
            OnGameBookLessonRepeatClickedAsync(section).Forget();
        }

        private async UniTask OnGameBookLessonRepeatClickedAsync(string section)
        {
            if (_isLoading)
                return;

            var repetitions = _repetitionService.GetSectionRepetition(_gameBus.CurrentLanguage, GeneralRepetitionAmount, section);
            await MakeRepetitionAsync(repetitions);
        }

        private void OnGeneralRepeatClicked()
        {
            OnGeneralRepeatClickedAsync().Forget();
        }

        private async UniTask OnGeneralRepeatClickedAsync()
        {
            if (_isLoading)
                return;

            var repetitions = _repetitionService.GetGeneralRepetition(_gameBus.CurrentLanguage, GeneralRepetitionAmount);
            await MakeRepetitionAsync(repetitions);
        }

        private async UniTask MakeRepetitionAsync(List<QuestLog> repetitions)
        {
            if (repetitions.Count < 4)
            {
                Debug.LogWarning("Not enough logs for general repetition");
                return;
            }

            _isLoading = true;
            await UniTask.DelayFrame(1);

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
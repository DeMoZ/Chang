using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Zenject;
using Chang.Services;
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
            RepetitionService repetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _gameBookController = gameBookController;
            _repetitionController = repetitionController;
            _repetitionService = repetitionService;

            _mainScreenBus.OnGameBookLessonClicked += OnGameBookLessonClickedAsync;
            _mainScreenBus.OnRepeatClicked += OnGeneralRepeatClickedAsync;
        }

        public void Dispose()
        {
            _mainScreenBus.OnGameBookLessonClicked -= OnGameBookLessonClickedAsync;
            _mainScreenBus.OnRepeatClicked -= OnGeneralRepeatClickedAsync;
        }

        public void Init(Action onExitState)
        {
            _onExitState = onExitState;

            _view.Init(OnToggleSelected);
            _gameBookController.Init();
            _repetitionController.Init();
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null);
            }

            _gameBookController.SetViewActive(tabType == MainTabType.Lessons);
            _repetitionController.SetViewActive(tabType == MainTabType.Repetition);
            _currentTabType = tabType;
        }

        private async void OnGameBookLessonClickedAsync(string name)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);
            _gameBus.CurrentLesson.SetFileName(name);
            _gameBus.CurrentLesson.SetSimpQuesitons(_gameBus.SimpleLessons[name].Questions);

            _gameBus.PreloadFor = PreloadType.LessonConfig;
            _isLoading = false;

            _onExitState?.Invoke();
        }

        private async void OnGeneralRepeatClickedAsync()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            await UniTask.DelayFrame(1);

            _gameBus.CurrentLesson = new Lesson();

            var repetitions = _repetitionService.GetGeneralRepetition(GeneralRepetitionAmount);
            var questions = repetitions.Select(q => _gameBus.SimpleQuestions[q.FileName]).ToList();
            // _gameBus.CurrentLesson.SetFileName(string.Empty); // todo roman need to not to mark lesson as finished on end repetition
            _gameBus.CurrentLesson.SetSimpQuesitons(questions);

            _gameBus.PreloadFor = PreloadType.QuestConfigs;
            _isLoading = false;

            _onExitState?.Invoke();
        }
    }
}
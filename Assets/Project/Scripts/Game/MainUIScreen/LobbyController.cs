using System;
using System.Collections.Generic;
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

        private async void OnGameBookLessonClickedAsync(string name)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);
            _gameBus.CurrentLesson.SetFileName(name);
            //_gameBus.CurrentLesson.SetSimpleQuestions(_gameBus.SimpleLessons[name].Questions);
            _gameBus.CurrentLesson.SetSimpleQuestions(_gameBus.SimpleLessons[name].Questions.Cast<ISimpleQuestion>().ToList());
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
            List<ISimpleQuestion> questions = repetitions.Select(q => _gameBus.SimpleQuestions[q.FileName]).ToList();
            // _gameBus.CurrentLesson.SetFileName(string.Empty); // todo roman need to not to mark lesson as finished on end repetition
            _gameBus.CurrentLesson.SetSimpleQuestions(questions);

            _gameBus.PreloadFor = PreloadType.QuestConfigs;
            _isLoading = false;

            _onExitState?.Invoke();
        }
    }
}
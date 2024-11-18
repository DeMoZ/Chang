using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class MainUiController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly MainUiView _view;
        private readonly GameBookController _gameBookController;
        private readonly RepetitionController _repetitionController;

        private bool _isLoading;

        /// <summary>
        /// should return to this tab after play any other game state
        /// </summary>
        private MainTabType _currentTabType = MainTabType.Lessons;
        private Action _onExitState;

        [Inject]
        public MainUiController(GameBus gameBus, MainScreenBus mainScreenBus, MainUiView view, GameBookController gameBookController, RepetitionController repetitionController)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _gameBookController = gameBookController;
            _repetitionController = repetitionController;

            _mainScreenBus.OnGameBookLessonClicked += OnGameBookLessonClickedAsync;
            _mainScreenBus.OnRepeatClicked += OnRepeatClickedAsync;
        }

        public void Dispose()
        {
            _mainScreenBus.OnGameBookLessonClicked -= OnGameBookLessonClickedAsync;
            _mainScreenBus.OnRepeatClicked -= OnRepeatClickedAsync;
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

        private void OnToggleSelected(bool isOn, MainTabType lessons)
        {
            if (_isLoading)
                return;

            if (!isOn)
            {
                return;
            }

            if (lessons == MainTabType.Lessons)
            {
                _gameBookController.Set();
            }
            else if (lessons == MainTabType.Repetition)
            {
                _repetitionController.Set();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(lessons), lessons, null);
            }

            _gameBookController.SetViewActive(lessons == MainTabType.Lessons);
            _repetitionController.SetViewActive(lessons == MainTabType.Repetition);
        }

        private async void OnGameBookLessonClickedAsync(string name)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);
            _gameBus.CurrentLesson.SetFileName(name);
            _gameBus.CurrentLesson.SetSimpQuesitons(_gameBus.Lessons[name].Questions);

            _gameBus.PreloadFor = PreloadType.LessonConfig;
            _isLoading = false;

            _onExitState?.Invoke();
        }

        private async void OnRepeatClickedAsync()
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);

            // todo roman need to create new lesson with questions from repetition
            var questions = new List<QuestionConfig>();

            // foreach (var quest in maidLesson)
            // {
            //     var question = await load question config quest.name
            //     questions.Add(question);
            // }

            _gameBus.PreloadFor = PreloadType.QuestConfigs;
            _isLoading = false;

            _onExitState?.Invoke();
        }
    }
}
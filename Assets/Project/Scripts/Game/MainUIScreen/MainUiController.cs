using System;
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

            _mainScreenBus.OnGameBookLessonClicked += OnGameBookLessonClicked;
        }

        // todo roman on lobby enter trigger init method
        public void Init(Action onExitState)
        {
            _onExitState = onExitState;
            _view.Init(_currentTabType, OnToggleSelected);
            OnToggleSelected(true, _currentTabType);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        // todo roman on toggle selected i have to enable / disable one of the controllers
        private void OnToggleSelected(bool isOn, MainTabType lessons)
        {
            if (_isLoading)
                return;

            // if (isOn)
            //     Debug.Log($"Toggle selected: {lessons}");

            switch (lessons)
            {
                case MainTabType.Lessons:
                    if (isOn)
                    {
                        // _gameBookController.Init(_onExitState);
                        _gameBookController.Init();
                    }
                    _gameBookController.SetViewActive(isOn);
                    break;

                case MainTabType.Repetition:
                    if (isOn)
                    {
                        _repetitionController.Init();
                    }
                    _repetitionController.SetViewActive(isOn);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lessons), lessons, null);
            }
        }

        private void OnGameBookLessonClicked(string name)
        {
            OnGameBookLessonClickedAsync(name).Forget();
        }

        private async UniTaskVoid OnGameBookLessonClickedAsync(string name)
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

        public void Dispose()
        {

        }
    }
}
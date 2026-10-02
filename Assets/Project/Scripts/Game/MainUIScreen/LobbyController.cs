using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using Chang.GameBook;
using Chang.Services;
using Chang.Vocabulary;
using Chang.Sentences;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class LobbyController : IViewController
    {
        private readonly MainScreenBus _mainScreenBus;
        private readonly MainUiView _view;
        private readonly VocabularyController _vocabularyController;
        private readonly SentencesController _sentencesController;
        private readonly RepetitionController _repetitionController;
        private readonly ProfileController _profileController;
        private readonly GameBus _gameBus;
        private readonly RepetitionService _repetitionService;
        private readonly RepetitionLessonBuilder _repetitionLessonBuilder;

        private Action _onExitState;

        private CancellationTokenSource _cts;
        private CancellationTokenSource _tabCts;

        /// <summary>
        /// should return to this tab after play any other game state
        /// </summary>
        private MainTabType _currentTabType = MainTabType.Vocabulary;

        [Inject]
        public LobbyController(
            MainScreenBus mainScreenBus,
            MainUiView view,
            VocabularyController vocabularyController,
            RepetitionController repetitionController,
            SentencesController sentencesController,
            ProfileController profileController,
            GameBus gameBus,
            RepetitionService repetitionService,
            RepetitionLessonBuilder repetitionLessonBuilder)
        {
            _mainScreenBus = mainScreenBus;
            _view = view;
            _vocabularyController = vocabularyController;
            _repetitionController = repetitionController;
            _sentencesController = sentencesController;
            _profileController = profileController;
            _gameBus = gameBus;
            _repetitionService = repetitionService;
            _repetitionLessonBuilder = repetitionLessonBuilder;

            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _tabCts?.Cancel();
            _tabCts?.Dispose();
            _cts.Cancel();
            _cts.Dispose();
        }

        public void Init(Action onExitState)
        {
            _onExitState = onExitState;
            _view.Init(OnToggleSelected);
            _vocabularyController.Init(onExitState);
            _sentencesController.Init(onExitState);
            _repetitionController.Init(
                _vocabularyController.OnGeneralRepeatClicked,
                _sentencesController.OnGeneralRepeatClicked,
                OnMixedRepeatClicked);
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
            if (_mainScreenBus.IsLoading || !isOn)
                return;

            _tabCts?.Cancel();
            _tabCts?.Dispose();
            _tabCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);

            OnToggleSelectedAsync(tabType, _tabCts.Token).Forget();
        }

        private async UniTaskVoid OnToggleSelectedAsync(MainTabType tabType, CancellationToken ct)
        {
            _vocabularyController.SetViewActive(tabType == MainTabType.Vocabulary);
            _sentencesController.SetViewActive(tabType == MainTabType.Sentences);
            _repetitionController.SetViewActive(tabType == MainTabType.Repetition);
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
                    await _repetitionController.SetAsync(ct);
                    break;

                case MainTabType.Profile:
                    await _profileController.SetAsync(ct);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null);
            }
        }

        /// <summary>
        /// Repetition of words and sentences together
        /// </summary>
        private void OnMixedRepeatClicked()
        {
            if (_mainScreenBus.IsLoading)
            {
                return;
            }

            List<RepetitionCandidate> repetitions = _repetitionService.GetMixedRepetition();
            if (repetitions.Count == 0)
            {
                Debug.LogWarning("No played keys for repetition");
                return;
            }

            _gameBus.SetLesson(_repetitionLessonBuilder.Build(repetitions));
            _gameBus.GameType = GameType.Repetition;
            _onExitState?.Invoke();
        }
    }
}
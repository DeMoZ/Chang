using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using Chang.Services;
using Chang.Vocabulary;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class LobbyController : IViewController
    {
        private readonly MainScreenBus _mainScreenBus;
        private readonly MainUiView _view;
        private readonly Vocabulary.VocabularyController _vocabularyController;
        private readonly VocabularyRepetitionController _vocabularyRepetitionController;
        private readonly Sentences.SentencesController _sentencesController;
        private readonly SentencesRepetitionController _sentencesRepetitionController;
        private readonly ProfileController _profileController;

        // private bool _isLoading;
        private CancellationTokenSource _cts;

        /// <summary>
        /// should return to this tab after play any other game state
        /// </summary>
        private MainTabType _currentTabType = MainTabType.Vocabulary;

        private IBookController _currentController;

        [Inject]
        public LobbyController(
            MainScreenBus mainScreenBus,
            MainUiView view,
            Vocabulary.VocabularyController vocabularyController,
            VocabularyRepetitionController vocabularyRepetitionController,
            VocabularyRepetitionService vocabularyRepetitionService,
            Sentences.SentencesController sentencesController,
            SentencesRepetitionController sentencesRepetitionController,
            SentencesRepetitionService sentencesRepetitionService,
            ProfileController profileController)
        {
            _mainScreenBus = mainScreenBus;
            _view = view;
            _vocabularyController = vocabularyController;
            _vocabularyRepetitionController = vocabularyRepetitionController;
            _sentencesController = sentencesController;
            _sentencesRepetitionController = sentencesRepetitionController;
            _profileController = profileController;

            _cts = new CancellationTokenSource();

            _mainScreenBus.OnLessonClicked += OnLessonClicked;
            _mainScreenBus.OnSectionRepeatClicked += OnSectionRepeatClicked;
            _mainScreenBus.OnRepeatClicked += OnGeneralRepeatClicked;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();

            _mainScreenBus.OnLessonClicked -= OnLessonClicked;
            _mainScreenBus.OnSectionRepeatClicked -= OnSectionRepeatClicked;
            _mainScreenBus.OnRepeatClicked -= OnGeneralRepeatClicked;
        }

        public void Init(Action onExitState)
        {
            _view.Init(OnToggleSelected);
            _vocabularyController.Init(onExitState);
            _sentencesController.Init(onExitState);
            _vocabularyRepetitionController.Init();
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
            if (_mainScreenBus.IsLoading || !isOn)
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
                    _currentController = _vocabularyController;
                    await _vocabularyController.SetAsync(ct);
                    break;

                case MainTabType.Sentences:
                    _currentController = _sentencesController;
                    await _sentencesController.SetAsync(ct);
                    break;

                case MainTabType.Repetition:
                    _currentController = null;
                    await _vocabularyRepetitionController.SetAsync(ct);
                    break;

                case MainTabType.Profile:
                    _currentController = null;
                    await _profileController.SetAsync(ct);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null);
            }
        }

        private void OnLessonClicked(string sectionName, int lessonIndex)
        {
            _currentController?.OnLessonClicked( sectionName,  lessonIndex);
        }

        private void OnSectionRepeatClicked(string section)
        {
            _currentController?.OnSectionRepeatClicked(section);
        }

        private void OnGeneralRepeatClicked()
        {
            _currentController?.OnGeneralRepeatClicked();
        }
    }
}
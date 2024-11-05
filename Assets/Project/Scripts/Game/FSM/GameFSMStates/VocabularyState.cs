using System;
using Chang.Profile;
using Chang.Resources;
using Chang.Services;
using DMZ.FSM;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class VocabularyState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        public override StateType Type => StateType.PlayVocabulary;

        private readonly IResourcesManager _resourcesManager;
        private readonly GameOverlayController _overlayController;
        private readonly ProfileService _profileService;

        private VocabularyBus _vocabularyBus;
        private VocabularyFSM _vocabularyFSM;

        public VocabularyState(GameBus gameBus, Action<StateType> onStateResult, IResourcesManager resourcesManager, ProfileService profileService) :
            base(gameBus, onStateResult)
        {
            _overlayController = Bus.ScreenManager.GameOverlayController;
            _resourcesManager = resourcesManager;
            _profileService = profileService;
        }

        public void Dispose()
        {
            _vocabularyFSM.Dispose();
        }

        public override void Enter()
        {
            base.Enter();

            Bus.ScreenManager.SetActivePagesContainer(true);

            Bus.ScreenManager.GameOverlayController.OnCheck += OnCheck;
            Bus.ScreenManager.GameOverlayController.OnContinue += OnContinue;
            // todo roman implement phonetic toggle
            //Bus.ScreenManager.GameOverlayController.OnPhonetic += OnPhonetic;

            _vocabularyBus = new VocabularyBus
            {
                ScreenManager = Bus.ScreenManager,
                CurrentLesson = Bus.CurrentLesson,
            };

            _vocabularyFSM = new VocabularyFSM(_vocabularyBus);
            _vocabularyFSM.Initialize();
            OnContinue();
        }

        public override void Exit()
        {
            base.Exit();

            Bus.ScreenManager.SetActivePagesContainer(false);
            Bus.ScreenManager.GameOverlayController.OnCheck -= OnCheck;
            Bus.ScreenManager.GameOverlayController.OnContinue += OnContinue;
        }

        private void OnCheck()
        {
            // get current state result, may be show the hint.... (as hint I will show the correct answer)
            Debug.Log($"{nameof(OnCheck)}");

            // if the answer is correct
            var isCorrect = _vocabularyBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _vocabularyBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");

            _profileService.AddLog(_vocabularyBus.CurrentLesson.CurrentSimpQuestion.FileName, new LogUnit(DateTime.UtcNow, isCorrect));
            _profileService.SavePrefs();

            if (!isCorrect)
            {
                _vocabularyBus.CurrentLesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo
            {
                IsCorrect = isCorrect,
                InfoText = _vocabularyBus.QuestionResult.Info[0]
            };

            _overlayController.SetContinueButtonInfo(info);
            _overlayController.EnableContinueButton(true);
        }

        private async void OnContinue()
        {
            // todo roman check for empty queue etc 
            _vocabularyBus.CurrentLesson.SetCurrentSimpQiestion();
#if UNITY_WEBGL
            var questionConfig = await _resourcesManager.LoadAssetAsync<QuestionConfig>(_vocabularyBus.CurrentLesson.CurrentSimpQuestion.FileName);
#else
            var questionConfig = _resourcesManager.LoadAssetSync<QuestionConfig>(_vocabularyBus.CurrentLesson.CurrentSimpQuestion.FileName);
#endif
            _vocabularyBus.CurrentLesson.SetCurrentQuestionConfig(questionConfig.QuestionData);
            _vocabularyFSM.SwitchState(questionConfig.QuestionType);
        }
    }

    public struct ContinueButtonInfo
    {
        public bool IsCorrect;
        public string InfoText;
    }
}
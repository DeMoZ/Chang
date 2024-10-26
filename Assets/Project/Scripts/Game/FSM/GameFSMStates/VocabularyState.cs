using System;
using Chang.Resources;
using DMZ.FSM;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class VocabularyState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        public override StateType Type => StateType.PlayVocabulary;

        private readonly IResourcesManager _resourcesManager;
        private readonly GameOverlayController _overlayController;

        private VocabularyBus _vocabularyBus;
        private VocabularyFSM _vocabularyFSM;
        
        public VocabularyState(GameBus gameBus, Action<StateType> onStateResult, IResourcesManager resourcesManager) : base(gameBus, onStateResult)
        {
            _overlayController = Bus.ScreenManager.GameOverlayController;
            _resourcesManager = resourcesManager;
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
            Debug.Log($"The answer is <color=Yellow>{isCorrect}</color>");
            
            if (!isCorrect)
            {
               _vocabularyBus.CurrentLesson.EnqueueCurrentQuestion();
            }
            
            var info = new ContinueButtonInfo
            {
                IsCorrect = isCorrect,
                InfoText = _vocabularyBus.QuestionResult.Info
            };
            
            _overlayController.SetContinueButtonInfo(info);
            _overlayController.EnableContinueButton(true);
        }

        private void OnContinue()
        {
            // todo roman check for empty queue etc 
            _vocabularyBus.CurrentLesson.SetCurrentSimpQiestion();
            var questionConfig = _resourcesManager.LoadAssetSync<QuestionConfig>(_vocabularyBus.CurrentLesson.CurrentSimpQuestion.FileName);
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
using System;
using Chang.Profile;
using Chang.Resources;
using Chang.Services;
using DMZ.FSM;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class VocabularyState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        public override StateType Type => StateType.PlayVocabulary;

        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly ScreenManager _screenManager;
        [Inject] private readonly IResourcesManager _resourcesManager;
        
        private readonly DiContainer _diContainer;
        
        private VocabularyBus _vocabularyBus;
        private VocabularyFSM _vocabularyFSM;

        public VocabularyState(DiContainer diContainer, GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
            _diContainer = diContainer;
        }

        public void Dispose()
        {
            _vocabularyFSM.Dispose();
        }

        public override void Enter()
        {
            base.Enter();

            _screenManager.SetActivePagesContainer(true);

            _gameOverlayController.OnCheck += OnCheck;
            _gameOverlayController.OnContinue += OnContinue;
            // todo roman implement phonetic toggle
            //Bus.ScreenManager.GameOverlayController.OnPhonetic += OnPhonetic;

            _vocabularyBus = new VocabularyBus
            {
                // ScreenManager = _screenManager,
                CurrentLesson = Bus.CurrentLesson,
            };

            _vocabularyFSM = new VocabularyFSM(_diContainer, _vocabularyBus);
            _vocabularyFSM.Initialize();
            OnContinue();
        }

        public override void Exit()
        {
            base.Exit();

            _screenManager.SetActivePagesContainer(false);
            _gameOverlayController.OnCheck -= OnCheck;
            _gameOverlayController.OnContinue -= OnContinue;
        }

        private async void OnCheck()
        {
            // get current state result, may be show the hint.... (as hint I will show the correct answer)
            Debug.Log($"{nameof(OnCheck)}");

            // if the answer is correct
            var isCorrect = _vocabularyBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _vocabularyBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");

            _profileService.AddLog(_vocabularyBus.CurrentLesson.CurrentSimpleQuestion.FileName, new LogUnit(DateTime.UtcNow, isCorrect));

            // todo roman this is very temporary solution with save everywerere and need to replace with save only in prefs
            //await _profileService.SavePrefsAsync();
            await _profileService.SaveAsync();

            if (!isCorrect)
            {
                _vocabularyBus.CurrentLesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo
            {
                IsCorrect = isCorrect,
                InfoText = _vocabularyBus.QuestionResult.Info[0]
            };

            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
        }

        private async void OnContinue()
        {
            var lesson = _vocabularyBus.CurrentLesson;

            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                // the lesson has finished
                // todo roman implement ResultState. UML needs to be updated

                // todo remove Temporary solution with exit to lobby
                OnStateResult.Invoke(StateType.Lobby);
            }
            else
            {
                lesson.SetCurrentSimpQiestion();
#if UNITY_WEBGL
                var questionConfig = await _resourcesManager.LoadAssetAsync<QuestionConfig>(lesson.CurrentSimpleQuestion.FileName);
#else
                var questionConfig = _resourcesManager.LoadAssetSync<QuestionConfig>(lesson.CurrentSimpleQuestion.FileName);
#endif
                lesson.SetCurrentQuestionConfig(questionConfig.QuestionData);
                _vocabularyFSM.SwitchState(questionConfig.QuestionType);
            }
        }
    }

    public struct ContinueButtonInfo
    {
        public bool IsCorrect;
        public string InfoText;
    }
}
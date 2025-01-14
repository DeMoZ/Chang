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

        public VocabularyState(DiContainer diContainer, GameBus gameBus, Action<StateType> onStateResult)
            : base(gameBus, onStateResult)
        {
            _diContainer = diContainer;
        }

        public void Dispose()
        {
            _vocabularyFSM.Dispose();
            _vocabularyBus.Dispose();
        }

        public override void Enter()
        {
            base.Enter();

            _screenManager.SetActivePagesContainer(true);

            _gameOverlayController.OnCheck += OnCheck;
            _gameOverlayController.OnContinue += OnContinue;
            _gameOverlayController.OnReturnFromGame += ExitToLobby;

            _gameOverlayController.EnableReturnButton(true);

            // todo roman implement phonetic toggle
            //gameOverlayController.OnPhonetic += OnPhonetic;

            _vocabularyBus = new VocabularyBus
            {
                CurrentLesson = Bus.CurrentLesson,
            };

            _vocabularyFSM = new VocabularyFSM(_diContainer, _vocabularyBus);
            _vocabularyFSM.Initialize();
            OnContinue();
        }

        public override void Exit()
        {
            base.Exit();

            _vocabularyBus = null;
            _vocabularyFSM.Dispose();
            _screenManager.SetActivePagesContainer(false);
            _gameOverlayController.OnCheck -= OnCheck;
            _gameOverlayController.OnContinue -= OnContinue;
            _gameOverlayController.OnReturnFromGame -= ExitToLobby;

            _gameOverlayController.OnExitToLobby();
        }

        private void ExitToLobby()
        {
            // todo remove Temporary solution with exit to lobby, and add game result popup
            OnStateResult.Invoke(StateType.Lobby);
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

            if (!isCorrect)
            {
                _vocabularyBus.CurrentLesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo();
            info.IsCorrect = isCorrect;
            info.InfoText = (string)_vocabularyBus.QuestionResult.Info[0];

            _vocabularyBus.LessonLog.Add(_vocabularyBus.QuestionResult);
                
            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
            await _profileService.SaveAsync(); // todo roman in case of bug move before _gameOverlayController.EnableContinueButton(true); 
        }

        private async void OnContinue()
        {
            if(_vocabularyFSM.CurrentStateType == QuestionType.Result)
            {
                ExitToLobby();
                return;
            }
            
            var lesson = _vocabularyBus.CurrentLesson;

            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                // the lesson has finished
                // todo roman UML needs to be updated
                _vocabularyFSM.SwitchState(QuestionType.Result);
            }
            else
            {
                var nextQuestion = lesson.PeekNextQuestion();
                var questionConfig = await _resourcesManager.LoadAssetAsync<QuestionConfig>(nextQuestion.FileName);
                var questionData = questionConfig.GetQuestData();

                if (nextQuestion.QuestionType == QuestionType.SelectWord && IsNeedDemonstration(nextQuestion))
                {
                    var demonstration = new SimpleQuestDemonstrationWord
                    {
                        FileName = nextQuestion.FileName
                    };

                    lesson.InsertNextQuest(demonstration);
                    var questionSelectWordData = (QuestSelectWordData)questionData;
                    questionData = new QuestDemonstrateWordData(questionSelectWordData.CorrectWord);
                }

                lesson.DequeueAndSetSipmQiestion();
                lesson.SetCurrentQuestionConfig(questionData);
                _vocabularyFSM.SwitchState(questionData.QuestionType);
            }
        }

        // if no records stored about this question or the question mark is 1 (or 0)
        private bool IsNeedDemonstration(SimpleQuestionBase question)
        {
            bool logExists = _profileService.TryGetLog(question.FileName, out var questLog);

            if (!logExists)
            {
                Debug.Log($"Demonstration required. No log for: {question.FileName}");
                return true;
            }

            bool isSmallMark = questLog.Mark < 1;

            if (isSmallMark)
            {
                Debug.Log($"Demonstration required. Mark: {questLog.Mark} for: {question.FileName}");
            }

            return isSmallMark;
        }
    }
}

public struct ContinueButtonInfo
{
    public bool IsCorrect;
    public string InfoText;
}


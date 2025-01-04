using System;
using DMZ.FSM;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class DemonstrationWordResult : IQuestionResult
    {
        public QuestionType Type => QuestionType.DemonstrationWord;
        public bool IsCorrect { get; }
        public object[] Info { get; }

        public DemonstrationWordResult(params object[] info)
        {
            IsCorrect = true;
            Info = info;
        }
    }
    
    public class DemonstrationState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly DemonstrationWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private PhraseConfig _correctWord;
        
        public override QuestionType Type => QuestionType.DemonstrationWord;

        public DemonstrationState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            StateBody();
        }

        public override void Exit()
        {
            base.Exit();
            _stateController.SetViewActive(false);
        }
        
        private void StateBody()
        {
            // 1 instantiate screen and initialise with data.
            if (Bus.CurrentLesson.CurrentQuestion.QuestionType != Type)
                throw new ArgumentException("Question type doesnt match with state type");

            var questionData = (QuestSelectWord)Bus.CurrentLesson.CurrentQuestion;
            _correctWord = questionData.CorrectWord;
            
            var questInStudiedLanguage = false; // todo roman implement switch from thai to eng or from eng to thai
            _stateController.Init(questInStudiedLanguage, _correctWord, OnToggleValueChanged);
            _stateController.SetViewActive(true);
        }
        
        private void OnToggleValueChanged(bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle isOn: {isOn}");
            var info = new [] { _correctWord.Word.Phonetic };
            var result = new DemonstrationWordResult(true, info);
            Bus.QuestionResult = result;
        }
    }
}

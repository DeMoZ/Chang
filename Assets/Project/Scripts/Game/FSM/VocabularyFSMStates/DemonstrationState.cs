using System;
using System.IO;
using Chang.Resources;
using DMZ.FSM;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class DemonstrationWordResult : IQuestionResult
    {
        public string Key { get; }
        public string Presentation { get; }
        public QuestionType Type => QuestionType.DemonstrationWord;
        public bool IsCorrect => true;
        public object[] Info { get; }

        public DemonstrationWordResult(string key, string presentation, params object[] info)
        {
            Key = key;
            Presentation = presentation;
            Info = info;
        }
    }

    public class DemonstrationState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly DemonstrationWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private PhraseData _correctWord;

        public override QuestionType Type => QuestionType.DemonstrationWord;

        public DemonstrationState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _gameOverlayController.EnableHintButton(false);
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
            if (Bus.CurrentLesson.CurrentSimpleQuestion.QuestionType != Type)
                throw new ArgumentException("Question type doesnt match with state type");

            var questionData = (QuestDemonstrateWordData)Bus.CurrentLesson.CurrentQuestionData;
            _correctWord = questionData.CorrectWord;

            _stateController.Init(_correctWord, OnToggleValueChanged);
            _stateController.SetViewActive(true);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle isOn: {isOn}");
            object[] info = { _correctWord.Word.LearnWord, false };
            var result = new DemonstrationWordResult(
                Path.Combine(
                    Bus.CurrentLanguage.ToString(),
                    AssetPaths.Addressables.WORDS,
                    _correctWord.Word.Section,
                    _correctWord.Word.Key),
                _correctWord.Word.LearnWord,
                info);
            Bus.QuestionResult = result;
        }
    }
}
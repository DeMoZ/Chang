using System;
using DMZ.FSM;
using System.Collections.Generic;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class SelectWordResult : IQuestionResult
    {
        public string Key { get; }
        public QuestionType Type => QuestionType.SelectWord;
        public bool IsCorrect { get; }
        public object[] Info { get; }

        public SelectWordResult(string key, bool isCorrect, params object[] info)
        {
            Key = key;
            IsCorrect = isCorrect;
            Info = info;
        }
    }

    public class SelectWordState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly SelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private List<PhraseData> _mixWords;
        private PhraseData _correctWord;

        public override QuestionType Type => QuestionType.SelectWord;

        public SelectWordState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
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
            if (Bus.CurrentLesson.CurrentQuestionData.QuestionType != Type)
                throw new ArgumentException("Question type doesnt match with state type");

            var questionData = (QuestSelectWordData)Bus.CurrentLesson.CurrentQuestionData;
            _correctWord = questionData.CorrectWord;
            _mixWords ??= new List<PhraseData>();
            _mixWords.Clear();
            _mixWords.Add(_correctWord);
            _mixWords.AddRange(questionData.MixWords);
            _mixWords.Shuffle();

            var questInStudiedLanguage = false; // todo roman implement switch from thai to eng or from eng to thai
            _stateController.Init(questInStudiedLanguage, _correctWord, _mixWords, OnToggleValueChanged);
            _stateController.SetViewActive(true);
        }

        private void OnToggleValueChanged(int index, bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle: {index}; isOn: {isOn}");
            var isCorrect = _mixWords[index] == _correctWord;
            var correctWord = _correctWord.Word.LearnWord;
            var selectedWord = _mixWords[index].Word.LearnWord;
            object[] info = { correctWord, selectedWord };
            var result = new SelectWordResult(_correctWord.Word.Key, isCorrect, info);
            Bus.QuestionResult = result;
        }
    }
}
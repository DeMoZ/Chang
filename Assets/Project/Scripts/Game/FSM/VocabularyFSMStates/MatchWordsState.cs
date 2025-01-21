using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class MatchWordsStateResult : IQuestionResult
    {
        public readonly List<SelectWordResult> Results = new();
        public string Key { get; }
        public QuestionType Type => QuestionType.MatchWords;
        public bool IsCorrect => true;

        public object[] Info { get; }
    }

    public class MatchWordsState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly MatchWordsController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private List<WordData> _leftWords;
        private List<WordData> _rightWords;

        public override QuestionType Type => QuestionType.MatchWords;

        private int _correctCount;
        private MatchWordsStateResult _result;

        public MatchWordsState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
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

            _leftWords.Clear();
            _rightWords.Clear();
        }

        private void StateBody()
        {
            _correctCount = 0;
            _result = new MatchWordsStateResult();

            _stateController.EnableContinueButton(false);

            var questionData = (QuestMatchWordsData)Bus.CurrentLesson.CurrentQuestionData;
            var words = questionData.MatchWords.Select(p => p.Word);
            _leftWords = new List<WordData>(words);
            _rightWords = new List<WordData>(words);

            _leftWords.Shuffle();
            _rightWords.Shuffle();

            var isLeftLanguage = RandomUtils.GetRandomBool();
            Debug.Log($"isLeft: {isLeftLanguage}");
            _stateController.Init(isLeftLanguage, _leftWords, _rightWords, OnToggleValueChanged, OnContinueClicked);
            _stateController.SetViewActive(true);
        }

        private void OnToggleValueChanged(int leftIndex, int rightIndex)
        {
            var isCorrect = _leftWords[leftIndex] == _rightWords[rightIndex];
            Debug.Log($"leftIndex: {leftIndex}; rightIndex: {rightIndex}; result: {isCorrect}");

            _stateController.ShowCorrect(leftIndex, rightIndex, isCorrect);
            var leftResult = new SelectWordResult(_leftWords[leftIndex].Key, isCorrect, _leftWords[leftIndex].LearnWord,
                _leftWords[leftIndex].Phonetic);
            _result.Results.Add(leftResult);

            if (!isCorrect)
            {
                var rightResult = new SelectWordResult(_rightWords[rightIndex].Key, false, _rightWords[rightIndex].LearnWord,
                    _rightWords[rightIndex].Phonetic);
                _result.Results.Add(rightResult);
            }

            if (!isCorrect)
                return;

            _correctCount++;

            if (_correctCount == _leftWords.Count)
                _stateController.EnableContinueButton(true);
        }

        private void OnContinueClicked()
        {
            Debug.Log($"Continue clicked");

            Bus.QuestionResult = _result;
            _gameOverlayController.OnCheck?.Invoke();
        }
    }
}
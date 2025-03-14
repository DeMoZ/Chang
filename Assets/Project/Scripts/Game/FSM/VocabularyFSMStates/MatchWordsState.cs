using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class MatchWordsStateResult : IQuestionResult
    {
        public readonly List<SelectWordResult> Results = new();
        public string Key { get; }
        public string Presentation { get; }
        public QuestionType Type => QuestionType.MatchWords;
        public bool IsCorrect => true;

        public object[] Info { get; }
    }

    public class MatchWordsState : ResultStateBase<QuestionType, PagesBus>
    {
        [Inject] private readonly MatchWordsController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;

        private List<WordData> _leftWords;
        private List<WordData> _rightWords;

        public override QuestionType Type => QuestionType.MatchWords;

        private int _correctCount;
        private MatchWordsStateResult _result;

        public MatchWordsState(PagesBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Bus.OnHintUsed.Subscribe(OnHint);
            StateBody();
        }

        public override void Exit()
        {
            base.Exit();
            Bus.OnHintUsed.Unsubscribe(OnHint);
            _stateController.SetViewActive(false);
            _stateController.Clear();

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

            foreach (var word in words)
            {
                word.SetShowPhonetics(WordHelper.GetShowPhonetics(_profileService.GetMark(word.Key)));
            }

            _leftWords = new List<WordData>(words);
            _rightWords = new List<WordData>(words);

            _leftWords.Shuffle();
            _rightWords.Shuffle();

            var isLeftLearnLanguage = RandomUtils.GetRandomBool();
            Debug.Log($"isLeft: {isLeftLearnLanguage}");
            _stateController.Init(isLeftLearnLanguage, _leftWords, _rightWords, OnToggleValueChanged, OnContinueClicked);
            _stateController.SetViewActive(true);
        }

        private void OnToggleValueChanged(int leftIndex, int rightIndex)
        {
            var isCorrect = _leftWords[leftIndex] == _rightWords[rightIndex];
            Debug.Log($"leftIndex: {leftIndex}; rightIndex: {rightIndex}; result: {isCorrect}");

            _stateController.ShowCorrect(leftIndex, rightIndex, isCorrect).Forget();
            var leftResult = new SelectWordResult(
                Path.Combine(
                    Bus.CurrentLanguage.ToString(),
                    AssetPaths.Addressables.Words,
                    _leftWords[leftIndex].Section,
                    _leftWords[leftIndex].Key),
                _leftWords[leftIndex].LearnWord, isCorrect,
                _leftWords[leftIndex].LearnWord, _leftWords[leftIndex].Phonetic);

            _result.Results.Add(leftResult);

            if (!isCorrect)
            {
                var rightResult = new SelectWordResult(
                    Path.Combine(
                        Bus.CurrentLanguage.ToString(),
                        AssetPaths.Addressables.Words,
                        _rightWords[rightIndex].Section,
                        _rightWords[rightIndex].Key),
                    _rightWords[rightIndex].LearnWord, false,
                    _rightWords[rightIndex].LearnWord, _rightWords[rightIndex].Phonetic);

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

        private void OnHint(bool isHintUsed)
        {
            _stateController.ShowHint();
        }
    }
}
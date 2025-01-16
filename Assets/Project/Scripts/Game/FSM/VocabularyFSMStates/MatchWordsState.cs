using System;
using DMZ.FSM;
using System.Collections.Generic;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class MatchWordsStateResult : IQuestionResult
    {
        public string Key { get; }
        public QuestionType Type => QuestionType.MatchWords;
        public bool IsCorrect { get; }

        public object[] Info { get; }
        //
        // public MatchWordsStateResult(string word, bool isCorrect, params object[] info)
        // {
        //     Word = word;
        //     IsCorrect = isCorrect;
        //     Info = info;
        // }
    }

    public class MatchWordsState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly MatchWordsController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        public override QuestionType Type => QuestionType.MatchWords;

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
        }

        private void StateBody()
        {
            // // 1 instantiate screen and initialise with data.
            // if (Bus.CurrentLesson.CurrentQuestion.QuestionType != Type)
            //     throw new ArgumentException("Question type doesnt match with state type");
            //
            // var questionData = (QuestSelectWordData)Bus.CurrentLesson.CurrentQuestion;
            // _correctWord = questionData.CorrectWord;
            // _mixWords ??= new List<PhraseData>();
            // _mixWords.Clear();
            // _mixWords.Add(_correctWord);
            // _mixWords.AddRange(questionData.MixWords);
            // Shuffle(_mixWords);
            //
            // var questInStudiedLanguage = false; // todo roman implement switch from thai to eng or from eng to thai
            // _stateController.Init(questInStudiedLanguage, _correctWord, _mixWords, OnToggleValueChanged);
            // _stateController.SetViewActive(true);

            var left = new List<WordData>();
            var right = new List<WordData>();
            
            // todo roman populate left and right lists and shafle them
            var questionData = (QuestMatchWordsData)Bus.CurrentLesson.CurrentQuestionData;
            
            bool isLeft = UnityEngine.Random.Range(0, 2) == 0;
            _stateController.Init(isLeft, left, right, OnToggleValueChanged, OnContinueClicked);
            _stateController.SetViewActive(true);
        }

        private void OnToggleValueChanged(bool isLeft, int index, bool isOn)
        {
            // _gameOverlayController.EnableCheckButton(isOn);
            // Debug.Log($"toggle: {index}; isOn: {isOn}");
            // var isCorrect = _mixWords[index] == _correctWord;
            // object[] info = { _correctWord.Word.Phonetic, _mixWords[index].Word.Phonetic };
            // var result = new SelectWordResult( _correctWord.Word.Word, isCorrect, info);
            // Bus.QuestionResult = result;
        }

        private void OnContinueClicked()
        {
            
        }

        private void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
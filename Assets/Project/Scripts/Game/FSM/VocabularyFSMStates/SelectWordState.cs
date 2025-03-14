using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.IO;
using Chang.Resources;
using Chang.Services;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class SelectWordResult : IQuestionResult
    {
        public string Key { get; }
        public string Presentation { get; }
        public QuestionType Type => QuestionType.SelectWord;
        public bool IsCorrect { get; }
        public object[] Info { get; }

        public SelectWordResult(string key, string presentation, bool isCorrect, params object[] info)
        {
            Key = key;
            Presentation = presentation;
            IsCorrect = isCorrect;
            Info = info;
        }
    }

    public class SelectWordState : ResultStateBase<QuestionType, PagesBus>
    {
        [Inject] private readonly SelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;

        private List<PhraseData> _mixWords;
        private PhraseData _correctWord;

        public override QuestionType Type => QuestionType.SelectWord;

        public SelectWordState(PagesBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Bus.OnHintUsed.Subscribe(OnHint);
            _gameOverlayController.EnableHintButton(true);
            StateBody();
        }

        public override void Exit()
        {
            base.Exit();
            Bus.OnHintUsed.Unsubscribe(OnHint);
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

            var mark = _profileService.GetMark(_correctWord.Word.Key);
            var isQuestInTranslation = WordHelper.GetQuestInTranslation(mark);
            _correctWord.SetPhonetics(WordHelper.GetShowPhonetics(mark));

            foreach (var mixWord in _mixWords)
            {
                mark = _profileService.GetMark(mixWord.Word.Key);
                mixWord.SetPhonetics(WordHelper.GetShowPhonetics(mark));
            }

            _stateController.Init(isQuestInTranslation, _correctWord, _mixWords, OnToggleValueChanged, OnClickPlaySound);
            _stateController.SetViewActive(true);

            OnClickPlaySound();
        }

        private void OnClickPlaySound()
        {
            _pagesSoundController.PlaySound(_correctWord.AudioClip);
        }

        private void OnHint(bool isHintUsed)
        {
            _stateController.ShowHint();
        }

        private void OnToggleValueChanged(int index, bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle: {index}; isOn: {isOn}");
            var isCorrect = _mixWords[index].Key == _correctWord.Key;
            object[] info = { _correctWord.Word.LearnWord, Bus.OnHintUsed.Value };
            var result = new SelectWordResult(
                Path.Combine(
                    Bus.CurrentLanguage.ToString(),
                    AssetPaths.Addressables.Words,
                    _correctWord.Word.Section,
                    _correctWord.Word.Key),
                _correctWord.Word.LearnWord, isCorrect, info);
            Bus.QuestionResult = result;
        }
    }
}
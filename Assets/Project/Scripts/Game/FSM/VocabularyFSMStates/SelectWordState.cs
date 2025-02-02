using System;
using DMZ.FSM;
using System.Collections.Generic;
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

    public class SelectWordState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly SelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;

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

            var mark = _profileService.GetMark(_correctWord.Word.Key);
            var isQuestInTranslation = GetQuestInTranslation(mark);
            _correctWord.SetPhonetics(GetShowPhonetic(mark));

            foreach (var mixWord in _mixWords)
            {
                mark = _profileService.GetMark(mixWord.Word.Key);
                mixWord.SetPhonetics(GetShowPhonetic(mark));
            }

            _stateController.Init(isQuestInTranslation, _correctWord, _mixWords, OnToggleValueChanged);
            _stateController.SetViewActive(true);
        }

        private void OnToggleValueChanged(int index, bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle: {index}; isOn: {isOn}");
            var isCorrect = _mixWords[index].Key == _correctWord.Key;
            var correctWord = _correctWord.Word.LearnWord;
            var selectedWord = _mixWords[index].Word.LearnWord;
            object[] info = { correctWord, selectedWord };
            var result = new SelectWordResult(_correctWord.Word.Key, _correctWord.Word.LearnWord, isCorrect, info);
            Bus.QuestionResult = result;
        }

        private static bool GetQuestInTranslation(int mark)
        {
            return mark switch
            {
                < 3 => false,
                < 5 => true,
                _ => RandomUtils.GetRandomBool()
            };
        }

        private static bool GetShowPhonetic(int mark)
        {
            return mark < 7;
        }
    }
}
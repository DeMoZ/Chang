using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Popup;
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
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PopupManager _popupManager;

        private List<PhraseData> _mixWords;
        private PhraseData _correctWord;
        private PageService _pageService;

        public override QuestionType Type => QuestionType.SelectWord;

        public SelectWordState(PagesBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();

            _pageService = new PageService(_wordPathHelper, _assetManager, _popupManager);
            Bus.OnHintUsed.Subscribe(OnHint);
            _gameOverlayController.EnableHintButton(true);
            StateBodyAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();

            _pageService.Dispose();
            _correctWord = null;
            _mixWords?.Clear();
            _mixWords = null;
            Bus.OnHintUsed.Unsubscribe(OnHint);
            _stateController.SetViewActive(false);
        }

        private async UniTask StateBodyAsync()
        {
            ISimpleQuestion question = Bus.CurrentLesson.CurrentSimpleQuestion;

            await _pageService.LoadContentAsync(question);

            QuestSelectWordData questionData = GetQuestionData((SimpleQuestSelectWord)question);
            _correctWord = questionData.CorrectWord;
            _mixWords ??= new List<PhraseData>();
            _mixWords.Clear();
            _mixWords.Add(_correctWord);
            _mixWords.AddRange(questionData.MixWords);
            _mixWords.Shuffle();

            string key = $"{_profileService.ProfileData.LearnLanguage}/{_correctWord.Word.LogKey}"; // todo chang use section lang/section/word
            int mark = _profileService.GetMark(key);
            bool isQuestInTranslation = WordHelper.GetQuestInTranslation(mark);
            _correctWord.SetPhonetics(WordHelper.GetShowPhonetics(mark));

            foreach (var mixWord in _mixWords)
            {
                mark = _profileService.GetMark(mixWord.LogKey);
                mixWord.SetPhonetics(WordHelper.GetShowPhonetics(mark));
            }

            _stateController.Init(isQuestInTranslation, _correctWord, _mixWords, OnToggleValueChanged, OnClickPlaySound);
            _stateController.SetViewActive(true);

            OnClickPlaySound();
        }

        private QuestSelectWordData GetQuestionData(SimpleQuestSelectWord selectWord)
        {
            QuestSelectWordData selectWordData = new QuestSelectWordData
            {
                CorrectWord = _pageService.Configs[selectWord.CorrectWordFileName].Item.PhraseData,
                MixWords = new List<PhraseData>()
            };

            var mixWordsAmount = Bus.GameType == GameType.Learn
                ? ProjectConstants.MIX_WORDS_AMOUNT_IN_LEARN_SELECT_WORD_PAGE
                : ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE;

            var mixWords = selectWord.MixWordsFileNames.Take(mixWordsAmount);
            foreach (var fileName in mixWords)
            {
                var data = _pageService.Configs[fileName].Item.PhraseData;
                selectWordData.MixWords.Add(data);
            }

            return selectWordData;
        }

        private void OnClickPlaySound()
        {
            _pagesSoundController.PlaySound(_pageService.Sounds[_correctWord.Key].Item);
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

            string path = Path.Combine(
                _profileService.ProfileData.LearnLanguage.ToString(),
                AssetPaths.Addressables.Words,
                _correctWord.Word.Section,
                _correctWord.Word.Key);

            var result = new SelectWordResult(
                _wordPathHelper.NormalizePath(path),
                _correctWord.Word.LearnWord, isCorrect, info);
            Bus.QuestionResult = result;
        }
    }
}
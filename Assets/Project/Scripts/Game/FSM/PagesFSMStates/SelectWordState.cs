using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Popup;
using Project.Services.PagesContentProvider;
using UnityEngine;
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
        private readonly IPagesContentProvider _pagesContentProvider;
        
        [Inject] private readonly SelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PopupManager _popupManager;
        
        private List<PhraseData> _mixWords;
        private PhraseData _correctWord;
        private CancellationTokenSource _cts;

        public override QuestionType Type => QuestionType.SelectWord;

        public SelectWordState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
            _pagesContentProvider = pagesContentProvider;
        }

        public override void Enter()
        {
            base.Enter();
            
            Bus.OnHintUsed.Subscribe(OnHint);
            _gameOverlayController.EnableHintButton(true);
            _cts = new CancellationTokenSource();
            StateBodyAsync(_cts.Token).Forget();
        }

        public override void Exit()
        {
            base.Exit();
            _cts?.Cancel();
            _cts?.Dispose();
            
            _correctWord = null;
            _mixWords?.Clear();
            _mixWords = null;
            Bus.OnHintUsed.Unsubscribe(OnHint);
            _stateController.SetViewActive(false);
            _pagesContentProvider.ClearCache();
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            ISimpleQuestion question = Bus.CurrentLesson.CurrentSimpleQuestion;
            
            await _pagesContentProvider.GetContentAsync(question, ct);

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
            var path = _wordPathHelper.GetConfigPath(selectWord.CorrectWordFileName);
            var config = _pagesContentProvider.GetCachedAsset<PhraseConfig>(path);

            if (!config)
            {
                Debug.LogError($"SelectWordState: Config not found at path {path}");
                return null;
            }
            
            QuestSelectWordData selectWordData = new QuestSelectWordData
            {
                CorrectWord = config.PhraseData,
                MixWords = new List<PhraseData>()
            };

            var mixWordsAmount = Bus.GameType == GameType.Learn
                ? ProjectConstants.MIX_WORDS_AMOUNT_IN_LEARN_SELECT_WORD_PAGE
                : ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE;

            var mixWords = selectWord.MixWordsFileNames.Take(mixWordsAmount);
            foreach (var fileName in mixWords)
            {
                path = _wordPathHelper.GetConfigPath(fileName);
                var asset = _pagesContentProvider.GetCachedAsset<PhraseConfig>(path);

                if (asset)
                {
                    selectWordData.MixWords.Add(asset.PhraseData);    
                }
            }

            return selectWordData;
        }

        private void OnClickPlaySound()
        {
            var path = _wordPathHelper.GetSoundPath(_correctWord.LogKey);
            var asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);
            if (asset)
            {
                _pagesSoundController.PlaySound(asset);
            }
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
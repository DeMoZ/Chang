using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Chang.Core;
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
    public class WordResult : IQuestionResult
    {
        public virtual ChangTypes Type => ChangTypes.SelectWord;
        public Word Word { get; }
        public bool IsCorrect { get; }
        public bool IsHintUsed { get; }

        public WordResult(Word word, bool isCorrect, bool isHintUsed)
        {
            Word = word;
            IsCorrect = isCorrect;
            IsHintUsed = isHintUsed;
        }

        public string Key => Word.WordKey;
        public string Presentation => Word.LearnWord;
    }

    public class SelectWordState : ResultStateBase<ChangTypes, PagesBus>
    {
        private readonly IPagesContentProvider _pagesContentProvider;

        [Inject] private readonly SelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PopupManager _popupManager;

        private List<Word> _mixWords;
        private Word _correctWord;
        private CancellationTokenSource _cts;

        public override ChangTypes Type => ChangTypes.SelectWord;

        public SelectWordState(PagesBus bus, IPagesContentProvider pagesContentProvider,
            Action<ChangTypes> onStateResult) : base(bus, onStateResult)
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
            QuestSelectWord question = Bus.Lesson.CurrentQuestion as QuestSelectWord;

            if (question == null)
            {
                throw new Exception("SelectWordState: Current question is not of type QuestSelectWord.");
            }

            _correctWord = Bus.Words[question.Key];
            _mixWords = Bus.Words.Where(pair => question.WordsKeys.Contains(pair.Key)).Select(pair => pair.Value)
                .ToList();
            _mixWords.Add(_correctWord);
            _mixWords.Shuffle();

            int mark = _profileService.GetVocabularyMark(_correctWord.WordKey);
            bool isQuestInTranslation = WordHelper.GetQuestInTranslation(mark);

            _correctWord.SetShowPhonetics(WordHelper.GetShowPhonetics(mark));

            foreach (var mixWord in _mixWords)
            {
                mark = _profileService.GetVocabularyMark(mixWord.WordKey);
                mixWord.SetShowPhonetics(WordHelper.GetShowPhonetics(mark));
            }

            _correctWord.SetSprite(_pagesContentProvider.GetCachedSprite(_correctWord.ImageKey));
            _stateController.Init(isQuestInTranslation, _correctWord, _mixWords, OnToggleValueChanged,
                () => OnClickPlaySound(!isQuestInTranslation));
            _stateController.SetViewActive(true);

            OnClickPlaySound(!isQuestInTranslation);
        }

        private void OnClickPlaySound(bool isLearnLanguage)
        {
            string key = isLearnLanguage
                ? _correctWord.WordKey
                : _wordPathHelper.GetNativeSoundKey(_correctWord.WordKey, _profileService.ProfileData.NativeLanguage);

            AudioClip asset = _pagesContentProvider.GetCachedAudioClip(key);

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
            var result = new WordResult(_correctWord, isCorrect, Bus.OnHintUsed.Value);
            Bus.QuestionResult = result;
        }
    }
}
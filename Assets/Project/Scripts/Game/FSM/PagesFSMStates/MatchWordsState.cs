using System;
using DMZ.FSM;
using System.Collections.Generic;
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
    public class MatchWordsResult : IQuestionResult
    {
        public ChangTypes Type => ChangTypes.MatchWords;
        public List<WordResult> WordResults { get; } = new ();

        public string Key
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public string Presentation
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public bool IsCorrect
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public bool IsHintUsed
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public class MatchWordsState : ResultStateBase<ChangTypes, PagesBus>
    {
        private readonly IPagesContentProvider _pagesContentProvider;

        [Inject] private readonly MatchWordsController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly PopupManager _popupManager;

        private List<Word> _leftWords;
        private List<Word> _rightWords;
        private CancellationTokenSource _cts;
        private int _correctCount;
        private MatchWordsResult _result;

        public override ChangTypes Type => ChangTypes.MatchWords;

        public MatchWordsState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<ChangTypes> onStateResult)
            : base(bus, onStateResult)
        {
            _pagesContentProvider = pagesContentProvider;
        }

        public override void Enter()
        {
            base.Enter();

            Bus.OnHintUsed.Subscribe(OnHint);
            _cts = new CancellationTokenSource();
            StateBodyAsync(_cts.Token).Forget();
        }

        public override void Exit()
        {
            base.Exit();

            Bus.OnHintUsed.Unsubscribe(OnHint);
            _stateController.SetViewActive(false);
            _pagesContentProvider.ClearCache();
            _stateController.Clear();
            _result = null;
            _leftWords.Clear();
            _rightWords.Clear();
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            QuestMatchWords question = Bus.Lesson.CurrentQuestion as  QuestMatchWords;
            List<Word> words = Bus.Words.Where(pair => question.GetWordsKeys.Contains(pair.Key))
                .Select(pair => pair.Value)
                .ToList();

            _correctCount = 0;
            _result = new MatchWordsResult();

            _stateController.EnableContinueButton(false);
            
            foreach (var word in words)
            {
                word.SetShowPhonetics(WordHelper.GetShowPhonetics(_profileService.GetVocabularyMark(word.WordKey)));
            }

            _leftWords = new List<Word>(words);
            _rightWords = new List<Word>(words);

            _leftWords.Shuffle();
            _rightWords.Shuffle();

            var isLeftLearnLanguage = RandomUtils.GetRandomBool();
            Debug.Log($"isLeft: {isLeftLearnLanguage}");
            _stateController.Init(isLeftLearnLanguage, _leftWords, _rightWords, OnToggleValueChanged, OnContinueClicked, OnPlaySound);
            _stateController.SetViewActive(true);
        }

        private void OnPlaySound(string key, bool isLearnLanguage)
        {
            var language = isLearnLanguage 
                ? _profileService.ProfileData.LearnLanguage.ToString() 
                : _profileService.ProfileData.NativeLanguage.ToString();
            
            string path = _wordPathHelper.GetSoundPath(key);
            AudioClip asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);

            if (asset)
            {
                _pagesSoundController.PlaySound(asset);
            }
        }

        private void OnToggleValueChanged(int leftIndex, int rightIndex)
        {
            var isCorrect = _leftWords[leftIndex] == _rightWords[rightIndex];
            Debug.Log($"leftIndex: {leftIndex}; rightIndex: {rightIndex}; result: {isCorrect}");
            _stateController.ShowCorrectAsync(leftIndex, rightIndex, isCorrect).Forget();
            _result.WordResults.Add(new WordResult(_leftWords[leftIndex], isCorrect, false));

            if (!isCorrect)
            {
                _result.WordResults.Add(new WordResult(_rightWords[rightIndex], false, false));
                return;
            }
            
            _correctCount++;

            if (_correctCount == _leftWords.Count)
            {
                _stateController.EnableContinueButton(true);
            }
        }

        private void OnContinueClicked()
        {
            Debug.Log($"Continue clicked");

            Bus.QuestionResult = _result;
            _gameOverlayController.OnCheck?.Invoke();
            _stateController.EnableContinueButton(false);
        }

        private void OnHint(bool isHintUsed)
        {
            _stateController.ShowHint();
        }
    }
}
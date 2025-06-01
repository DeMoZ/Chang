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
    public class MatchWordsStateResult : IQuestionResult
    {
        public readonly List<SelectWordResult> Results = new();

        public object[] Info { get; } = null;

        public string Key { get; } = string.Empty;
        public string Presentation { get; } = string.Empty;
        public QuestionType Type => QuestionType.MatchWords;
        public bool IsCorrect => true;
    }

    public class MatchWordsState : ResultStateBase<QuestionType, PagesBus>
    {
        private readonly IPagesContentProvider _pagesContentProvider;

        [Inject] private readonly MatchWordsController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly PopupManager _popupManager;

        private List<WordData> _leftWords;
        private List<WordData> _rightWords;
        private CancellationTokenSource _cts;

        public override QuestionType Type => QuestionType.MatchWords;

        private int _correctCount;
        private MatchWordsStateResult _result;

        public MatchWordsState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<QuestionType> onStateResult) : base(bus,
            onStateResult)
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
            ISimpleQuestion question = Bus.CurrentLesson.CurrentSimpleQuestion;

            await _pagesContentProvider.GetContentAsync(question, ct);

            QuestMatchWordsData questionData = new QuestMatchWordsData(new List<PhraseData>());
            string path = string.Empty;
            foreach (var fileName in question.GetConfigKeys())
            {
                path = _wordPathHelper.GetConfigPath(fileName);
                var asset = _pagesContentProvider.GetAsset<PhraseConfig>(path);
                
                if (!asset)
                {
                    Debug.LogError($"Asset not found: {path}");
                    continue;
                }

                var data = asset.PhraseData;
                questionData.MatchWords.Add(data);
            }

            _correctCount = 0;
            _result = new MatchWordsStateResult();

            _stateController.EnableContinueButton(false);

            var words = questionData.MatchWords.Select(p => p.Word);

            foreach (var word in words)
            {
                string key = $"{_profileService.ProfileData.LearnLanguage}/{word.LogKey}";
                word.SetShowPhonetics(WordHelper.GetShowPhonetics(_profileService.GetMark(key)));
            }

            _leftWords = new List<WordData>(words);
            _rightWords = new List<WordData>(words);

            _leftWords.Shuffle();
            _rightWords.Shuffle();

            var isLeftLearnLanguage = RandomUtils.GetRandomBool();
            Debug.Log($"isLeft: {isLeftLearnLanguage}");
            _stateController.Init(isLeftLearnLanguage, _leftWords, _rightWords, OnToggleValueChanged, OnContinueClicked, OnPlaySound);
            _stateController.SetViewActive(true);
        }

        private void OnPlaySound(string key)
        {
            string path = _wordPathHelper.GetSoundPath($"{_profileService.ProfileData.LearnLanguage}/{key}");
            AudioClip asset = _pagesContentProvider.GetAsset<AudioClip>(path);

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
            string path = Path.Combine(
                _profileService.ProfileData.LearnLanguage.ToString(),
                AssetPaths.Addressables.Words,
                _leftWords[leftIndex].Section,
                _leftWords[leftIndex].Key);

            var leftResult = new SelectWordResult(
                _wordPathHelper.NormalizePath(path),
                _leftWords[leftIndex].LearnWord, isCorrect,
                _leftWords[leftIndex].LearnWord, _leftWords[leftIndex].Phonetic);

            _result.Results.Add(leftResult);

            if (!isCorrect)
            {
                path = Path.Combine(
                    _profileService.ProfileData.LearnLanguage.ToString(),
                    AssetPaths.Addressables.Words,
                    _rightWords[rightIndex].Section,
                    _rightWords[rightIndex].Key);

                var rightResult = new SelectWordResult(
                    _wordPathHelper.NormalizePath(path),
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
            _stateController.EnableContinueButton(false);
        }

        private void OnHint(bool isHintUsed)
        {
            _stateController.ShowHint();
        }
    }
}
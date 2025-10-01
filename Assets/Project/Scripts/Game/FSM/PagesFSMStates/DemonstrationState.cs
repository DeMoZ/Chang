using System;
using System.IO;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Popup;
using Project.Services.PagesContentProvider;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class DemonstrationWordResult : IQuestionResult
    {
        public string Key { get; }
        public string Presentation { get; }
        public QuestionType Type => QuestionType.DemonstrationWord;
        public bool IsCorrect => true;
        public object[] Info { get; }

        public DemonstrationWordResult(string key, string presentation, params object[] info)
        {
            Key = key;
            Presentation = presentation;
            Info = info;
        }
    }

    public class DemonstrationState : ResultStateBase<QuestionType, PagesBus>
    {
        private readonly IPagesContentProvider _pagesContentProvider;

        [Inject] private readonly DemonstrationWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PopupManager _popupManager;

        private PhraseData _correctWord;
        private CancellationTokenSource _cts;

        public override QuestionType Type => QuestionType.DemonstrationWord;

        public DemonstrationState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<QuestionType> onStateResult) : base(bus,
            onStateResult)
        {
            _pagesContentProvider = pagesContentProvider;
        }

        public override void Enter()
        {
            base.Enter();

            _gameOverlayController.EnableHintButton(false);
            _cts = new CancellationTokenSource();
            StateBodyAsync(_cts.Token).Forget();
        }

        public override void Exit()
        {
            base.Exit();
            _cts?.Cancel();
            _cts?.Dispose();

            _stateController.SetViewActive(false);
            _pagesContentProvider.ClearCache();
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            ISimpleQuestion question = Bus.CurrentLesson.CurrentSimpleQuestion;

            await _pagesContentProvider.GetContentAsync(question, ct);

            var path = _wordPathHelper.GetConfigPath(((SimpleQuestDemonstrationWord)question).CorrectWordFileName);
            var asset = _pagesContentProvider.GetCachedAsset<PhraseConfig>(path);

            if (!asset )
            {
                return;
            }
            
            QuestDemonstrateWordData questionData = new QuestDemonstrateWordData(asset.PhraseData);
            _correctWord = questionData.CorrectWord;
            
            string spritePath = _wordPathHelper.GetTexturePath(((SimpleQuestDemonstrationWord)question).CorrectWordFileName);
            var sprite = _pagesContentProvider.GetCachedSprite(spritePath);
            
            _stateController.Init(_correctWord, sprite, OnToggleValueChanged, OnClickPlaySound);
            _stateController.SetViewActive(true);

            OnClickPlaySound();
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

        private void OnToggleValueChanged(bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle isOn: {isOn}");
            object[] info = { _correctWord.Word.LearnWord, false };
            string path = Path.Combine(
                _profileService.ProfileData.LearnLanguage.ToString(),
                AssetPaths.Addressables.Words,
                _correctWord.Word.Section,
                _correctWord.Word.Key);
            var result = new DemonstrationWordResult(
                _wordPathHelper.NormalizePath(path),
                _correctWord.Word.LearnWord,
                info);
            Bus.QuestionResult = result;
        }
    }
}
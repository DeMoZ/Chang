using System;
using System.Threading;
using Chang.Core;
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
    public class DemonstrationWordResult : WordResult
    {
        public override ChangTypes Type => ChangTypes.DemonstrationWord;

        public DemonstrationWordResult(Word word, bool isCorrect, bool isHintUsed) : base(word, isCorrect, isHintUsed)
        {
        }
    }

    public class DemonstrationState : ResultStateBase<ChangTypes, PagesBus>
    {
        private readonly IPagesContentProvider _pagesContentProvider;

        [Inject] private readonly DemonstrationWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PopupManager _popupManager;

        private Word _correctWord;
        private CancellationTokenSource _cts;

        public override ChangTypes Type => ChangTypes.DemonstrationWord;

        public DemonstrationState(PagesBus bus, IPagesContentProvider pagesContentProvider,
            Action<ChangTypes> onStateResult)
            : base(bus, onStateResult)
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
            QuestSelectWord question = Bus.Lesson.CurrentQuestion as QuestSelectWord;

            if (question == null)
            {
                throw new Exception("DemonstrateWordState: Current question is not of type QuestSelectWord.");
            }

            _correctWord = Bus.Words[question.Key];
            _correctWord.SetSprite(_pagesContentProvider.GetCachedSprite(_correctWord.ImageKey));
            // the Sound Slow button is shown only when the word has a slow recording
            bool hasSlowSound = _assetManager.IsAssetExists(_wordPathHelper.GetSlowSoundPath(_correctWord.WordKey));
            // the same for the Syllables button
            bool hasSyllablesSound = _assetManager.IsAssetExists(_wordPathHelper.GetSyllablesSoundPath(_correctWord.WordKey));
            _stateController.Init(_correctWord, OnToggleValueChanged, OnClickPlaySound,
                hasSlowSound ? OnClickPlaySlowSound : null,
                hasSyllablesSound ? OnClickPlaySyllablesSound : null);
            _stateController.SetViewActive(true);

            OnClickPlaySound();
            
            await UniTask.Yield(ct);
        }

        private void OnClickPlaySound()
        {
            string path = _wordPathHelper.GetSoundPath(_correctWord.WordKey);
            AudioClip asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);

            if (asset)
            {
                _pagesSoundController.PlaySound(asset);
            }
        }

        private void OnClickPlaySlowSound()
        {
            string path = _wordPathHelper.GetSlowSoundPath(_correctWord.WordKey);
            AudioClip asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);

            if (asset)
            {
                _pagesSoundController.PlaySound(asset);
            }
        }

        private void OnClickPlaySyllablesSound()
        {
            string path = _wordPathHelper.GetSyllablesSoundPath(_correctWord.WordKey);
            AudioClip asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);

            if (asset)
            {
                _pagesSoundController.PlaySound(asset);
            }
        }

        private void OnToggleValueChanged(bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle isOn: {isOn}");
            DemonstrationWordResult result = new (_correctWord, true, false);
            Bus.QuestionResult = result;
        }
    }
}
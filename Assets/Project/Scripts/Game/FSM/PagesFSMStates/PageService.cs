using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.FSM
{
    public class PageService : IDisposable
    {
        private readonly WordPathHelper _wordPathHelper;
        private readonly IResourcesManager _assetManager;
        private readonly PopupManager _popupManager;

        private List<IDisposableAsset> _disposableAssets;
        private CancellationTokenSource _cts;

        public Dictionary<string, DisposableAsset<PhraseConfig>> Configs { get; private set; }
        public Dictionary<string, DisposableAsset<AudioClip>> Sounds { get; private set; }
        public Dictionary<string, DisposableAsset<Image>> Images { get; private set; }

        public PageService(WordPathHelper wordPathHelper, IResourcesManager assetManager, PopupManager popupManager)
        {
            _wordPathHelper = wordPathHelper;
            _assetManager = assetManager;
            _popupManager = popupManager;

            _cts = new CancellationTokenSource();

            _disposableAssets = new List<IDisposableAsset>();
            Configs = new Dictionary<string, DisposableAsset<PhraseConfig>>();
            Sounds = new Dictionary<string, DisposableAsset<AudioClip>>();
            Images = new Dictionary<string, DisposableAsset<Image>>();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            foreach (var disposable in _disposableAssets)
            {
                disposable.Dispose();
            }

            _disposableAssets = null;
            Configs = null;
            Sounds = null;
            Images = null;
        }

        public async UniTask LoadContentAsync(ISimpleQuestion nextQuestion)
        {
            var loadingModel = new LoadingUiModel(LoadingElements.Animation);
            var loadingUiController = _popupManager.ShowLoadingUi(loadingModel);
            
            var configKeys = nextQuestion.GetConfigKeys();
            var soundKeys = nextQuestion.GetSoundKeys();
            var imageKeys = nextQuestion.GetImageKeys();

            foreach (var key in configKeys)
            {
                string path = _wordPathHelper.GetConfigPath(key);
                DisposableAsset<PhraseConfig> asset = await _assetManager.LoadAssetAsync<PhraseConfig>(path, _cts.Token);
                _disposableAssets.Add(asset);
                Configs.Add(key, asset);
            }

            foreach (var key in soundKeys)
            {
                string path = _wordPathHelper.GetSoundPath(key);
                DisposableAsset<AudioClip> asset = await _assetManager.LoadAssetAsync<AudioClip>(path, _cts.Token);
                _disposableAssets.Add(asset);
                Sounds.Add(Configs[key].Item.Key, asset);
            }
            
            // todo chang images
            // foreach (var key in imageKeys)
            // {
            //     string path = _wordPathHelper.GetImagePath(key);
            //     DisposableAsset<Image> asset = await _assetManager.LoadAssetAsync<Image>(path, _cts.Token);
            //     _disposableAssets.Add(asset);
            //     Images.Add(Configs[key].Item.Key, asset);
            // }
            
            _popupManager.DisposePopup(loadingUiController);
        }
    }
}
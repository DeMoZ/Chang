using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Chang.FSM
{
    public class PageService : IDisposable
    {
        private readonly WordPathHelper _wordPathHelper;
        private readonly IResourcesManager _assetManager;

        private List<IDisposableAsset> _disposableAssets;
        private CancellationTokenSource _cts;

        public Dictionary<string, DisposableAsset<PhraseConfig>> Configs { get; private set; }
        public Dictionary<string, DisposableAsset<AudioClip>> Sounds { get; private set; }
        // Dictionary<string, DisposableAsset<Image>> _images;

        public PageService(WordPathHelper wordPathHelper, IResourcesManager assetManager)
        {
            _wordPathHelper = wordPathHelper;
            _assetManager = assetManager;

            _cts = new CancellationTokenSource();

            _disposableAssets = new List<IDisposableAsset>();
            Configs = new Dictionary<string, DisposableAsset<PhraseConfig>>();
            Sounds = new Dictionary<string, DisposableAsset<AudioClip>>();
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
            //_images = null;
        }

        public async UniTask LoadContentAsync(ISimpleQuestion nextQuestion)
        {
            var configKeys = nextQuestion.GetConfigKeys();
            var soundKeys = nextQuestion.GetSoundKeys();

            foreach (var key in configKeys)
            {
                var path = _wordPathHelper.GetConfigPath(key);
                var asset = await _assetManager.LoadAssetAsync<PhraseConfig>(path);
                _disposableAssets.Add(asset);
                Configs.Add(key, asset);
            }

            foreach (var key in soundKeys)
            {
                var path = _wordPathHelper.GetSoundPath(key);
                var asset = await _assetManager.LoadAssetAsync<AudioClip>(path);
                _disposableAssets.Add(asset);
                Sounds.Add(Configs[key].Item.Key, asset);
            }

            // todo chang images
            // Dictionary<string, List<IDisposableAsset>> images = new();
            //var imageKeys = nextQuestion.GetImageKeys();
            // foreach (var key in imageKeys)
            // {
            //    var path = _wordPathHelper.GetSoundPath(key);
            //    var asset = await _assetManager.LoadAssetAsync<Image>(path);
            //    _disposableAssets.Add(asset);
            //    _sounds.Add(_configs[key].Item.Key, asset);
            // }
        }
    }
}
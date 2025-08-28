using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Popup;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Project.Services.PagesContentProvider
{
    public class NoCachePagesContentProvider : IPagesContentProvider
    {
        private readonly IResourcesManager _assetManager;
        private readonly WordPathHelper _wordPathHelper;
        private readonly PopupManager _popupManager;

        private Dictionary<string, IDisposableAsset> Content { get; set; }

        public NoCachePagesContentProvider(IResourcesManager assetManager,
            WordPathHelper wordPathHelper,
            PopupManager popupManager)
        {
            _assetManager = assetManager;
            _wordPathHelper = wordPathHelper;
            _popupManager = popupManager;

            Content = new Dictionary<string, IDisposableAsset>();
        }

        public void Dispose()
        {
            ClearCache();

            Content = null;
        }

        public AudioClip GetCachedAudioClip(string key)
        {
            throw new NotImplementedException();
        }

        public void ClearCache()
        {
            foreach (var disposable in Content)
            {
                disposable.Value.Dispose();
            }

            Content.Clear();
        }

        public async UniTask PreloadPagesStateAsync(List<ISimpleQuestion> questions, Action<float, float> progress, CancellationToken ct)
        {
            HashSet<string> keys = new();
            foreach (ISimpleQuestion quest in questions)
            {
                keys.AddRange(quest.GetConfigKeys().Select(k => _wordPathHelper.GetConfigPath(k)));
                keys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetSoundPath(k)));
                // todo chang images keys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetImagePath(k)));
            }

            await Preload(keys, progress, ct);
        }

        public async UniTask GetContentAsync(ISimpleQuestion nextQuestion, CancellationToken ct)
        {
            var loadingModel = new LoadingUiModel(LoadingElements.Animation);
            var loadingUiController = _popupManager.ShowLoadingUi(loadingModel);

            var configKeys = nextQuestion.GetConfigKeys();
            var soundKeys = nextQuestion.GetSoundKeys();
            var imageKeys = nextQuestion.GetImageKeys();

            foreach (var key in configKeys)
            {
                string path = _wordPathHelper.GetConfigPath(key);

                if (Content.TryGetValue(path, out var configAsset))
                {
                    if (configAsset != null)
                    {
                        continue;
                    }
                }

                DisposableAsset<PhraseConfig> asset = await _assetManager.LoadAssetAsync<PhraseConfig>(path, ct);
                Content[path] = asset;
            }

            foreach (var key in soundKeys)
            {
                string path = _wordPathHelper.GetSoundPath(key);

                if (Content.TryGetValue(path, out var configAsset))
                {
                    if (configAsset != null)
                    {
                        continue;
                    }
                }

                DisposableAsset<AudioClip> asset = await _assetManager.LoadAssetAsync<AudioClip>(path, ct);
                Content[path] = asset;
            }

            // todo chang images
            // foreach (var key in imageKeys)
            // {
            //     string path = _wordPathHelper.GetImagePath(key);
            //
            //     if (Content.TryGetValue(path, out var configAsset))
            //     {
            //         if (configAsset != null)
            //         {
            //             continue;
            //         }
            //     }
            //
            //     DisposableAsset<Image> asset = await _assetManager.LoadAssetAsync<Image>(path, _cts.Token);
            //     Content[path] = asset;
            // }

            _popupManager.DisposePopup(loadingUiController);
        }

        [CanBeNull]
        public PhraseConfig GetPhraseConfig(string key)
        {
            if (Content.TryGetValue(key, out var asset))
            {
                return ((DisposableAsset<PhraseConfig>)asset).Item;
            }

            Debug.LogError($"PhraseConfig not found for key: {key}");
            return null;
        }

        [CanBeNull]
        public T GetCachedAsset<T>(string key) where T : class
        {
            if (Content.TryGetValue(key, out var asset))
            {
                if (asset is DisposableAsset<T> { Item: not null } disposableAsset)
                {
                    return disposableAsset.Item;
                }
            }

            Debug.LogError($"Item of type {typeof(T)} not found for key: {key}");
            return null;
        }

        public Sprite GetCachedSprite(string key)
        {
            throw new NotImplementedException();
        }

        private async UniTask Preload(HashSet<string> keys, Action<float, float> progress, CancellationToken ct)
        {
            Debug.Log($"{nameof(Preload)}");
            AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(Preload)} failed to get download size: {getDownloadSizeHandle.OperationException}");
                getDownloadSizeHandle.Release();
                return;
            }

            long totalDownloadSize = getDownloadSizeHandle.Result;
            Debug.Log($"Total download size: {totalDownloadSize} bytes");
            getDownloadSizeHandle.Release();

            if (totalDownloadSize == 0)
            {
                Debug.Log("No assets need to be downloaded.");
                return;
            }

            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);
            while (!downloadHandle.IsDone && !ct.IsCancellationRequested)
            {
                progress?.Invoke(downloadHandle.PercentComplete * totalDownloadSize, totalDownloadSize);
                await UniTask.Yield(ct);
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(Preload)} download failed: {downloadHandle.OperationException}");
            }

            downloadHandle.Release();
        }
    }
}
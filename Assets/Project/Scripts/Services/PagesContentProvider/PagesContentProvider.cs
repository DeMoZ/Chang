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
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Project.Services.PagesContentProvider
{
    public class PagesContentProvider : IPagesContentProvider
    {
        private readonly IResourcesManager _assetManager;
        private readonly WordPathHelper _wordPathHelper;
        private readonly PopupManager _popupManager;

        private Action<float> _percents;

        private Dictionary<string, IDisposableAsset> Content { get; set; }

        public PagesContentProvider(IResourcesManager assetManager,
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
            foreach (var disposable in Content)
            {
                disposable.Value.Dispose();
            }

            Content.Clear();
            Content = null;
        }

        public void ClearCache()
        {
            // no clear cache on pages switch, so don't implement this method
        }

        public async UniTask PreloadPagesStateAsync(List<ISimpleQuestion> questions, Action<float> percents, CancellationToken ct)
        {
            _percents = percents;

            HashSet<string> configKeys = new();
            HashSet<string> soundKeys = new();
            HashSet<string> imageKeys = new();

            HashSet<string> totalKeys = new();
            foreach (ISimpleQuestion quest in questions)
            {
                configKeys.AddRange(quest.GetConfigKeys().Select(k => _wordPathHelper.GetConfigPath(k)));
                soundKeys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetSoundPath(k)));
                // todo chang imageKeys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetImagePath(k)));
            }

            totalKeys.UnionWith(configKeys);
            totalKeys.UnionWith(soundKeys);
            totalKeys.UnionWith(imageKeys);

            long totalToLoad = await GetDownloadSize(totalKeys, ct);

            if (totalToLoad == 0)
            {
                Debug.Log("No assets need to be downloaded.");
                return;
            }

            Dictionary<string, IDisposableAsset> configs = new();
            Dictionary<string, IDisposableAsset> sounds = new();
            // todo chang Dictionary<string, IDisposableAsset> images = new();

            long currentToLoad = 0;
            long downloadSize = 0;

            downloadSize = await GetDownloadSize(configKeys, ct);
            if (downloadSize > 0)
            {
                currentToLoad += downloadSize;
                configs = await Preload<PhraseConfig>(configKeys, progress => { CountProgress(progress, currentToLoad, totalToLoad); },
                    ct);
            }

            downloadSize = await GetDownloadSize(soundKeys, ct);
            if (downloadSize > 0)
            {
                currentToLoad += downloadSize;
                sounds = await Preload<AudioClip>(soundKeys, progress => { CountProgress(progress, currentToLoad, totalToLoad); },
                    ct);
            }

            // todo chang
            // downloadSize = await GetDownloadSize(imageKeys, ct);
            // if (downloadSize > 0)
            // {
            // currentToLoad += downloadSize;
            // images = await Preload<Image>(imageKeys, progress => { CountProgress(progress, currentToLoad, totalToLoad); },
            //      ct);
            // }

            Merge(Content, configs);
            Merge(Content, sounds);
            // todo chang Merge(Content,images);
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

        private async UniTask<long> GetDownloadSize(HashSet<string> keys, CancellationToken ct)
        {
            AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);

            try
            {
                await getDownloadSizeHandle.ToUniTask(cancellationToken: ct);

                if (getDownloadSizeHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return getDownloadSizeHandle.Result;
                }

                Debug.LogError($"{nameof(GetDownloadSize)} failed to get download size: {getDownloadSizeHandle.OperationException}");
                return 0;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"{nameof(GetDownloadSize)} operation was cancelled.");
                return 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(GetDownloadSize)} failed to get download size: {ex.Message}");
                return 0;
            }
            finally
            {
                getDownloadSizeHandle.Release();
            }
        }

        private void CountProgress(float progress, long currentLoad, long totalToLoad)
        {
            var percent = (currentLoad + progress) / totalToLoad;
            _percents?.Invoke(percent);
        }

        private async UniTask<Dictionary<string, IDisposableAsset>> Preload<T>(HashSet<string> keys, Action<float> progress, CancellationToken ct)
            where T : class
        {
            Debug.Log($"{nameof(Preload)}");
            Dictionary<string, IDisposableAsset> result = new();
            List<string> keysList = keys.ToList();
            List<AsyncOperationHandle<T>> handles = new();
            List<UniTask<T>> loadAssetTasks = new();

            float[] individualProgress = new float[keysList.Count];

            for (int i = 0; i < keysList.Count; i++)
            {
                string key = keysList[i];
                int index = i;
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
                handles.Add(handle);
                loadAssetTasks.Add(handle.ToUniTask(
                    progress: Progress.Create<float>(p =>
                    {
                        individualProgress[index] = p;
                        progress?.Invoke(individualProgress.Sum());
                    }), cancellationToken: ct));
            }

            T[] completedTasks = await UniTask.WhenAll(loadAssetTasks);

            for (int i = 0; i < completedTasks.Length; i++)
            {
                result[keysList[i]] = new DisposableAsset<T>(completedTasks[i], handles[i]);
            }

            return result;
        }

        private void Merge(Dictionary<string, IDisposableAsset> toDictionary, Dictionary<string, IDisposableAsset> fromDictionary)
        {
            foreach (var pair in fromDictionary)
            {
                toDictionary.TryAdd(pair.Key, pair.Value);
            }
        }
    }
}
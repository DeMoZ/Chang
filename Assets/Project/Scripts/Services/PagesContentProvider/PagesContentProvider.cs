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
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Project.Services.PagesContentProvider
{
    public class PagesContentProvider : IPagesContentProvider
    {
        private readonly IResourcesManager _assetManager;
        private readonly WordPathHelper _wordPathHelper;
        private readonly PopupManager _popupManager;

        private Action<float, float> _progress;

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

        public async UniTask PreloadPagesStateAsync(List<ISimpleQuestion> questions, Action<float, float> progress,
            CancellationToken ct)
        {
            _progress = progress;

            HashSet<string> imageKeys = new();
            HashSet<string> soundKeys = new();
            HashSet<string> configKeys = new();

            HashSet<string> totalKeys = new();
            foreach (ISimpleQuestion quest in questions)
            {
                imageKeys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetTexturePath(k)));
                soundKeys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetSoundPath(k)));
                configKeys.AddRange(quest.GetConfigKeys().Select(k => _wordPathHelper.GetConfigPath(k)));
            }

            totalKeys.UnionWith(imageKeys);
            totalKeys.UnionWith(soundKeys);
            totalKeys.UnionWith(configKeys);

            long totalToLoad = await GetDownloadSize(totalKeys, ct);

            if (totalToLoad == 0)
            {
                Debug.Log("No assets need to be downloaded.");
                return;
            }

            Dictionary<string, IDisposableAsset> images = new();
            Dictionary<string, IDisposableAsset> sounds = new();
            Dictionary<string, IDisposableAsset> configs = new();

            long currentToLoad = 0;
            long downloadSize = 0;

            downloadSize = await GetDownloadSize(imageKeys, ct);
            if (downloadSize > 0)
            {
                currentToLoad += downloadSize;
                images = await Preload<Sprite>(imageKeys,
                    progress => { CountProgress(progress, currentToLoad, totalToLoad); }, ct);
            }

            downloadSize = await GetDownloadSize(soundKeys, ct);
            if (downloadSize > 0)
            {
                currentToLoad += downloadSize;
                sounds = await Preload<AudioClip>(soundKeys,
                    bytes => { CountProgress(bytes, currentToLoad, totalToLoad); }, ct);
            }

            downloadSize = await GetDownloadSize(configKeys, ct);
            if (downloadSize > 0)
            {
                currentToLoad += downloadSize;
                configs = await Preload<PhraseConfig>(configKeys,
                    bytes => { CountProgress(bytes, currentToLoad, totalToLoad); }, ct);
            }

            Merge(Content, images);
            Merge(Content, sounds);
            Merge(Content, configs);
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

            foreach (var key in imageKeys)
            {
                string path = _wordPathHelper.GetTexturePath(key);
            
                if (Content.TryGetValue(path, out var configAsset))
                {
                    if (configAsset != null)
                    {
                        continue;
                    }
                }
            
                DisposableAsset<Sprite> asset = await _assetManager.LoadAssetAsync<Sprite>(path, ct);
                if (asset.Item != null)
                {
                    Content[path] = asset;
                }
            }

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

        public Sprite GetCachedSprite(string path)
        {
            Sprite texture = GetCachedAsset<Sprite>(path);
            
            if (texture == null)
            {
                Debug.LogError($"Texture not found for key: {path}");
                return _assetManager.LoadMissingSprite();
            }
            
            return texture;
        }
        
        private static Sprite CreateSprite(Texture2D texture, float pixelsPerUnit = 100f)
        {
            if (texture == null) return null;
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
        }
        
        public AudioClip GetCachedAudioClip(string name)
        {
            string path = _wordPathHelper.GetSoundPath(name);
            return GetCachedAsset<AudioClip>(path);
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

                Debug.LogError(
                    $"{nameof(GetDownloadSize)} failed to get download size: {getDownloadSizeHandle.OperationException}");
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

        private void CountProgress(float bytes, long currentLoad, long totalToLoad)
        {
            _progress?.Invoke(currentLoad + bytes, totalToLoad);
        }

        private async UniTask<Dictionary<string, IDisposableAsset>> Preload<T>(HashSet<string> keys,
            Action<float> bytes, CancellationToken ct)
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
                        bytes?.Invoke(individualProgress.Sum());
                    }), cancellationToken: ct));
            }

            T[] completedTasks = await UniTask.WhenAll(loadAssetTasks);

            for (int i = 0; i < completedTasks.Length; i++)
            {
                result[keysList[i]] = new DisposableAsset<T>(completedTasks[i], handles[i]);
            }

            return result;
        }

        private void Merge(Dictionary<string, IDisposableAsset> toDictionary,
            Dictionary<string, IDisposableAsset> fromDictionary)
        {
            foreach (var pair in fromDictionary)
            {
                toDictionary.TryAdd(pair.Key, pair.Value);
            }
        }
    }
}
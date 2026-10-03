using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang;
using Chang.Core;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Popup;
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
                disposable.Value?.Dispose();
            }

            Content.Clear();
            Content = null;
        }

        public void ClearCache()
        {
            // no clear cache on pages switch, so don't implement this method
        }

        public bool GetPhrase(string path)
        {
            // get from book/vocabulary
            throw new NotImplementedException();
        }

        /// <param name="progress">downloaded bytes and total bytes to download, not called if everything is cached</param>
        public async UniTask PreloadWordsContentAsync(List<Word> words, Action<float, float> progress,
            CancellationToken ct)
        {
            HashSet<string> imageKeys = words.Select(w => _wordPathHelper.GetTexturePath(w.ImageKey)).ToHashSet();
            HashSet<string> soundKeys = words.Select(w => _wordPathHelper.GetSoundPath(w.SoundKey)).ToHashSet();

            HashSet<string> totalKeys = new();
            totalKeys.UnionWith(imageKeys);
            totalKeys.UnionWith(soundKeys);

            // download bundles first to report real bytes, then the assets load from the cache
            await DownloadDependenciesAsync(totalKeys, progress, ct);

            Dictionary<string, IDisposableAsset> images = await Preload<Sprite>(imageKeys, ct);
            Dictionary<string, IDisposableAsset> sounds = await Preload<AudioClip>(soundKeys, ct);

            Merge(Content, images);
            Merge(Content, sounds);
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
            string path = _wordPathHelper.GetTexturePath(key);
            Sprite sprite = GetCachedAsset<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogError($"Texture not found for key: {path}");
                return _assetManager.LoadMissingSprite();
            }

            return sprite;
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

        public AudioClip GetCachedAudioClip(string key)
        {
            string path = _wordPathHelper.GetSoundPath(key);
            return GetCachedAsset<AudioClip>(path);
        }

        private async UniTask<long> GetDownloadSize(HashSet<string> keys, CancellationToken ct)
        {
            AsyncOperationHandle<long> handle = default;

            try
            {
                handle = Addressables.GetDownloadSizeAsync(keys);
                
                await handle.ToUniTask(cancellationToken: ct);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }

                Debug.LogError(
                    $"{nameof(GetDownloadSize)} failed to get download size: {handle.OperationException}");
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
                handle.Release();
            }
        }

        private async UniTask DownloadDependenciesAsync(HashSet<string> keys, Action<float, float> progress,
            CancellationToken ct)
        {
            long totalBytes = await GetDownloadSize(keys, ct);
            if (totalBytes == 0)
            {
                return;
            }

            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);

            try
            {
                while (!handle.IsDone)
                {
                    DownloadStatus status = handle.GetDownloadStatus();
                    progress?.Invoke(status.DownloadedBytes, status.TotalBytes);
                    await UniTask.Yield(ct);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    // the assets will try to download again on load
                    Debug.LogError($"{nameof(DownloadDependenciesAsync)} failed: {handle.OperationException}");
                    return;
                }

                progress?.Invoke(totalBytes, totalBytes);
            }
            finally
            {
                handle.Release();
            }
        }

        private async UniTask<Dictionary<string, IDisposableAsset>> Preload<T>(HashSet<string> keys,
            CancellationToken ct)
            where T : UnityEngine.Object
        {
            Debug.Log($"{nameof(Preload)}");
            Dictionary<string, IDisposableAsset> result = new();
            List<string> keysList = keys.ToList();
            List<UniTask<DisposableAsset<T>>> loadAssetTasks = new();

            for (int i = 0; i < keysList.Count; i++)
            {
                loadAssetTasks.Add(_assetManager.LoadAssetAsync<T>(keysList[i], ct));
            }

            DisposableAsset<T>[] loadedAssets = await UniTask.WhenAll(loadAssetTasks);

            for (int i = 0; i < loadedAssets.Length; i++)
            {
                result[keysList[i]] = loadedAssets[i];
            }

            return result;
        }

        private void Merge(Dictionary<string, IDisposableAsset> toDictionary,
            Dictionary<string, IDisposableAsset> fromDictionary)
        {
            foreach (KeyValuePair<string, IDisposableAsset> pair in fromDictionary)
            {
                toDictionary.TryAdd(pair.Key, pair.Value);
            }
        }
    }
}
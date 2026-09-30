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

        public async UniTask PreloadWordsContentAsync(List<Word> words, Action<float, float> progress,
            CancellationToken ct)
        {
            _progress = progress;

            HashSet<string> imageKeys = words.Select(w => _wordPathHelper.GetTexturePath(w.ImageKey)).ToHashSet();
            HashSet<string> soundKeys = words.Select(w => _wordPathHelper.GetSoundPath(w.SoundKey)).ToHashSet();

            HashSet<string> totalKeys = new();
            totalKeys.UnionWith(imageKeys);
            totalKeys.UnionWith(soundKeys);

            long totalToLoad = await GetDownloadSize(totalKeys, ct);

            Dictionary<string, IDisposableAsset> images = new();
            Dictionary<string, IDisposableAsset> sounds = new();

            long currentToLoad = 0;
            long downloadSize = 0;
            downloadSize = await GetDownloadSize(imageKeys, ct);
            currentToLoad += downloadSize;
            images = await Preload<Sprite>(imageKeys,
                progress => { CountProgress(progress, currentToLoad, totalToLoad); }, ct);
           
            downloadSize = await GetDownloadSize(soundKeys, ct);
           
            currentToLoad += downloadSize;
            sounds = await Preload<AudioClip>(soundKeys,
                bytes => { CountProgress(bytes, currentToLoad, totalToLoad); }, ct);

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

        private void CountProgress(float bytes, long currentLoad, long totalToLoad)
        {
            _progress?.Invoke(currentLoad + bytes, totalToLoad);
        }

        private async UniTask<Dictionary<string, IDisposableAsset>> Preload<T>(HashSet<string> keys,
            Action<float> bytes, CancellationToken ct)
            where T : UnityEngine.Object
        {
            Debug.Log($"{nameof(Preload)}");
            Dictionary<string, IDisposableAsset> result = new();
            List<string> keysList = keys.ToList();
            List<UniTask<DisposableAsset<T>>> loadAssetTasks = new();

            float[] individualProgress = new float[keysList.Count];

            for (int i = 0; i < keysList.Count; i++)
            {
                string key = keysList[i];
                int index = i;
                loadAssetTasks.Add(_assetManager.LoadAssetAsync<T>(key, ct,
                    Progress.Create<float>(p =>
                    {
                        individualProgress[index] = p;
                        bytes?.Invoke(individualProgress.Sum());
                    })));
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
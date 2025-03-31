using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Resources
{
    /// <summary>
    /// Provides assets from cached addressables.
    /// </summary>
    public class AddressablesAssetManager : IResourcesManager
    {
        private bool _isDisposed;

        private List<IDisposable> _disposables = new();

        private Sprite _missingSprite;

        // private WordsProvider _wordsProvider;
        // private SoundWordsProvider _soundWordsProvider; 

        public AddressablesAssetManager()
        {
            // _wordsProvider = new _wordsProvider();
            // _soundWordsProvider = new _soundWordsProvider();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }

            _disposables.Clear();
        }

        public bool IsAssetExists(string key)
        {
            bool isExists = false;

            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations) && locations.Count > 0)
                {
                    isExists = true;
                    break;
                }
            }

            Debug.Log($"{nameof(IsAssetExists)} {isExists} for key '{key}'");
            return isExists;
        }

        public T LoadAssetSync<T>(AssetReference key) where T : Object
        {
            throw new NotImplementedException();
        }

        public T LoadAssetSync<T>(string key) where T : Object
        {
            throw new NotImplementedException();
        }

        // public UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : Object
        // {
        //     // todo chang disposable asset? or handle and then dissposable asset is not required?
        //     throw new NotImplementedException();
        // }

        // todo chang implement with progress? or cant it be moved into downloader class?
        //public async UniTask<DisposableAsset<T>> LoadAssetAsync<T>(string key, IProgress<float> progress, CancellationToken ct) where T : Object
        public async UniTask<DisposableAsset<T>> LoadAssetAsync<T>(string key, CancellationToken ct) where T : Object
        {
            // InitializationGuard();
            bool isKeyNotFound = false;
            AsyncOperationHandle<T> handle = default;
            T result = null;

            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
                result = await handle.WithCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                handle.SafeRelease();
                return DisposableAsset<T>.Empty();
            }
            catch (InvalidKeyException ex)
            {
                Debug.Log($"<color=Yellow>Warning</color> not found asset with key '{key}': {ex.Message}");
                isKeyNotFound = true;
                handle.SafeRelease();
            }

            if (isKeyNotFound)
            {
                try
                {
                    Debug.Log($"Repeat load asset from cached path '{key}':");
                    handle = Addressables.LoadAssetAsync<T>(key);
                    result = await handle.WithCancellation(ct);
                }
                catch (OperationCanceledException)
                {
                    handle.SafeRelease();
                    return DisposableAsset<T>.Empty();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error loading asset with cached path '{key}': {ex.Message}");
                    handle.SafeRelease();
                    return DisposableAsset<T>.Empty();
                }
            }

            if (handle.Result == null)
            {
                Debug.LogError($"missing asset with key '{key}'");
                handle.SafeRelease();
                return DisposableAsset<T>.Empty();
            }

            return new DisposableAsset<T>(result, handle);
        }

        public Sprite LoadMissingSprite()
        {
            if (_missingSprite == null)
            {
                var holder = UnityEngine.Resources.Load<MissingSpriteLinkHolder>(AssetPaths.Resources.MissingSpriteLinkHolder);
                _missingSprite = holder?.MissingSprite;
                if (_missingSprite == null)
                {
                    Debug.LogError("Can't load missing sprite from " + AssetPaths.Resources.MissingSpriteLinkHolder);
                }
            }

            return _missingSprite;
        }
    }
}
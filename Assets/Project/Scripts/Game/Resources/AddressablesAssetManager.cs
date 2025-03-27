using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        public UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : Object
        {
            throw new NotImplementedException();
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
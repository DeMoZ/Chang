using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Resources
{
    public class SimpleResourceManager : IResourcesManager
    {

        public async UniTask InitializeAsync()
        {
            await Addressables.InitializeAsync();
        }

        public bool IsAssetExists(AssetReference key)
        {
            if (key == null)
            {
                Debug.LogError($"[{nameof(SimpleResourceManager)}] {nameof(IsAssetExists)}() asset reference is null.");
                return false;
            }

            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key.RuntimeKey, typeof(object), out IList<IResourceLocation> locations) && locations.Count > 0)
                {
                    Debug.Log($"{nameof(IsAssetExists)}() asset exists: true, key: {key}");
                    return true;
                }
            }

            Debug.Log($"{nameof(IsAssetExists)}() asset exists: false, key: {key}");
            return false;
        }

        public bool IsAssetExists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError($"{nameof(IsAssetExists)}() asset key is null or empty.");
                return false;
            }

            bool isExists = false;

            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations) && locations.Count > 0)
                {
                    isExists = true;
                    break;
                }
            }

            Debug.Log($"{nameof(IsAssetExists)}() asset exists {isExists} : {key}");
            return isExists;
        }

        public T LoadAssetSync<T>(string key) where T : Object
        {
            if (!IsAssetExists(key))
            {
                Debug.LogWarning($"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Asset with key '{key}' does not exist.");
                return null;
            }

            AsyncOperationHandle<T> handle = default;
            T result = null;

            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
                result = handle.WaitForCompletion();

                if (result == null)
                {
                    Debug.LogError($"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Failed to load asset with key '{key}'.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Exception occurred while loading asset with key '{key}', ex: {ex}");
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            return result;
        }

        public T LoadAssetSync<T>(AssetReference key) where T : Object
        {
            if (!IsAssetExists(key))
            {
                Debug.LogWarning($"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): AssetReference '{key}' does not exist.");
                return null;
            }

            AsyncOperationHandle<T> handle = default;
            T result = null;

            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
                result = handle.WaitForCompletion();

                if (result == null)
                {
                    Debug.LogError($"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Failed to load asset with AssetReference '{key}'.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Exception occurred while loading asset with AssetReference '{key}', ex: {ex}");
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            return result;
        }

        public void Dispose()
        {
        }

        public async UniTask<DisposableAsset<T>> LoadAssetAsync<T>(string key, CancellationToken token) where T : Object
        {
            await UniTask.Yield(token);
            throw new NotImplementedException();
        }
       /*
        public async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken token) where T : Object
        {
            while (!_isInitialized)
            {
                await UniTask.DelayFrame(1, cancellationToken: token);
                if (token.IsCancellationRequested)
                    break;
            }

            if (!IsAssetExists(key))
            {
                Debug.LogWarning(
                    $"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): AssetReference '{key}' does not exist.");
                return default;
            }

            AsyncOperationHandle<T> handle = default;
            T result = default;

            try
            {
                // todo chang implement with cancellation token
                handle = Addressables.LoadAssetAsync<T>(key);
                result = await handle;

                if (result == null)
                {
                    Debug.LogError($"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Failed to load asset with AssetReference '{key}'.");
                }

                handle = Addressables.LoadAssetAsync<T>(key);

                while (!handle.IsDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    await UniTask.Yield();
                }

                result = handle.Result;

                if (result == null)
                {
                    Debug.LogError($"Failed to load asset with key '{key}'.");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Operation was canceled for asset with key '{key}'.");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"{nameof(LoadAssetSync)}<{typeof(T).Name}>(): Exception occurred while loading asset with AssetReference '{key}', ex: {ex}");
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            return result;
        }
        */
    }
}
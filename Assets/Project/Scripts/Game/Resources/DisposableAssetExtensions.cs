using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
// using UniRx;
// using UniRx.Triggers;
using Object = UnityEngine.Object;

namespace Chang.Resources
{
    // todo Cahng UniRx is required
    public static class DisposableAssetExtensions
    {
        /*
        public static GameObject InstantiateAndDisposeOnDestroy(this DisposableAsset<GameObject> asset)
        {
            GameObject instance = Object.Instantiate(asset.Item);
            instance.AttachAndDisposeOnDestroy(asset);
            return instance;
        }

        public static GameObject InstantiateAndDisposeOnDestroy(this DisposableAsset<GameObject> asset,
            Transform parent)
        {
            GameObject instance = Object.Instantiate(asset.Item, parent);
            instance.AttachAndDisposeOnDestroy(asset);
            return instance;
        }

        public static GameObject InstantiateAndDisposeOnDestroy(this DisposableAsset<GameObject> asset,
            Transform parent,
            bool worldPositionStays)
        {
            GameObject instance = Object.Instantiate(asset.Item, parent, worldPositionStays);
            instance.AttachAndDisposeOnDestroy(asset);
            return instance;
        }

        public static GameObject InstantiateAndDisposeOnDestroy(this DisposableAsset<GameObject> asset,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(asset.Item, position, rotation);
            instance.AttachAndDisposeOnDestroy(asset);
            return instance;
        }

        public static GameObject InstantiateAndDisposeOnDestroy(this DisposableAsset<GameObject> asset,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            GameObject instance = Object.Instantiate(asset.Item, position, rotation, parent);
            instance.AttachAndDisposeOnDestroy(asset);
            return instance;
        }

        public static void AttachAndDisposeOnDestroy(this GameObject gameObject, IDisposable asset, Action continuation = null)
        {
            DisposeByObservable(asset, gameObject.OnDestroyAsObservable(), gameObject, continuation);
        }

        public static void AttachAndDisposeOnDestroy(this Component component, IDisposable asset, Action continuation = null)
        {
            DisposeByObservable(asset, component.OnDestroyAsObservable(), component, continuation);
        }

        private static void DisposeByObservable(IDisposable disposable, IObservable<Unit> observable, Object owner, Action continuation = null)
        {
            CheckAssetState(disposable, out bool isNull, out bool isDisposed, out string assetName);
            Debug.Log($"Scheduling dispose of '{assetName}', isNull={isNull}, isDisposed={isDisposed} when destroyed '{owner}'");

            if (isNull)
            {
                Debug.LogError("Null asset can't be disposed");
                ExecuteContinuation(continuation, assetName);
            }
            else if (isDisposed)
            {
                Debug.LogError($"'{assetName}' is already disposed");
                ExecuteContinuation(continuation, assetName);
            }
            else
            {
                observable.Subscribe(_ =>
                    {
                        CheckAssetState(disposable, out bool isNullNow, out bool isDisposedNow, out string _);
                        Debug.Log($"Disposing '{assetName}', isNull={isNullNow}, isDisposed={isDisposedNow}");

                        if (isNullNow || isDisposedNow)
                        {
                            Debug.LogError($"Asset '{assetName}' is already disposed. Releasing an addressable handle " +
                                           "more times than necessary may result in unexpected unload of the asset.");
                        }
                        else
                        {
                            disposable.Dispose();
                            ExecuteContinuation(continuation, assetName);
                        }
                    },
                    onError: ex => { Debug.LogError($"Failed to dispose asset '{assetName}': {ex}"); });
            }
        }

        private static void CheckAssetState(IDisposable disposable, out bool isNull, out bool isDisposed, out string name)
        {
            isNull = disposable == null;
            isDisposed = false;
            name = "null";

            if (isNull)
            {
                return;
            }

            if (disposable is IDisposableAsset asset)
            {
                isDisposed = asset.IsDisposed;
#if DEVELOPMENT
                name = asset.ToString();
#else
                name = "<use DEVELOPMENT to show names>";
#endif
            }
            else if (disposable is CompositeDisposable compositeDisposable)
            {
                isDisposed = compositeDisposable.IsDisposed;
                name = nameof(CompositeDisposable);
            }
        }

        private static void ExecuteContinuation(Action continuation, string assetName)
        {
            try
            {
                continuation?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to execute continuation after disposing of '{assetName}': {ex}");
            }
        }
        */
    }
}
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Chang.Resources
{
    public class DisposableAsset<T> : IDisposableAsset where T : class
        {
            private readonly AsyncOperationHandle<T> _handle;

            public T Item { get; protected set; }
            public bool IsDisposed { get; private set; }
            protected Action OnDispose { get; set; }

            public void Dispose()
            {
                if (IsDisposed)
                {
                    return;
                }

                Addressables.Release(_handle);
                
                OnDispose?.Invoke();
                OnDispose = null;

                Item = null;
                IsDisposed = true;
            }

            public static DisposableAsset<T> Empty()
            {
                return new DisposableAsset<T>(null);
            }

            public DisposableAsset()
            {
            }

            public DisposableAsset(T item, AsyncOperationHandle<T> handle = default, Action onDispose = null)
            {
                Item = item;
                _handle = handle;
                OnDispose = onDispose;
                IsDisposed = false;
            }

            public override string ToString()
            {
                string type = Item?.GetType().Name ?? "null";
                string name = Item is UnityEngine.Object obj ? obj.name : "n/a";
                return $"{nameof(DisposableAsset<T>)}({type}, name={name})";
            }
        }
}
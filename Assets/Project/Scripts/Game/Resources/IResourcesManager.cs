using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Chang.Resources
{
    public interface IResourcesManager : IDisposable
    {
        bool IsAssetExists(string key);

        T LoadAssetSync<T>(AssetReference key) where T : UnityEngine.Object;
        T LoadAssetSync<T>(string key) where T : UnityEngine.Object;
        UniTask<DisposableAsset<T>> LoadAssetAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object;
    }
}
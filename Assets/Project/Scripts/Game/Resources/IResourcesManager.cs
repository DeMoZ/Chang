using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Chang.Resources
{
    public interface IResourcesManager : IDisposable
    {
        UniTask InitAsync();

        bool IsAssetExists(AssetReference key);
        bool IsAssetExists(string key);

        T LoadAssetSync<T>(AssetReference key) where T : UnityEngine.Object;
        T LoadAssetSync<T>(string key) where T : UnityEngine.Object;

        UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;
    }
}
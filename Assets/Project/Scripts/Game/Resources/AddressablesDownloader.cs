using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Resources
{
    public static class BundleLabels
    {
        [Flags]
        public enum Labels
        {
            Base = 1 << 0,
        }
    }

    /// <summary>
    /// Downloads addressables assets from the server.
    /// </summary>
    public class AddressablesDownloader
    {
        private AsyncLazy _initialization;

        public UniTask PreloadAtGameStartAsync(Action<float> percents, CancellationToken ct)
        {
            return PreloadLabelsAsync(BundleLabels.Labels.Base, percents, ct);
        }

        public UniTask PreloadLabelsAsync(BundleLabels.Labels labels, Action<float> percents, CancellationToken ct)
        {
            var keys = Enum.GetValues(typeof(BundleLabels.Labels))
                .Cast<BundleLabels.Labels>()
                .Where(label => labels.HasFlag(label))
                .Select(label => label.ToString())
                .ToList();

            if (keys.Count == 0)
            {
                throw new ArgumentException("No labels provided for preloading assets.", nameof(labels));
            }

            return PreloadLabelsAsync(keys, percents, ct);
        }

        /// <summary>
        /// Downloads all assets that have any of the labels. Throws if the download failed.
        /// </summary>
        /// <param name="percents">download progress 0..1</param>
        public async UniTask PreloadLabelsAsync(IReadOnlyCollection<string> keys, Action<float> percents, CancellationToken ct)
        {
            await EnsureInitializedAsync();

            long totalDownloadSize = await GetDownloadSizeAsync(keys, ct);
            Debug.Log($"{nameof(PreloadLabelsAsync)} [{string.Join(", ", keys)}], download size: {totalDownloadSize} bytes");

            if (totalDownloadSize == 0)
            {
                percents?.Invoke(1);
                return;
            }

            // Union to match GetDownloadSizeAsync, which sums the bundles of all keys
            var handle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);

            try
            {
                while (!handle.IsDone)
                {
                    percents?.Invoke(handle.GetDownloadStatus().Percent);
                    await UniTask.Yield(ct);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception($"{nameof(PreloadLabelsAsync)} download failed", handle.OperationException);
                }

                percents?.Invoke(1);
            }
            finally
            {
                handle.Release();
            }
        }

        private async UniTask EnsureInitializedAsync()
        {
            _initialization ??= UniTask.Lazy(InitializeAsync);

            try
            {
                await _initialization;
            }
            catch
            {
                // allow retry on the next call
                _initialization = null;
                throw;
            }
        }

        private static async UniTask InitializeAsync()
        {
            AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync(false);

            try
            {
                await handle;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception("Addressables initialization failed", handle.OperationException);
                }
            }
            finally
            {
                handle.Release();
            }
        }

        private static async UniTask<long> GetDownloadSizeAsync(IReadOnlyCollection<string> keys, CancellationToken ct)
        {
            AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(keys);

            try
            {
                await handle.ToUniTask(cancellationToken: ct);

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception("Failed to get download size", handle.OperationException);
                }

                return handle.Result;
            }
            finally
            {
                handle.Release();
            }
        }
    }
}

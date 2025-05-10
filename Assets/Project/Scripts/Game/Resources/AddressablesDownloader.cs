using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Popup;
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
    public class AddressablesDownloader : IDisposable
    {
        private readonly PopupManager _popupManager;

        private bool _isInitialized;

        public AddressablesDownloader(PopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        public async UniTask PreloadAtGameStartAsync(Action<float> percents, CancellationToken ct)
        {
            await PreloadLabelsAsync(BundleLabels.Labels.Base, percents, ct);
        }

        public async UniTask PreloadLabelsAsync(BundleLabels.Labels labels, Action<float> percents, CancellationToken ct)
        {
            Debug.Log($"{nameof(PreloadLabelsAsync)}");

            if (labels == 0)
            {
                Debug.LogError("No labels provided for preloading assets.");
                return;
            }

            var keys = labels.ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            await PreloadLabelsAsync(keys, percents, ct);
        }

        public async UniTask PreloadLabelsAsync(IEnumerable<string> keys, Action<float> percents, CancellationToken ct)
        {
            await InitializationGuard();
            Debug.Log($"{nameof(PreloadLabelsAsync)}");
            AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadLabelsAsync)} failed to get download size: {getDownloadSizeHandle.OperationException}");
                getDownloadSizeHandle.Release();
                return;
            }

            long totalDownloadSize = getDownloadSizeHandle.Result;
            Debug.Log($"Total download size: {totalDownloadSize} bytes");
            getDownloadSizeHandle.Release();

            if (totalDownloadSize == 0)
            {
                Debug.Log("No assets need to be downloaded.");
                return;
            }
            
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Intersection);
            while (!downloadHandle.IsDone && !ct.IsCancellationRequested)
            {
                Debug.Log($"Download progress: {downloadHandle.PercentComplete * 100}%");
                percents?.Invoke(downloadHandle.PercentComplete);
                await UniTask.Yield(ct);
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadLabelsAsync)} download failed: {downloadHandle.OperationException}");
            }
            
            downloadHandle.Release();
        }

        /// <summary>
        /// Preload on enter pages state
        /// </summary>
        public async UniTask PreloadPagesStateAsync(IEnumerable<string> keys, Action<float> percents, CancellationToken ct)
        {
            Debug.Log($"{nameof(PreloadPagesStateAsync)}");
            AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadPagesStateAsync)} failed to get download size: {getDownloadSizeHandle.OperationException}");
                getDownloadSizeHandle.Release();
                return;
            }

            long totalDownloadSize = getDownloadSizeHandle.Result;
            Debug.Log($"Total download size: {totalDownloadSize} bytes");
            getDownloadSizeHandle.Release();

            if (totalDownloadSize == 0)
            {
                Debug.Log("No assets need to be downloaded.");
                return;
            }

            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);
            while (!downloadHandle.IsDone && !ct.IsCancellationRequested)
            {
                percents?.Invoke(downloadHandle.PercentComplete);
                await UniTask.Yield(ct);
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadPagesStateAsync)} download failed: {downloadHandle.OperationException}");
            }

            downloadHandle.Release();
        }

        private async UniTask InitializationGuard()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
        }

        private async UniTask InitializeAsync()
        {
            AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync();
            await handle.Task;

            // todo chang Check handle validity
            if (!handle.IsValid())
            {
                Debug.LogWarning("Handle is invalid, skipping further operations.");
                return;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                string errorMessage = handle.OperationException?.Message;
                Debug.LogError($"Initialization failed: {errorMessage}");

                if (handle.IsValid())
                {
                    handle.Release();
                }

                return;
            }

            _isInitialized = true;
            handle.Release();
        }

        public void Dispose()
        {
        }
    }
}
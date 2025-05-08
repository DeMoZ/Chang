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
        private LoadingUiController _loadingUiController;

        public AddressablesDownloader(PopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        public async UniTask PreloadGameStartAddressables(CancellationToken ct)
        {
            await PreloadAssetAsync(BundleLabels.Labels.Base, ct);
        }

        public async UniTask PreloadAssetAsync(BundleLabels.Labels labels, CancellationToken ct)
        {
            if (labels == 0)
            {
                Debug.LogError("No labels provided for preloading assets.");
                return;
            }

            var keys = labels.ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            await PreloadAssetAsync(keys, ct);
        }

        public async UniTask PreloadAssetAsync(IEnumerable<string> keys, CancellationToken ct)
        {
            await InitializationGuard();

            AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadAssetAsync)} failed to get download size: {getDownloadSizeHandle.OperationException}");
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

            var loadingModel = new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar | LoadingElements.Percent);
            _loadingUiController = _popupManager.ShowLoadingUi(loadingModel);
            _loadingUiController.SetProgress(0);

            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Intersection);
            while (!downloadHandle.IsDone && !ct.IsCancellationRequested)
            {
                Debug.Log($"Download progress: {downloadHandle.PercentComplete * 100}%");
                _loadingUiController.SetProgress(downloadHandle.PercentComplete);
                await UniTask.Yield(ct);
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadAssetAsync)} download failed: {downloadHandle.OperationException}");
            }

            _popupManager.DisposePopup(_loadingUiController);
            _loadingUiController = null;
            downloadHandle.Release();
        }

        /// <summary>
        /// Without download screen 
        /// </summary>
        public async UniTask PreloadAsync(IEnumerable<string> keys, CancellationToken ct)
        {
            AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadAssetAsync)} failed to get download size: {getDownloadSizeHandle.OperationException}");
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
                Debug.Log($"Download progress: {downloadHandle.PercentComplete * 100}%");
                _loadingUiController.SetProgress(downloadHandle.PercentComplete);
                await UniTask.Yield(ct);
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{nameof(PreloadAssetAsync)} download failed: {downloadHandle.OperationException}");
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
            if (_loadingUiController != null)
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }
        }
    }
}
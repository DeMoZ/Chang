using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;

namespace Chang
{
    public class LoadingUiController : IViewController
    {
        public event Action OnDispose;

        private readonly LoadingUiView _view;

        private CancellationTokenSource _simulationCts;
        private bool _isDisposed;

        public LoadingUiModel Model { get; private set; }

        public LoadingUiController(LoadingUiView view, LoadingUiModel model)
        {
            _view = view;

            _view.EnableBlocker(true);
            Update(model);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            StopSimulation();

            Model.Dispose();
            UnityEngine.Object.Destroy(_view.gameObject);

            OnDispose?.Invoke();
            OnDispose = null;
        }

        public void Update(LoadingUiModel model)
        {
            Model = model;

            _view.EnableBackground(model.Elements.HasFlag(LoadingElements.Background));
            _view.EnableProgressSlider(model.Elements.HasFlag(LoadingElements.Bar));
            _view.EnablePercents(model.Elements.HasFlag(LoadingElements.Percent));
            _view.EnableBytes(model.Elements.HasFlag(LoadingElements.Bytes));
            _view.SetBytes(0, 0);
            _view.EnableLoadingAnimation(model.Elements.HasFlag(LoadingElements.Animation));
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        /// <param name="progress">0..1</param>
        public void SetPercents(float progress)
        {
            StopSimulation();
            _view.SetProgress(Mathf.Clamp01(progress));
        }

        /// <param name="current">downloaded bytes</param>
        /// <param name="total">total bytes to download</param>
        public void SetProgress(float current, float total)
        {
            SetPercents(total > 0 ? current / total : 0);
            _view.SetBytes(current, total);
        }

        /// <summary>
        /// Fake progress for operations without real progress reporting. Stops on any Set* call or on Dispose.
        /// </summary>
        public async UniTaskVoid SimulateProgress(float duration, float from = 0, float to = 1)
        {
            StopSimulation();
            _simulationCts = new CancellationTokenSource();
            var ct = _simulationCts.Token;

            _view.SetProgress(from);

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                // cancellation is the expected way to stop the simulation, no exception needed
                if (await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow())
                {
                    return;
                }

                _view.SetProgress(Mathf.Lerp(from, to, elapsed / duration));
            }

            _view.SetProgress(to);
        }

        private void StopSimulation()
        {
            _simulationCts?.Cancel();
            _simulationCts?.Dispose();
            _simulationCts = null;
        }
    }
}

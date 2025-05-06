using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.Events;
using UnityEngine;
using Zenject;

namespace Chang
{
    [Flags]
    public enum LoadingElements
    {
        Background = 1 << 0,
        Bar = 1 << 1,
        Percent = 1 << 2,
        Animation = 1 << 3,
        None = 0,
    }

    public class LoadingUiController : IDisposable
    {
        private readonly LoadingUiView _view;
        
        private CancellationTokenSource _cts;

        private DMZState<float> _progress = new();
        
        [Inject]
        public LoadingUiController(LoadingUiView view)
        {
            _view = view;

            _progress.Subscribe(SetViewProgress);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            _progress.Unsubscribe(SetViewProgress);
            _progress.Dispose();
            _progress = null;
        }

        public void Show(LoadingElements elements)
        {
            _view.EnableBlocker(true);

            _view.EnableBackground((elements & LoadingElements.Background) != 0);
            _view.EnableProgressSlider((elements & LoadingElements.Bar) != 0);
            _view.EnablePercents((elements & LoadingElements.Percent) != 0);
            _view.EnableLoadingAnimation((elements & LoadingElements.Animation) != 0);

            _view.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _view.gameObject.SetActive(false);
        }
        
        public void SetProgress(float progress)
        {
            _cts?.Cancel();
            _progress.Value = progress;
        }
        
        public async UniTask SimulateProgress(float duration, float from = 0, float to = 1, CancellationToken ct = default)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (from < 0)
            {
                from = _progress.Value;
            }

            _progress.Value = from;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested || _cts.Token.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                _progress.Value = Mathf.Lerp(from, to, elapsed / duration);
                await UniTask.Yield(ct);
            }

            _progress.Value = to;
        }
        
        private void SetViewProgress(float value)
        {
            _view.SetProgress(value);
        }
    }
}
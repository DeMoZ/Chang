using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.Events;
using Popup;
using UnityEngine;

namespace Chang
{
    public class LoadingUiController : IViewController
    {
        private readonly LoadingUiView _view;
        private readonly LoadingUiModel _model;

        private CancellationTokenSource _cts;
        private DMZState<float> _progress = new();
        
        public LoadingUiModel Model => _model;
        public Action OnDispose;

        public LoadingUiController(LoadingUiView view, LoadingUiModel model)
        {
            _view = view;
            _model = model;

            _view.EnableBlocker(true);
            Update(_model);

            _progress.Subscribe(SetViewProgress);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _progress.Unsubscribe(SetViewProgress);
            _progress.Dispose();
            _progress = null;

            _model.Dispose();
            UnityEngine.Object.Destroy(_view.gameObject);
            
            OnDispose?.Invoke();
            OnDispose = null;
        }

        public void Update(LoadingUiModel model)
        {
            // _view.name = $"Loading_{string.Join("", GetFlags(model.Elements))}";
            _view.EnableBackground((model.Elements & LoadingElements.Background) == LoadingElements.Background);
            _view.EnableProgressSlider((model.Elements & LoadingElements.Bar) == LoadingElements.Bar);
            _view.EnablePercents((model.Elements & LoadingElements.Percent) == LoadingElements.Percent);
            _view.EnableLoadingAnimation((model.Elements & LoadingElements.Animation) == LoadingElements.Animation);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(true);
        }

        public void SetProgress(float progress)
        {
            _cts?.Cancel();
            _progress.Value = progress;
        }

        public async UniTaskVoid SimulateProgress(float duration, float from = 0, float to = 1)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (from < 0)
            {
                from = _progress.Value;
            }

            _progress.Value = from;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
                    elapsed += Time.deltaTime;
                    _progress.Value = Mathf.Lerp(from, to, elapsed / duration);
                }
                
                _progress.Value = to;
            }
            catch (OperationCanceledException e)
            {
                // todo chang handle?
                Debug.Log($"cancel operation {e}");
            }
        }

        private void SetViewProgress(float value)
        {
            _view.SetProgress(value);
        }

        private static IEnumerable<LoadingElements> GetFlags(LoadingElements elements)
        {
            foreach (LoadingElements value in Enum.GetValues(typeof(LoadingElements)))
            {
                if (value != 0 && elements.HasFlag(value))
                {
                    yield return value;
                }
            }
        }
    }
}
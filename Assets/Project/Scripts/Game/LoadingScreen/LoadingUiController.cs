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
        private DMZState<float> _percents = new();
        private DMZState<string> _bytes = new();

        public LoadingUiModel Model => _model;
        public Action OnDispose;

        public LoadingUiController(LoadingUiView view, LoadingUiModel model)
        {
            _view = view;
            _model = model;

            _view.EnableBlocker(true);
            Update(_model);

            _percents.Subscribe(SetViewProgress);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _percents.Unsubscribe(SetViewProgress);
            _percents.Dispose();
            _percents = null;

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

        public void SetPercents(float progress)
        {
            _cts?.Cancel();
            _percents.Value = progress;
        }

        public void SetBytes(float current, float total)
        {
            _cts?.Cancel();
            _bytes.Value = $"{current}/{total}";
        }

        public void SetPercentsAndBytes(float current, float total)
        {
            _cts?.Cancel();
            SetPercents(current / total);
            SetBytes(current, total);
        }

        public async UniTaskVoid SimulateProgress(float duration, float from = 0, float to = 1)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (from < 0)
            {
                from = _percents.Value;
            }

            _percents.Value = from;
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
                    _percents.Value = Mathf.Lerp(from, to, elapsed / duration);
                }

                _percents.Value = to;
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
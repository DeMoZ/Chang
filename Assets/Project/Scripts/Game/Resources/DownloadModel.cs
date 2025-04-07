using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.Events;
using UnityEngine;

namespace Chang.Resources
{
    public class DownloadModel : IDisposable
    {
        public DMZState<bool> ShowUi = new(false);
        public DMZState<float> Progress = new(0);

        private CancellationTokenSource _cts;

        public async UniTask SimulateProgress(float duration, float from = 0, float to = 1, CancellationToken ct = default)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (from < 0)
            {
                from = Progress.Value;
            }

            Progress.Value = from;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested || _cts.Token.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                Progress.Value = Mathf.Lerp(from, to, elapsed / duration);
                await UniTask.Yield(ct);
            }

            Progress.Value = to;
        }

        public void SetProgress(float progress)
        {
            _cts?.Cancel();
            Progress.Value = progress;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            ShowUi.Dispose();
            Progress.Dispose();
        }
    }
}
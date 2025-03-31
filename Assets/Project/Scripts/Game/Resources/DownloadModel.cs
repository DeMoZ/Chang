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

        public async void SimulateProgress(float duration, float from = 0, float to = 1, CancellationToken ct = default)
        {
            if (from < 0)
            {
                from = Progress.Value;
            }

            Progress.Value = from;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                Progress.Value = Mathf.Lerp(from, to, elapsed / duration);
                await UniTask.Yield(ct);
            }

            Progress.Value = to;
        }
            
        public void Dispose()
        {
            
            ShowUi.Dispose();
            Progress.Dispose();
        }
    }
}
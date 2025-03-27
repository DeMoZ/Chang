using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Chang.Resources
{
    public static class AddressablesInternalExtensions
    {
        public static void SafeRelease<T>(this AsyncOperationHandle<T> handle)
        {
            Addressables.Release(handle);
        }

        /// <summary>
        /// Ensures that no exceptions are thrown except OperationCanceledException
        /// </summary>
        public static async UniTask<T> WaitSafeAsync<T>(this AsyncOperationHandle<T> operationHandle, CancellationToken cancellationToken = default)
        {
            if (operationHandle.IsDone)
            {
                return operationHandle.Result;
            }
            
            var cs = new UniTaskCompletionSource<T>();
            
            var tokenRegistration = cancellationToken.Register(() => cs.TrySetCanceled(cancellationToken));
            
            operationHandle.Completed += handle => cs.TrySetResult(handle.Result);

            try
            {
                return await cs.Task;
            }
            finally
            {
                await tokenRegistration.DisposeAsync();
            }
        }

        // public static async UniTask StartAsync(this IDownloadGroupRequest request, CancellationToken cancellationToken = default)
        // {
        //     if (request.Handle.IsDone)
        //     {
        //         return;
        //     }
        //     
        //     var cs = new UniTaskCompletionSource();
        //     
        //     var tokenRegistration = cancellationToken.Register(request.Stop);
        //     
        //     request.Handle.Completed += handle =>
        //     {
        //         if (handle.IsReleased || cancellationToken.IsCancellationRequested)
        //         {
        //             cs.TrySetCanceled(cancellationToken.IsCancellationRequested ? cancellationToken : default);
        //         }
        //         else
        //         {
        //             cs.TrySetResult();
        //         }
        //     };
        //     
        //     request.Start();
        //
        //     try
        //     {
        //         await cs.Task;
        //     }
        //     finally
        //     {
        //         await tokenRegistration.DisposeAsync();
        //     }
        // }
    }
}
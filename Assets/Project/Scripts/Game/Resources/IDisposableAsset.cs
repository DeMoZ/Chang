using System;

namespace Chang.Resources
{
    public interface IDisposableAsset : IDisposable
    {
        bool IsDisposed { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Chang;
using Cysharp.Threading.Tasks;

namespace Project.Services.PagesContentProvider
{
    public interface IPagesContentProvider : IDisposable
    {
        UniTask PreloadPagesStateAsync(List<ISimpleQuestion> questions, Action<float> percents, CancellationToken ct);
        UniTask GetContentAsync(ISimpleQuestion question, CancellationToken ct);
        
        T GetAsset<T>(string key) where T : class;
        
        /// <summary>
        /// Clears all cached content on Page Exit.
        /// </summary>
        void ClearCache();
    }
}
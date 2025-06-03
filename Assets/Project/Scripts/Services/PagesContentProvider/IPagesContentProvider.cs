using System;
using System.Collections.Generic;
using System.Threading;
using Chang;
using Cysharp.Threading.Tasks;

namespace Project.Services.PagesContentProvider
{
    public interface IPagesContentProvider : IDisposable
    {
        /// <summary>
        /// Preloading all content on Enter Pages state. Content from all pages.
        /// </summary>
        UniTask PreloadPagesStateAsync(List<ISimpleQuestion> questions, Action<float, float> percents, CancellationToken ct);

        /// <summary>
        /// Get content on Enter Every Page. Content for current page.
        /// </summary>
        UniTask GetContentAsync(ISimpleQuestion question, CancellationToken ct);

        /// <summary>
        /// Get an asset from the cache by its key.
        /// </summary>
        T GetCachedAsset<T>(string key) where T : class;

        /// <summary>
        /// Clears all cached content on Page Exit.
        /// </summary>
        void ClearCache();
    }
}
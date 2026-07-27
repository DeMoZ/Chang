using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Services.PagesContentProvider
{
    public interface IPagesContentProvider : IDisposable
    {
        /// <summary>
        /// Preloading all content on Enter Pages state. Content from all pages.
        /// </summary>
        UniTask PreloadWordsContentAsync(List<Word> words, Action<float, float> percents, CancellationToken ct);

        /// <summary>
        /// Cache individual content by path
        /// </summary>
        UniTask CacheContentAsync(string emptyWordPlaceHolderPath, CancellationToken ct);
        
        /// <summary>
        /// Get an asset from the cache by its key.
        /// </summary>
        T GetCachedAsset<T>(string key) where T : class;
        
        Sprite GetCachedSprite(string key);
        AudioClip GetCachedAudioClip(string key);
        
        
        /// <summary>
        /// Clears all cached content on Page Exit.
        /// </summary>
        void ClearCache();

        bool GetPhrase(string path);
    }
}
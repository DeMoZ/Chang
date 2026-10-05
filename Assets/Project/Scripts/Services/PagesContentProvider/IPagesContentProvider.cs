using System;
using System.Collections.Generic;
using System.Threading;
using Chang;
using Chang.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Services.PagesContentProvider
{
    public interface IPagesContentProvider : IDisposable
    {
        /// <summary>
        /// Preloading all content on Enter Pages state. Content from all pages. For words.
        /// Translation sounds (in the native language) are loaded only when they exist in the catalog,
        /// a missing one is reported as an error when it is played.
        /// </summary>
        UniTask PreloadWordsContentAsync(List<Word> words, Languages nativeLanguage, Action<float, float> percents, CancellationToken ct);

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
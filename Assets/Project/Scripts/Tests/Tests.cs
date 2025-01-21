using System;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    /// <summary>
    /// Test loading assets with resource manager
    /// </summary>
    public class Tests : IDisposable
    {
        private readonly IResourcesManager _resourcesManager;

        public Tests(IResourcesManager resourcesManager)
        {
            _resourcesManager = resourcesManager;
        }

        public async UniTask Run()
        {
            await Test_LoadingAssets<TextAsset>("BookJson"); // WebBuild LZ4 compressed json
            // OBSOLETE // await Test_LoadingAssets<LessonConfig>("AdjectiveAdverb 0"); // WebBuild Uncompressed
            // OBSOLETE // await Test_LoadingAssets<QuestionConfig>("Hot"); // WebBuild Uncompressed
            await Test_LoadingAssets<PhraseConfig>("go"); // WebBuild Uncompressed
            
            // todo roman create groups with following assets and test different archiving methods
            // await Test_LoadingAssets<Sprite>("a_sprite"); // WebBuild ?
            // await Test_LoadingAssets<AudioClip>("an_audio"); // WebBuild ?
            // await Test_LoadingAssets<GameObject>("a_prefab"); // WebBuild ?
        }

        private async UniTask Test_LoadingAssets<T>(string key) where T : UnityEngine.Object
        {
            Debug.Log($"Start Test_LoadingAsset: {key}, type: {typeof(T)}");
            await _resourcesManager.LoadAssetAsync<T>(key);
            Debug.Log($"End   Test_LoadingAsset: {key}, type: {typeof(T)}");
        }

        public void Dispose()
        {
        }
    }
}
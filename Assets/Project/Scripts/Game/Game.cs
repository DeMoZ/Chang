using System.Collections;
using System.Collections.Generic;
using Chang.FSM;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class Game : IInitializable
    {
        private readonly IResourcesManager _resourcesManager;
        private readonly ScreenManager _screenManager;
        private readonly GameBus _gameBus;

        private GameFSM _gameFSM;

        [Inject]
        public Game(IResourcesManager resourcesManager, ScreenManager screenManager, GameBus gameBus)
        {
            _resourcesManager = resourcesManager;
            _screenManager = screenManager;
            _gameBus = gameBus;
        }

        public async void Initialize()
        {
            Debug.Log($"{nameof(Game)} Initialize");

            // todo roman show loading screen

            await _resourcesManager.InitAsync();

            _gameFSM = new GameFSM(_gameBus, _resourcesManager);
            _gameFSM.Initialize();

            // var lessonNames = await LoadGameBookConfig();

            // var gameBookController = _screenManager.GetGameBookController();
            // gameBookController.Init(lessonNames, (index) => OnLessonClick(index, lessonNames).Forget());
            // gameBookController.SetViewActive(true);

            // todo roman hide loading screen
        }

        // private async UniTask<List<LessonName>> LoadGameBookConfig()
        // {
        //     var key = "BookJson";
        //     var text = await _resourcesManager.LoadAssetAsync<TextAsset>(key);
        //     return JsonConvert.DeserializeObject<List<LessonName>>(text.text);
        // }

        public void Dispose()
        {
            _gameFSM.Dispose();
        }
    }
}
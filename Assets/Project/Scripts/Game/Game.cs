using System;
using System.Collections.Generic;
using Chang.Resources;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class Game : IInitializable
    {
        private readonly SimpleResourceManager _resourceManager;
        private readonly ScreenManager _screenManager;

        [Inject]
        public Game(SimpleResourceManager resourceManager, ScreenManager screenManager)
        {
            _resourceManager = resourceManager;
            _screenManager = screenManager;
        }

        public async void Initialize()
        {
            Debug.Log($"{nameof(Game)} Initialize");
            // Load config and play
            // load config and populate page with lessons

            // todo roman nake loading screen and load book onfing in background
            // i dont think that i can load book here because it contains too much configs that is going to be linked.
            // So probably in that case:
            // todo roman make the datafile (json/or SO) to represent same config file(book) but with only string data.
            // need to make converter to datafile (json/or SO).
            // may be not all the data but lessons configs file names List<string> so i will know that object to load for every lesson

            // todo roman show loading screen
            await _resourceManager.InitAsync();

            var lessonNames = LoadGameBookConfig();

            var gameBookController = _screenManager.GetGameBookController();
            gameBookController.Init(lessonNames);
            gameBookController.SetViewActive(true);

            // todo roman hide loading screen
        }

        public List<LessonName> LoadGameBookConfig()
        {
            var key = "BookJson";
            var text = _resourceManager.LoadAssetSync<TextAsset>(key);
            return JsonConvert.DeserializeObject<List<LessonName>>(text.text);
        }

        public void Dispose()
        {

        }
    }
}
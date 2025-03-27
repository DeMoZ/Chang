using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using DMZ.FSM;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class PreloadState : ResultStateBase<StateType, GameBus>
    {
        [Inject] private readonly PreloaderController _preloaderController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly AuthorizationService _authorizationService;
        [Inject] private readonly IResourcesManager _resourcesManager;

        public override StateType Type { get; } // => StateType.Preload;

        public PreloadState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();

            StateBodyAsync().Forget();
        }

        public override void Exit()
        {
            _preloaderController.SetViewActive(false);
        }

        private async UniTaskVoid StateBodyAsync()
        {
            // todo chang Show loading UI and some info on that UI
            // todo chang implement loading error catching

            _preloaderController.SetViewActive(true);

            switch (Bus.PreloadFor)
            {
                case PreloadType.Boot:
                    // todo chang this logic supposed to be in main game logic, not in the FSM
                    await LoadGameBookConfigAsync();
                    await _authorizationService.AuthenticateAsync();
                    await _profileService.LoadStoredData();
                    OnStateResult.Invoke(StateType.Lobby);
                    break;
                case PreloadType.LessonData:
                    Debug.LogWarning("Should not be here because it loads in VocabularyState without loading ui");
                    OnStateResult.Invoke(StateType.PlayPages);
                    break;

                default:
                    throw new NotImplementedException($"PreloadType not implemented {Bus.PreloadFor}");
            }
        }

        // todo chang think about to move this logic to the resource manager as the other services to the job inside
        private async UniTask LoadGameBookConfigAsync()
        {
            Debug.Log("LoadGameBookConfigAsync start");
            var key = "BookJson";
            var text = await _resourcesManager.LoadAssetAsync<TextAsset>(key);

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new BookConverter() }
            };

            Bus.SimpleBookData = JsonConvert.DeserializeObject<SimpleBookData>(text.text, settings);
            Bus.SimpleLessons = Bus.SimpleBookData.Sections
                .SelectMany(section => section.Lessons)
                .ToDictionary(lesson => lesson.FileName);
            
            Debug.Log("LoadGameBookConfigAsync end");
        }
    }
}
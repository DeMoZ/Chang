using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType, GameBus>
    {
        public override StateType Type => StateType.Lobby;

        [Inject] private readonly LobbyController _mainUiController;
        [Inject] private readonly AddressablesAssetManager _assetManager;
        [Inject] private readonly ProfileService _profileService;

        private CancellationTokenSource _cts;

        public LobbyState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
        }

        public void Init()
        {
            _mainUiController.Init(OnExitState);
        }

        public override void Enter()
        {
            base.Enter();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            //3 download player profile
            await _profileService.LoadStoredData();
                
            //4 *skip for now download additional addressables related to profile?
                
            // todo chang HideLoadingUi();
            
            Debug.Log("LoadGameBookConfigAsync start");
            var key = "BookJson";
            DisposableAsset<TextAsset> asset = await _assetManager.LoadAssetAsync<TextAsset>(key, _cts.Token);

            if (asset.Item == null)
            {
                Debug.LogError($"[{nameof(LobbyState)}] {nameof(EnterAsync)}() asset is null, key: {key}");
                return;
            }

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new BookConverter() }
            };

            Bus.SimpleBookData = JsonConvert.DeserializeObject<SimpleBookData>(asset.Item.text, settings);
            Bus.SimpleLessons = Bus.SimpleBookData.Sections
                .SelectMany(section => section.Lessons)
                .ToDictionary(lesson => lesson.FileName);

            asset.Dispose();
            
            Debug.Log("LoadGameBookConfigAsync end");
            _mainUiController.Enter();
        }

        public override void Exit()
        {
            _mainUiController.SetViewActive(false);
        }

        private void OnExitState()
        {
            // OnStateResult.Invoke(StateType.Preload); todo Chang so i need to figure out how to switch to a new state on exit from _mainUiController.Init(OnExitState);
        }
    }
}
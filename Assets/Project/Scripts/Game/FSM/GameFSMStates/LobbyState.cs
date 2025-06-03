using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Newtonsoft.Json;
using Popup;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType, GameBus>
    {
        private const string BookKey = "BookJson";

        public override StateType Type => StateType.Lobby;

        [Inject] private readonly LobbyController _lobbyController;
        [Inject] private readonly AddressablesAssetManager _assetManager;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PopupManager _popupManager;

        private LoadingUiController _loadingUiController;
        private CancellationTokenSource _cts;

        public LobbyState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
        }

        public void Init()
        {
            _lobbyController.Init(OnExitState);
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
            _loadingUiController = _popupManager.ShowLoadingUi(
                new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar | LoadingElements.Percent));
            _loadingUiController.SimulateProgress(2f).Forget();

            await _profileService.LoadStoredData(_cts.Token);

            // todo chang download additional addressables related to profile?

            Debug.Log("LoadGameBookConfigAsync start");
            DisposableAsset<TextAsset> asset = await _assetManager.LoadAssetAsync<TextAsset>(BookKey, _cts.Token);

            if (!asset.Item)
            {
                Debug.LogError($"[{nameof(LobbyState)}] {nameof(EnterAsync)}() asset is null, BookKey: {BookKey}");
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

            _loadingUiController.SetPercents(1f);
            if (_loadingUiController != null)
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }

            _lobbyController.Enter();
        }

        public override void Exit()
        {
            _lobbyController.SetViewActive(false);
            _cts?.Cancel();
        }

        private void OnExitState()
        {
            OnStateResult.Invoke(StateType.PlayPages);
        }
    }
}
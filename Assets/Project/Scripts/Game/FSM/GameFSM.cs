using System;
using System.Collections.Generic;
using Chang.Profile;
using Chang.Resources;
using DMZ.FSM;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class GameFSM : FSMResultBase<StateType>
    {
        private readonly GameBus _gameBus;
        private readonly IResourcesManager _resourcesManager;
        private readonly ScreenManager _screenManager;
        private readonly ProfileService _profileService;
        
        protected override StateType _defaultStateType => StateType.Preload;

        public GameFSM(GameBus gameBus, IResourcesManager resourcesManager, ProfileService profileService, Action<StateType> stateChangedCallback = null)
        {
            _gameBus = gameBus;
            _resourcesManager = resourcesManager;
            _profileService = profileService;
        }

        public void Initialize()
        {
            Init();
        }

        protected override void Init()
        {
            _gameBus.PreloadFor = PreloadType.Boot;

            _states = new Dictionary<StateType, IResultState<StateType>>
            {
                { StateType.Preload, new PreloadState(_gameBus, OnStateResult, _resourcesManager, _profileService) },
                { StateType.Lobby, new LobbyState(_gameBus, OnStateResult) },
                { StateType.PlayVocabulary, new VocabularyState(_gameBus, OnStateResult, _resourcesManager, _profileService) },
            };

            _currentState.Subscribe(s => OnStateChanged(s.Type));
            _currentState.Value = _states[_defaultStateType];
            _currentState.Value.Enter();
        }

        /// <summary>
        /// it is just for outside messages
        /// </summary>
        protected override void OnStateChanged(StateType stateType)
        {
            Debug.Log($"New game stateType {stateType}");
        }
    }
}
using System;
using System.Collections.Generic;
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
        private StateType _defaultState => StateType.Preload;

        public GameFSM(GameBus gameBus, IResourcesManager resourcesManager, Action<StateType> stateChangedCallback = null)
        {
            _gameBus = gameBus;
            _resourcesManager = resourcesManager;
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
                { StateType.Preload, new PreloadState( _gameBus, OnStateResult, _resourcesManager) },
                { StateType.Lobby, new LobbyState( _gameBus, OnStateResult) },
                { StateType.PlayVocabulary, new VocabularyState( _gameBus, OnStateResult) },
            };

            _currentState.Subscribe(s => OnStateChanged(s.Type));
            _currentState.Value = _states[_defaultState];
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
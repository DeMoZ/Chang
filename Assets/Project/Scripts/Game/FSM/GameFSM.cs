using System;
using System.Collections.Generic;
using Chang.Resources;
using DMZ.FSM;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class GameFSM : FSMResultBase<StateType>
    {
        private readonly GameModel _gameModel;
        private readonly IResourcesManager _resourcesManager;
        private readonly ScreenManager _screenManager;
        private StateType _defaultState => StateType.Preload;

        public GameFSM(GameModel gameModel, ScreenManager screenManager, IResourcesManager resourcesManager, Action<StateType> stateChangedCallback = null)
        {
            _gameModel = gameModel;
            _resourcesManager = resourcesManager;
            _screenManager = screenManager;
        }

        public void Initialize()
        {
            Init();
        }

        protected override void Init()
        {
            _gameModel.PreloadType = PreloadType.Boot;
            
            _states = new Dictionary<StateType, IResultState<StateType>>
            {
                { StateType.Preload, new PreloadState( _gameModel, OnStateResult, _screenManager, _resourcesManager) },
                { StateType.Lobby, new LobbyState( _gameModel, OnStateResult, _screenManager) },
                // { States.PlayeVocabulary, new PlayeVocabularyState( ) },
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
            // switch (state)
            // {
            //     case States.Idle:
            //         break;
            //     case States.Chase:
            //         break;
            //     case States.Attack:
            //         break;
            //     case States.Return:
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(state), state, null);
            // }
        }
    }
}
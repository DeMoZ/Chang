using System.Collections.Generic;
using DMZ.FSM;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class GameFSM : FSMResultBase<StateType>
    {
        protected override StateType _defaultStateType => StateType.Lobby; 

        private readonly DiContainer _diContainer;
        private readonly GameBus _gameBus;

        [Inject]
        public GameFSM(DiContainer diContainer, GameBus gameBus)
        {
            _diContainer = diContainer;
            _gameBus = gameBus;
        }

        public void Initialize()
        {
            Init();
        }

        protected override void Init()
        {
            // _gameBus.PreloadFor = PreloadType.Boot;

            // var preloaderState = new PreloadState(_gameBus, OnStateResult);
            var lobbyState = new LobbyState(_gameBus, OnStateResult);
            var pagesState = new PagesState(_diContainer, _gameBus, OnStateResult);

            _diContainer.Inject(pagesState);
            // _diContainer.Inject(preloaderState);
            _diContainer.Inject(lobbyState);

            lobbyState.Init();

            _states = new Dictionary<StateType, IResultState<StateType>>
            {
                // { StateType.Preload, preloaderState },
                { StateType.Lobby, lobbyState },
                { StateType.PlayPages, pagesState },
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
        
        // todo Chang need to implement tests
        //await _resourcesManager.Init();
        // #if DEVELOPMENT
        //             var tests = new Tests(_resourcesManager);
        //             await tests.Run();
        //             tests.Dispose();
        // #endif
    }
}
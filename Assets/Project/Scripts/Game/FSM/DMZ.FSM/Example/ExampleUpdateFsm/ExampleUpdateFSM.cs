using System;
using UnityEngine;
using System.Collections.Generic;
using DMZ.FSM.Example;

namespace DMZ.FSM.UpdateExample
{
    public class ExampleUpdateFSM : FSMUpdateBase<States>
    {
        private States _defaultStateType => States.Boot;
        
        private Action<States> _stateChangedCallback;

        public ExampleUpdateFSM(Action<States> stateChangedCallback = null)
        {
            _stateChangedCallback = stateChangedCallback;
            Init();
        }

        protected override void Init()
        {
            _states = new Dictionary<States, IUpdateState<States>>
            {
                { States.Boot, new BootState() },
                { States.Lobby, new LobbyState() },
                { States.Game, new GameState() },
            };

            _currentState.Value = _states[_defaultStateType];
            _currentState.Value.Enter();
        }

        /// <summary>
        /// Can be used for outside messages
        /// </summary>
        protected override void OnStateChanged(States state)
        {
            Debug.Log($"State changed {state}");
        }
    }
}
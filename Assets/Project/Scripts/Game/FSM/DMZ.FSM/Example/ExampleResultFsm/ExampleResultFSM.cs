using System;
using System.Collections.Generic;
using DMZ.FSM.Example;
using UnityEngine;

namespace DMZ.FSM.ResultExample
{
    public class ExampleResultFSM : FSMResultBase<States>
    {
        protected override States _defaultStateType { get; } = States.Boot;

        private readonly Action<States> _stateChangedCallback;

        public ExampleResultFSM(Action<States> stateChangedCallback)
        {
            _stateChangedCallback = stateChangedCallback;
            Init();
        }
        
        protected override void Init()
        {
            _states = new Dictionary<States, IResultState<States>>
            {
                { States.Boot, new BootState(OnStateResult) },
                { States.Lobby, new LobbyState(OnStateResult) },
                { States.Game, new GameState(OnStateResult) }
            };
            
            _currentState.Value = _states[_defaultStateType];
            _currentState.Value.Enter();
        }

        protected override void OnStateChanged(States state)
        {
            Debug.Log($"State changed to: {state}");
        }
    }
}
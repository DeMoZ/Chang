using System;
using UnityEngine;
using DMZ.FSM;
using System.Collections.Generic;

namespace FSM.Example
{
    public enum States
    {
        Preload,
        Lobby,
        Game,
    }

    public abstract class StateBase<T> : IUpdateState<T> where T : Enum
    {
        protected int someData;

        public virtual T Type { get; }
        public Action<T> OnStateResult { get; set; }

        public virtual T Update(float deltaTime = 0) => Type;

        public StateBase(int someData)
        {
            this.someData = someData;
        }

        public virtual void Enter()
        {
            Debug.Log($"{GetType()} Enter");
        }

        public virtual void Exit()
        {
            Debug.Log($"{GetType()} Exit");
        }

        public int GetSomeData()
        {
            return someData;
        }
    }

    public abstract class UpdateStateBase<T> : IUpdateState<T> where T : Enum
    {
        protected int someData;

        public virtual T Type { get; }


        public virtual T Update(float deltaTime = 0) => Type;

        public UpdateStateBase(int someData)
        {
            this.someData = someData;
        }

        public virtual void Enter()
        {
            Debug.Log($"{GetType()} Enter");
        }

        public virtual void Exit()
        {
            Debug.Log($"{GetType()} Exit");
        }

        public int GetSomeData()
        {
            return someData;
        }
    }

    public class PreloadState : UpdateStateBase<States>
    {
        public override States Type { get; } = States.Preload;

        public PreloadState(int someData) : base(someData)
        {
        }

        public override States Update(float deltaTime)
        {
            // some logic to mome to particular state
            if (someData == 1)
                return States.Lobby;

            if (someData == 2)
                return States.Game;

            // or move to default state
            return Type;
        }
    }

    public class ExampleFSM : FSMUpdateBase<States>
    {
        private States _defaultState => States.Preload;
        private int[] data;

        public ExampleFSM(int[] data, Action<States> stateChangedCallback = null)
        {
            this.data = data;

            Init();
        }

        protected override void Init()
        {
            _states = new Dictionary<States, IUpdateState<States>>
            {
                { States.Preload, new PreloadState(1) },
                { States.Lobby, new PreloadState(2) },
                { States.Game, new PreloadState(3) },
            };

            _currentState.Value = _states[_defaultState];
            _currentState.Value.Enter();
        }

        /// <summary>
        /// it is just for outside messages
        /// </summary>
        protected override void OnStateChanged(States state)
        {
            Debug.Log($"State changed {state}");
        }
    }

    // todo roman need example for update fsm
}
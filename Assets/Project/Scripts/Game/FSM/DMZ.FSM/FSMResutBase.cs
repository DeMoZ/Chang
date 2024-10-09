using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DMZ.Events;

namespace DMZ.FSM
{
    public abstract class FSMResultBase<T> : IDisposable where T : Enum
    {
        protected DMZState<IResultState<T>> _currentState = new();
        protected Dictionary<T, IResultState<T>> _states;

        public virtual T DefaultStateType { get; }

        protected abstract void Init();

        public FSMResultBase()
        {
            //_currentState.Subscribe(state => OnStateChanged(state.Type));
        }

        public void OnEnter()
        {
            _currentState.Value = _states[DefaultStateType];
            _currentState.Value.OnStateResult += OnStateResult;

            _currentState.Value.Enter();
        }

        public void OnExit()
        {
            _currentState.Value.OnStateResult -= OnStateResult;

            _currentState.Value.Exit();
        }

        public IResultState<T> GetState => _currentState.Value;

        public void Dispose()
        {
            _currentState.Unsubscribe(state => OnStateChanged(state.Type));
            _currentState?.Dispose();
        }

        /// <summary>
        /// Triggered when current state has result
        /// </summary>
        /// <param name="state"></param>
        protected void OnStateResult(T stateType)
        {
            _currentState.Value.Exit();

            _currentState.Value = _states[stateType];
            _currentState.Value.Enter();
        }

        protected virtual void OnStateChanged(T state)
        {
            // switching logic is where?
        }
    }
}
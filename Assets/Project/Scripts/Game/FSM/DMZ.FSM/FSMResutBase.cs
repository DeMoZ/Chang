using System;
using System.Collections.Generic;
using DMZ.Events;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace DMZ.FSM
{
    public abstract class FSMResultBase<T> : IDisposable where T : Enum
    {
        protected DMZState<IResultState<T>> _currentState = new();
        protected Dictionary<T, IResultState<T>> _states;

        protected abstract T _defaultStateType { get; }

        protected abstract void Init();

        public void Dispose()
        {
            _currentState.Unsubscribe(state => OnStateChanged(state.Type));
            _currentState?.Dispose();
        }

        /// <summary>
        /// Triggered when current state has result
        /// </summary>
        /// <param name="stateType"></param>
        protected void OnStateResult(T stateType)
        {
            _currentState.Value?.Exit();
            _currentState.Value = _states[stateType];
            _currentState.Value.Enter();
        }

        protected virtual void OnStateChanged(T stateType)
        {
            Debug.Log($"New game stateType {stateType}");
        }
    }
}
using System;
using System.Collections.Generic;
using DMZ.Events;

namespace DMZ.FSM
{
    /// <summary>
    /// Finit State Machine with external update loop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FSMUpdateBase<T> : IDisposable where T : Enum
    {
        protected DMZState<IUpdateState<T>> _currentState = new();
        protected Dictionary<T, IUpdateState<T>> _states;

        public virtual T DefaultStateType { get; }

        protected abstract void Init();

        public FSMUpdateBase()
        {
            _currentState.Subscribe(state => OnStateChanged(state.Type));
        }

        public void OnEnter()
        {
            _currentState.Value = _states[DefaultStateType];
            _currentState.Value.Enter();
        }

        public void OnExit()
        {
            _currentState.Value.Exit();
        }

        public IState<T> GetState => _currentState.Value;

        public void Dispose()
        {
            _currentState.Unsubscribe(state => OnStateChanged(state.Type));
            _currentState?.Dispose();
        }

        public T Update(float deltaTime)
        {
            var nextStateType = _currentState.Value.Update(deltaTime);

            if (!nextStateType.Equals(_currentState.Value.Type))
            {
                _currentState.Value.Exit();
                _currentState.Value = _states[nextStateType];
                _currentState.Value.Enter();
            }

            return _currentState.Value.Type;
        }

        protected virtual void OnStateChanged(T state)
        {
        }
    }
}
using System;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace DMZ.FSM
{
    public abstract class ResultStateBase<TState, TBus> : IResultState<TState> where TState : Enum
    {
        protected readonly TBus Bus;

        public Action<TState> OnStateResult { get; set; }

        public virtual TState Type { get; }


        protected ResultStateBase(TBus bus, Action<TState> onStateResult)
        {
            Bus = bus;
            OnStateResult = onStateResult;
        }

        public virtual void Enter()
        {
            Debug.Log($"{GetType()} Enter");
        }

        public virtual void Exit()
        {
            Debug.Log($"{GetType()} Exit");
        }
    }
}
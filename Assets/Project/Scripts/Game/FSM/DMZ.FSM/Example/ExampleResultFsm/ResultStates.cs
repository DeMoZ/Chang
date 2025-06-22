using System;
using System.Collections;
using UnityEngine;
using DMZ.FSM.Example;

namespace DMZ.FSM.ResultExample
{
    public abstract class ResultStateBase<T> : IResultState<T> where T : Enum
    {
        protected MonoBehaviour CoroutineOwner { get; private set; }

        public virtual T Type { get; }

        public Action<T> OnStateResult { get; set; }

        public ResultStateBase(Action<T> onStateResult)
        {
            OnStateResult = onStateResult;
            CoroutineOwner = UnityEngine.Object.FindFirstObjectByType<CoroutineOwner>();
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

    public class BootState : ResultStateBase<States>
    {
        public override States Type { get; } = States.Boot;

        private float _yield = 2f;

        public BootState(Action<States> onStateResult) : base(onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();

            CoroutineOwner.StartCoroutine(YieldAndSwitch());
        }

        private IEnumerator YieldAndSwitch()
        {
            yield return new WaitForSeconds(_yield);
            OnStateResult?.Invoke(States.Lobby);
        }
    }

    public class LobbyState : ResultStateBase<States>
    {
        public override States Type { get; } = States.Lobby;

        private float _yield = 2f;

        public LobbyState(Action<States> onStateResult) : base(onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();

            CoroutineOwner.StartCoroutine(YieldAndSwitch());
        }

        private IEnumerator YieldAndSwitch()
        {
            yield return new WaitForSeconds(_yield);
            OnStateResult?.Invoke(States.Game);
        }
    }

    public class GameState : ResultStateBase<States>
    {
        public override States Type { get; } = States.Game;

        private float _yield = 2f;

        public GameState(Action<States> onStateResult) : base(onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();

            CoroutineOwner.StartCoroutine(YieldAndSwitch());
        }

        private IEnumerator YieldAndSwitch()
        {
            yield return new WaitForSeconds(_yield);
            OnStateResult?.Invoke(States.Boot);
        }
    }
}
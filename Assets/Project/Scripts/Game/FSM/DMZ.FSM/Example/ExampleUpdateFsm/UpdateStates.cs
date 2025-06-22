using System;
using DMZ.FSM.Example;
using UnityEngine;

namespace DMZ.FSM.UpdateExample
{
    public abstract class UpdateStates<T> : IUpdateState<T> where T : Enum
    {
        public virtual T Type { get; }


        public virtual T Update(float deltaTime = 0) => Type;

        public UpdateStates()
        {
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

    public class BootState : UpdateStates<States>
    {
        public override States Type { get; } = States.Boot;

        private float _count = 0f;
        private float _cup = 2f;

        public BootState() : base()
        {
        }

        public override States Update(float deltaTime)
        {
            _count += deltaTime;
            if (_count >= _cup)
            {
                return States.Lobby;
            }

            return Type;
        }

        public override void Enter()
        {
            base.Enter();
            _count = 0f;
        }
    }

    public class LobbyState : UpdateStates<States>
    {
        public override States Type { get; } = States.Lobby;

        private float _count = 0f;
        private float _cup = 2f;

        public LobbyState() : base()
        {
        }

        public override States Update(float deltaTime)
        {
            _count += deltaTime;
            if (_count >= _cup)
            {
                return States.Game;
            }

            return Type;
        }

        public override void Enter()
        {
            base.Enter();
            _count = 0f;
        }
    }

    public class GameState : UpdateStates<States>
    {
        public override States Type { get; } = States.Game;

        private float _count = 0f;
        private float _cup = 2f;

        public GameState() : base()
        {
        }

        public override States Update(float deltaTime)
        {
            _count += deltaTime;
            if (_count >= _cup)
            {
                return States.Boot;
            }

            return Type;
        }

        public override void Enter()
        {
            base.Enter();
            _count = 0f;
        }
    }
}
using System;

namespace DMZ.FSM
{
    /// <summary>
    /// base interface for states. Don't use it directly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IState<T>
    {
        T Type { get; }

        void Enter();
        void Exit();
    }

    public interface IResultState<T> : IState<T>
    {
        Action<T> OnStateResult { get; set; }
    }

    public interface IUpdateState<T> : IState<T>
    {
        T Update(float deltaTime);
    }
}
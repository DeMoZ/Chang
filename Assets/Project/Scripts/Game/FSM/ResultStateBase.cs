using System;
using DMZ.FSM;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public abstract class ResultStateBase<T> : IResultState<T> where T : Enum
    {
        protected readonly GameModel _gameModel;
        protected readonly ScreenManager _screenManager;

        public Action<T> OnStateResult { get; set; }

        public virtual T Type { get; }


        public ResultStateBase(GameModel gameModel, Action<T> onStateResult, ScreenManager screenManager)
        {
            _gameModel = gameModel;
            _screenManager = screenManager;
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
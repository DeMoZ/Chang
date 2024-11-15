using System;
using DMZ.FSM;
using Zenject;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType, GameBus>
    {
        public override StateType Type => StateType.Lobby;

       [Inject] private MainUiController _mainUiController;

        public LobbyState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {

        }

        public override void Enter()
        {
            base.Enter();

            _mainUiController.Init(OnExitState);
            _mainUiController.SetViewActive(true);
        }

        public override void Exit()
        {
            _mainUiController.SetViewActive(false);
        }

        private void OnExitState()
        {
            OnStateResult.Invoke(StateType.Preload);
        }
    }
}
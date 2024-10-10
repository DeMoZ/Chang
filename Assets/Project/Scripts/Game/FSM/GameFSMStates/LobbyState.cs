using System;
using Cysharp.Threading.Tasks;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType, GameBus>
    {
        public override StateType Type => StateType.Lobby;

        private bool _isLoading;
        private GameBookController _gameBookController;

        public LobbyState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // todo roman should be all the lobby view and controller, not only book
            _gameBookController = Bus.ScreenManager.GameBookController;
            _gameBookController.Init(Bus.LessonNames, (index) => OnLessonClick(index).Forget());
            _gameBookController.SetViewActive(true);
        }
        
        public override void Exit()
        {
            _gameBookController.SetViewActive(false);
        }


        private async UniTaskVoid OnLessonClick(int index)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);
            Bus.ClickedLessonIndex = index;
            Bus.PreloadFor = PreloadType.Lesson;

            OnStateResult.Invoke(StateType.Preload);
            _isLoading = false;
        }
    }
}
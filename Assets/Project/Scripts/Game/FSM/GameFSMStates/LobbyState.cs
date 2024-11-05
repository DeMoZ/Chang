using System;
using Cysharp.Threading.Tasks;
using DMZ.FSM;

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
            // todo roman should the lobby views and controller, not only book
            _gameBookController = Bus.ScreenManager.GameBookController;
            _gameBookController.Init(Bus.SimplifiedBookData.Lessons, name => OnLessonClickAsync(name).Forget());
            _gameBookController.SetViewActive(true);
        }

        public override void Exit()
        {
            _gameBookController.SetViewActive(false);
        }


        private async UniTaskVoid OnLessonClickAsync(string name)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            await UniTask.DelayFrame(1);
            Bus.CurrentLesson.SetFileName(name);
            Bus.CurrentLesson.SetSimpQuesitons(Bus.Lessons[name].Questions);

            Bus.PreloadFor = PreloadType.LessonConfig;
            OnStateResult.Invoke(StateType.Preload);
            _isLoading = false;
        }
    }
}
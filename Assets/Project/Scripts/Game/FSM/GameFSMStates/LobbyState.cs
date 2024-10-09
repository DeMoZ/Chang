using System;
using System.Collections.Generic;
using Chang.Resources;
using Cysharp.Threading.Tasks;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType>
    {
        public override StateType Type { get; } = StateType.Lobby;

        private bool _isLoading;
        //private readonly PreloaderController _preloaderController;

        public LobbyState(GameModel gameModel, Action<StateType> onStateResult, ScreenManager screenManager) : base(gameModel, onStateResult, screenManager)
        {
            //_preloaderController = _screenManager.GetPreloaderController();
        }

        public override void Enter()
        {
            base.Enter();

            // todo roman implement
            var gameBookController = _screenManager.GetGameBookController();
            gameBookController.Init(_gameModel.LessonNames, (index) => OnLessonClick(index, _gameModel.LessonNames).Forget());
            gameBookController.SetViewActive(true);
        }

         public async UniTaskVoid OnLessonClick(int index, List<LessonName> lessonNames)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            // todo roman show loading screen

            // load lesson by index

// todo roman Need to show preloading with loading lesson content / or move to preload state again but with loading content switch ????

            var lessonConfig = await _resourcesManager.LoadAssetAsync<LessonConfig>(lessonNames[index].FileName);
            // todo roman PlayLesson by iteration through lessonConfig.Questions


            // todo roman hide loading screen
            _isLoading = false;
        }
    }
}
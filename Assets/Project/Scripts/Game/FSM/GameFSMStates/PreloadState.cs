using System;
using System.Collections.Generic;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using DMZ.FSM;

namespace Chang.FSM
{
    public class PreloadState : ResultStateBase<StateType, GameBus>
    {
        private readonly IResourcesManager _resourcesManager;
        private readonly PreloaderController _preloaderController;

        public override StateType Type => StateType.Preload;

        public PreloadState(GameBus gameBus, Action<StateType> onStateResult, IResourcesManager resourcesManager) : base(gameBus, onStateResult)
        {
            _resourcesManager = resourcesManager;
            _preloaderController = gameBus.ScreenManager.PreloaderController;
        }

        public override void Enter()
        {
            base.Enter();

            StateBody().Forget();
        }

        public override void Exit()
        {
            _preloaderController.SetViewActive(false);
        }

        public async UniTaskVoid StateBody()
        {
            // todo roman Show loading UI and some info on that UI
            // todo roman implement loading error catching

            _preloaderController.SetViewActive(true);

            switch (Bus.PreloadFor)
            {
                case PreloadType.Boot:
                    await LoadGameBookConfig();
                    OnStateResult.Invoke(StateType.Lobby);
                    break;
                // case PreloadType.Lobby:
                //     // _gameModel.Lessons are already in the model, so no need to call PlreloaderType.Lobby
                //     break;
                case PreloadType.Lesson:
                    await LoadLessonContent();
                    OnStateResult.Invoke(StateType.PlayVocabulary);
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
        }

        private async UniTask LoadGameBookConfig()
        {
            var key = "BookJson";
            var text = await _resourcesManager.LoadAssetAsync<TextAsset>(key);
            Bus.LessonNames = JsonConvert.DeserializeObject<List<LessonName>>(text.text);
            await UniTask.Delay(3000); // todo roman temp test
        }

        private async UniTask LoadLessonContent()
        {
            Bus.ClickedLessonConfig = await _resourcesManager.LoadAssetAsync<LessonConfig>(Bus.ClickedLesson);
            await UniTask.Delay(3000); // todo roman temp test
        }
    }
}
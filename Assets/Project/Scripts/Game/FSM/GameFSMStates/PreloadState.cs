using System;
using System.Collections.Generic;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.FSM
{
    public class PreloadState : ResultStateBase<StateType>
    {
        private readonly IResourcesManager _resourcesManager;
        private readonly PreloaderController _preloaderController;

        public override StateType Type { get; } = StateType.Preload;

        public PreloadState(GameModel gameModel, Action<StateType> onStateResult, ScreenManager screenManager, IResourcesManager resourcesManager) : base(gameModel, onStateResult, screenManager)
        {
            _resourcesManager = resourcesManager;
            _preloaderController = _screenManager.GetPreloaderController();
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

            switch (_gameModel.PreloadType)
            {
                case PreloadType.Boot:
                    List<LessonName> lessons = await LoadGameBookConfig();
                    _gameModel.LessonNames = lessons;

                    OnStateResult.Invoke(StateType.Lobby);
                    break;
                // case PreloadType.Lobby:
                //     // _gameModel.Lessons are already in the model, so no need to call PlreloaderType.Lobby
                //     break;
                case PreloadType.Lesson:
                    await LoadLessonContent();
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
        }

        private async UniTask<List<LessonName>> LoadGameBookConfig()
        {
            var key = "BookJson";
            var text = await _resourcesManager.LoadAssetAsync<TextAsset>(key);
            await UniTask.Delay(5000);
            return JsonConvert.DeserializeObject<List<LessonName>>(text.text);
        }

        private async UniTask LoadLessonContent()
        {
            await UniTask.Delay(5000);
            var lessonConfig = await _resourcesManager.LoadAssetAsync<LessonConfig>(_gameModel.LessonNames[_gameModel.NextLessonIndex].FileName);

            OnStateResult.Invoke(StateType.PlayerVocabulary);
        }
    }
}
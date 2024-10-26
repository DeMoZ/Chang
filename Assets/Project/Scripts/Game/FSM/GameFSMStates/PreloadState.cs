using System;
using System.Linq;
using System.Threading.Tasks;
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

            StateBodyAsync().Forget();
        }

        public override void Exit()
        {
            _preloaderController.SetViewActive(false);
        }

        private async UniTaskVoid StateBodyAsync()
        {
            // todo roman Show loading UI and some info on that UI
            // todo roman implement loading error catching

            _preloaderController.SetViewActive(true);

            switch (Bus.PreloadFor)
            {
                case PreloadType.Boot:
                    await LoadGameBookConfigAsync();
                    OnStateResult.Invoke(StateType.Lobby);
                    break;
                // case PreloadType.Lobby:
                //     // _gameModel.Lessons are already in the model, so no need to call PlreloaderType.Lobby
                //     break;
                case PreloadType.LessonConfig:
                    await LoadLessonContentAsync();
                    OnStateResult.Invoke(StateType.PlayVocabulary);
                    break;
                case PreloadType.QuestConfigs:
                    await LoadQuestionsContentAsync();
                    OnStateResult.Invoke(StateType.PlayVocabulary);
                    break;
                
                default:
                    throw new NotImplementedException();
            }
        }

        private async UniTask LoadQuestionsContentAsync()
        {
            throw new NotImplementedException();
        }

        private async UniTask LoadGameBookConfigAsync()
        {
            var key = "BookJson";
            var text = await _resourcesManager.LoadAssetAsync<TextAsset>(key);
            Bus.BookData = JsonConvert.DeserializeObject<BookData>(text.text);
            Bus.Lessons = Bus.BookData.Lessons.ToDictionary(lesson => lesson.FileName);
            await UniTask.Delay(1000); // todo roman temp test
        }

        private async UniTask LoadLessonContentAsync()
        {
            var lesson = Bus.Lessons[Bus.CurrentLesson.FileName];
            await _resourcesManager.LoadAssetAsync<LessonConfig>(lesson.FileName);
        }
    }
}
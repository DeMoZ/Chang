using System;
using System.Linq;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using DMZ.FSM;
using Zenject;

namespace Chang.FSM
{
    public class PreloadState : ResultStateBase<StateType, GameBus>
    {
        [Inject] private readonly PreloaderController _preloaderController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly AuthorizationService _authorizationService;
        [Inject] private readonly IResourcesManager _resourcesManager;

        public override StateType Type => StateType.Preload;

        public PreloadState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {

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
                    await AuthorizeAsync();
                    await LoadProfileAsync();
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
        
        private async UniTask AuthorizeAsync()
        {
            await _authorizationService.AuthenticateAsync();
        }
        
        private async UniTask LoadProfileAsync()
        {
            await _profileService.LoadStoredData();
        }

        private async UniTask LoadGameBookConfigAsync()
        {
            var key = "BookJson";
            var text = await _resourcesManager.LoadAssetAsync<TextAsset>(key);
            Bus.SimpleBookData = JsonConvert.DeserializeObject<SimpleBookData>(text.text);
            Bus.SimpleLessons = Bus.SimpleBookData.Lessons.ToDictionary(lesson => lesson.FileName);
            
            Bus.SimpleQuestions = Bus.SimpleBookData.Lessons
                .SelectMany(lesson => lesson.Questions)
                .GroupBy(question => question.FileName)
                .Select(group => group.First())
                .ToDictionary(question => question.FileName);
        }

        /// <summary>
        /// Loads lesson config but not to be used as it as,
        /// but to also load contained question configs that will be called by the fileNames.
        /// </summary>
        private async UniTask LoadLessonContentAsync()
        {
            var lesson = Bus.SimpleLessons[Bus.CurrentLesson.FileName];
            await _resourcesManager.LoadAssetAsync<LessonConfig>(lesson.FileName);
        }

        /// <summary>
        /// Loads qeustion configs to cash it and load faster during game
        /// </summary>
        private async UniTask LoadQuestionsContentAsync()
        {
            foreach (var question in Bus.CurrentLesson.SimpleQuestions)
            {
                await _resourcesManager.LoadAssetAsync<QuestionConfig>(question.FileName);
            }
        }
    }
}
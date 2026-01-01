using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Resources;
using Chang.Sentences;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Newtonsoft.Json;
using Popup;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType, GameBus>
    {
        private const string VocabularyBookKey = "VocabularyBook";
        private const string SentencesBookKey = "SentencesBook";

        public override StateType Type => StateType.Lobby;
        private Languages Language => _profileService.LearnLanguage;
        private string GetPath(string key) => $"Assets/Project/Resources_Bundled/{Language}/{key}.json";

        [Inject] private readonly LobbyController _lobbyController;
        [Inject] private readonly AddressablesAssetManager _assetManager;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PopupManager _popupManager;

        private LoadingUiController _loadingUiController;
        private CancellationTokenSource _cts;

        public LobbyState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
        }

        public void Init()
        {
            _lobbyController.Init(OnExitState);
        }

        public override void Enter()
        {
            base.Enter();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            _loadingUiController = _popupManager.ShowLoadingUi(
                new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar | LoadingElements.Percent));
            _loadingUiController.SimulateProgress(2f).Forget();

            await _profileService.LoadStoredData(_cts.Token);

            List<UniTask> loads = new()
            {
                LoadVocabularyBookAsync(_cts.Token),
                LoadSentencesBookAsync(_cts.Token)
            };

            await UniTask.WhenAll(loads);

            _loadingUiController.SetPercents(1f);
            if (_loadingUiController != null)
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }

            _lobbyController.Enter();
        }

        private async UniTask LoadVocabularyBookAsync(CancellationToken ct)
        {
            string methodName = nameof(LoadVocabularyBookAsync);
            Debug.Log($"[{methodName}] Start");
            DisposableAsset<TextAsset> asset = await _assetManager.LoadAssetAsync<TextAsset>(GetPath(VocabularyBookKey), ct);

            if (!asset.Item)
            {
                Debug.LogError($"[{nameof(LobbyState)}] [{methodName}] asset is null, BookKey: {GetPath(VocabularyBookKey)}");
                return;
            }

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new Vocabulary.VocabularyBookConverter() }
            };

            Bus.VocabularyBookData = JsonConvert.DeserializeObject<Vocabulary.VocabularyBookData>(asset.Item.text, settings);
            Bus.VocabularyLessons = Bus.VocabularyBookData.Sections
                .SelectMany(section => section.Lessons)
                .ToDictionary(lesson => lesson.FileName);

            asset.Dispose();

            Debug.Log($"[{methodName}] End");
        }

        private async UniTask LoadSentencesBookAsync(CancellationToken ct)
        {
            string methodName = nameof(LoadSentencesBookAsync);
            Debug.Log($"[{methodName}] Start");
            DisposableAsset<TextAsset> asset = await _assetManager.LoadAssetAsync<TextAsset>(GetPath(SentencesBookKey), ct);

            if (!asset.Item)
            {
                Debug.LogError($"[{nameof(LobbyState)}] [{methodName}] asset is null, BookKey: {GetPath(SentencesBookKey)}");
                return;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new Sentences.SentencesBookConverter() }
            };

            Bus.SentencesBookData = JsonConvert.DeserializeObject<Sentences.SentencesBookData>(asset.Item.text, settings);

            // populate fileNames (keys)
            // Thai.Lesson.First.Test.1
            foreach (SectionData sectionData in Bus.SentencesBookData.Sections)
            {
                for (int i = 0; i < sectionData.Lessons.Count; i++)
                {
                    LessonData lesson = sectionData.Lessons[i];
                    lesson.FileName = ProjectSharedLogic.SENTENCE_LESSON_KEY(Language.ToString(), sectionData.Section, i + 1);
                }
            }

            Bus.SentencesLessons = Bus.SentencesBookData.Sections
                .SelectMany(section => section.Lessons)
                .ToDictionary(lesson => lesson.FileName);

            asset.Dispose();

            Debug.Log($"[{methodName}] End");
        }

        public override void Exit()
        {
            _lobbyController.SetViewActive(false);
            _cts?.Cancel();
        }

        private void OnExitState()
        {
            OnStateResult.Invoke(StateType.PlayPages);
        }
    }
}
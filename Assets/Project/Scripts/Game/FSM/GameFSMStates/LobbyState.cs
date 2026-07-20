using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Popup;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class LobbyState : ResultStateBase<StateType, GameBus>
    {
        public override StateType Type => StateType.Lobby;
        private Languages Language => _profileService.LearnLanguage;
        private string VocabularyPath => AssetPaths.Addressables.VocabularyPath(Language);
        private string SentencesPath => AssetPaths.Addressables.SentencesPath(Language);
        private string VocabularyBookPath => AssetPaths.Addressables.VocabularyBookPath(Language);
        private string SentencesBookPath => AssetPaths.Addressables.SentencesBookPath(Language);

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

            HashSet<string> paths = new HashSet<string>
                { VocabularyBookPath, VocabularyPath, SentencesBookPath, SentencesPath };
            long downloadSize = await _assetManager.GetDownloadSize(paths, _cts.Token);

            await _profileService.LoadStoredData(_cts.Token);

            List<UniTask> loads = new()
            {
                LoadVocabularyBookAsync(_cts.Token),
                LoadVocabularyAsync(_cts.Token),
                LoadSentencesBookAsync(_cts.Token),
                LoadSentencesAsync(_cts.Token)
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
            DisposableAsset<GoogleSheets.VocabularyBook> asset = await _assetManager
                .LoadAssetAsync<GoogleSheets.VocabularyBook>(VocabularyBookPath, ct);

            if (!asset.Item)
            {
                Debug.LogError($"[{nameof(LobbyState)}] [{methodName}] asset is null, BookKey: " +
                               $"{VocabularyBookPath}");
                return;
            }

            if (asset.Item.Sections == null || asset.Item.Sections.Count == 0)
            {
                Debug.LogError($"[{nameof(LobbyState)}] [{methodName}] no sections ins asset, BookKey: " +
                               $"{VocabularyBookPath}");
                return;
            }

            // todo chang GoogleSheets.VocabularyBook to Core.VocabularyBook
            VocabularyBook book = GoogleSheetsToCore.GetVocabularyBook(asset.Item);
            Bus.SetVocabularyBook(book);
            asset.Dispose();
            Debug.Log($"[{methodName}] End");
        }

        private async UniTask LoadVocabularyAsync(CancellationToken ct)
        {
            string methodName = nameof(LoadVocabularyAsync);
            Debug.Log($"[{methodName}] Start");

            DisposableAsset<Core.Vocabulary> asset =
                await _assetManager.LoadAssetAsync<Core.Vocabulary>(VocabularyBookPath, ct);

            if (!asset.Item)
            {
                Debug.LogError($"[{nameof(LobbyState)}] [{methodName}] asset is null, BookKey: " +
                               $"{VocabularyBookPath}");
                return;
            }
            Dictionary<string, Word> words = asset.Item.Words.ToDictionary(word => word.Key, word => word);
            Bus.SetWords(words);
            asset.Dispose();
            Debug.Log($"[{methodName}] End");
        }

        private async UniTask LoadSentencesAsync(CancellationToken ct)
        {
            string methodName = nameof(LoadSentencesAsync);
            Debug.Log($"[{methodName}] Start");
            Debug.Log($"[{methodName}] End");
        }

        private async UniTask LoadSentencesBookAsync(CancellationToken ct)
        {
            string methodName = nameof(LoadSentencesBookAsync);
            // Debug.Log($"[{methodName}] Start");
            // DisposableAsset<TextAsset> asset = await _assetManager.LoadAssetAsync<TextAsset>(SentencesBookPath, ct);
            //
            // if (!asset.Item)
            // {
            //     Debug.LogError($"[{nameof(LobbyState)}] [{methodName}] asset is null, BookKey: {SentencesBookPath}");
            //     return;
            // }
            //
            // JsonSerializerSettings settings = new JsonSerializerSettings
            // {
            //     Converters = new List<JsonConverter> { new Sentences.SentencesBookConverter() }
            // };
            //
            // Bus.SentencesBookData = JsonConvert.DeserializeObject<Sentences.SentencesBookData>(asset.Item.text, settings);
            //
            // // populate fileNames (keys)
            // // Thai.Lesson.First.Test.1
            // foreach (SectionData sectionData in Bus.SentencesBookData.Sections)
            // {
            //     for (int i = 0; i < sectionData.Lessons.Count; i++)
            //     {
            //         LessonData lesson = sectionData.Lessons[i];
            //         lesson.FileName = ProjectSharedLogic.SENTENCE_LESSON_KEY(Language.ToString(), sectionData.Section, i + 1);
            //
            //         foreach (var question in lesson.Questions)
            //         {
            //             if (question is ISentenceQuestion sentenceQuestion)
            //             {
            //                 sentenceQuestion.Language = Language;
            //                 sentenceQuestion.Section = sectionData.Section;
            //             }
            //             else
            //             {
            //                 throw new Exception($"[{methodName}] Unknown question type in SentencesBookData: {question.GetType()}");
            //             }
            //         }
            //     }
            // }
            //
            // Bus.SentencesLessons = Bus.SentencesBookData.Sections
            //     .SelectMany(section => section.Lessons)
            //     .ToDictionary(lesson => lesson.FileName);
            //
            // asset.Dispose();

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
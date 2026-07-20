using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Chang.Core;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Popup;
using Project.Services.PagesContentProvider;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class PagesState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        public override StateType Type => StateType.PlayPages;

        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly ScreenManager _screenManager;
        [Inject] private readonly AddressablesDownloader _assetDownloader;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly DiContainer _diContainer;
        [Inject] private readonly PopupManager _popupManager;

        private PagesBus _pagesBus;
        private PagesFSM _pagesFsm;
        private IPagesContentProvider _pagesContentProvider;
        private CancellationTokenSource _cts;

        public PagesState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _pagesFsm.Dispose();
            _pagesBus.Dispose();
        }

        public override void Enter()
        {
            base.Enter();

            _cts = new CancellationTokenSource();
            _pagesContentProvider =
                new PagesContentProvider(_assetManager, _wordPathHelper, _popupManager, _profileService);
            EnterAsync(_cts.Token).Forget();
        }

        private async UniTask EnterAsync(CancellationToken ct)
        {
            var loadingModel = new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar |
                                                  LoadingElements.Percent | LoadingElements.Bytes);
            var loadingUiController = _popupManager.ShowLoadingUi(loadingModel);
            loadingUiController.SetPercentsAndBytes(0, 0);

            await PreloadContentAsync(loadingUiController.SetPercentsAndBytes, ct);

            _screenManager.SetActivePagesContainer(true);

            _gameOverlayController.OnCheck += OnCheck;
            _gameOverlayController.OnContinue += OnContinue;
            _gameOverlayController.OnReturnFromGame += ExitToLobby;
            _gameOverlayController.OnHint += OnHint;

            _gameOverlayController.EnableReturnButton(true);
            _gameOverlayController.EnableHintButton(true);
            
            _pagesBus = new PagesBus
            {
                Lesson = Bus.Lesson,
                GameType = Bus.GameType,
            };

            _pagesFsm = new PagesFSM(_diContainer, _pagesBus, _pagesContentProvider);
            _pagesFsm.Initialize();

            loadingUiController.SetPercents(1);
            _popupManager.DisposePopup(loadingUiController);

            OnContinueAsync(ct).Forget();
        }

        public override void Exit()
        {
            base.Exit();

            Dispose();
            _pagesContentProvider.Dispose();
            _screenManager.SetActivePagesContainer(false);
            _gameOverlayController.OnCheck -= OnCheck;
            _gameOverlayController.OnContinue -= OnContinue;
            _gameOverlayController.OnReturnFromGame -= ExitToLobby;

            _gameOverlayController.OnHint -= OnHint;
            _gameOverlayController.EnableHintButton(false);

            _gameOverlayController.OnExitToLobby();
        }

        private async UniTask PreloadContentAsync(Action<float, float> progress, CancellationToken ct)
        { 
            HashSet<string> wordsKeys = Bus.Lesson.Questions.Select(q => q.GetWordsKeys)
                .SelectMany(hashSet => hashSet)
                .ToHashSet();

            List<Word> words = wordsKeys.Select(key => Bus.Words[key]).ToList();
            await _pagesContentProvider.PreloadWordsContentAsync(words, progress, ct);
        }

        private void ExitToLobby()
        {
            OnStateResult.Invoke(StateType.Lobby);
        }

        private void OnHint()
        {
            Debug.Log($"{nameof(OnHint)}");
            _pagesBus.OnHintUsed.Value = true;
        }

        private void OnCheck()
        {
            OnCheckAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid OnCheckAsync(CancellationToken ct)
        {
            // get current state result, may be show the hint.... (as hint I will show the correct answer)
            Debug.Log($"{nameof(OnCheck)}");
            await UniTask.Yield(ct);

            switch (_pagesFsm.CurrentStateType)
            {
                case ChangTypes.DemonstrationWord:
                case ChangTypes.SelectWord:
                    OnCheckSelectWordAsync(ct).Forget();
                    break;
                case ChangTypes.MatchWords:
                    OnCheckMatchWordsAsync(ct).Forget();
                    break;
                case ChangTypes.SentenceSelectWords:
                    OnCheckSentenceSelectWordsAsync(ct).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTaskVoid OnCheckSelectWordAsync(CancellationToken ct)
        {
            Debug.Log($"{nameof(OnCheckSelectWordAsync)}");

            var isCorrect = _pagesBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _pagesBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");
            var needIncrement = !(bool)_pagesBus.QuestionResult.Info[1];
            _profileService.AddVocabularyLog(_pagesBus.QuestionResult.Key, _pagesBus.QuestionResult.Presentation,
                ChangTypes.SelectWord, isCorrect,
                needIncrement);

            if (!isCorrect)
            {
                _pagesBus.Lesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo();
            info.IsCorrect = isCorrect;
            info.InfoText = (string)_pagesBus.QuestionResult.Info[0];

            _pagesBus.LessonLog.Add(_pagesBus.QuestionResult);

            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
            await _profileService.SaveProgressAsync(ct);
        }

        private async UniTaskVoid OnCheckMatchWordsAsync(CancellationToken ct)
        {
            Debug.Log($"{nameof(OnCheckMatchWordsAsync)}");

            MatchWordsStateResult stateResult = _pagesBus.QuestionResult as MatchWordsStateResult;
            if (stateResult == null)
            {
                throw new NullReferenceException($"{nameof(MatchWordsStateResult)} is null");
            }

            foreach (SelectWordResult result in stateResult.Results)
            {
                _profileService.AddVocabularyLog(result.Key, result.Presentation, ChangTypes.SelectWord,
                    result.IsCorrect, false);
                _pagesBus.LessonLog.Add(result);
            }

            await _profileService.SaveProgressAsync(ct);
            OnContinueAsync(ct).Forget();
        }

        private async UniTaskVoid OnCheckSentenceSelectWordsAsync(CancellationToken ct)
        {
            Debug.Log($"{nameof(OnCheckSentenceSelectWordsAsync)}");

            var isCorrect = _pagesBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _pagesBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");

            SentenceSelectWordStateResult stateResult = _pagesBus.QuestionResult as SentenceSelectWordStateResult;
            if (stateResult == null)
            {
                throw new NullReferenceException($"{nameof(MatchWordsStateResult)} is null");
            }

            bool needIncrement = !(bool)_pagesBus.QuestionResult.Info[1]; // whether hint was used

            if (stateResult.Info[2] is List<SelectWordResult> vocabularyResults)
            {
                foreach (SelectWordResult vocabularyResult in vocabularyResults)
                {
                    _profileService.AddVocabularyLog(vocabularyResult.Key, vocabularyResult.Presentation,
                        ChangTypes.SelectWord, vocabularyResult.IsCorrect, needIncrement);
                    _pagesBus.LessonLog.Add(vocabularyResult);
                }
            }

            _profileService.AddSentenceLog(stateResult.Key, stateResult.Presentation, ChangTypes.SentenceSelectWords,
                stateResult.IsCorrect, needIncrement);

            if (!isCorrect)
            {
                _pagesBus.Lesson.EnqueueCurrentQuestion();
            }

            ContinueButtonInfo info = new()
            {
                IsCorrect = isCorrect,
                InfoText = (string)_pagesBus.QuestionResult.Info[0]
            };

            _pagesBus.LessonLog.Add(stateResult);

            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
            await _profileService.SaveProgressAsync(ct);
        }

        private void OnContinue()
        {
            OnContinueAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid OnContinueAsync(CancellationToken ct)
        {
            await UniTask.Yield(ct);

            if (_pagesFsm.CurrentStateType == ChangTypes.Result)
            {
                ExitToLobby();
                return;
            }

            Lesson lesson = _pagesBus.Lesson;

            // Add generated match words quest at the end of the lesson
            if (lesson.QuestionQueue.Count == 0)
            {
                if (TryGenerateQuestMatchWordsData(lesson, out var matchWordsQuest))
                {
                    // todo chang add matchWordQuest
                    // lessonProvider.AddQuestion(matchWordsQuest);
                    lesson.IsGeneratedMathWordsQuestPlayed = true;
                }
            }

            // If the lesson has finished
            if (lesson.QuestionQueue.Count == 0)
            {
                SwitchState(ChangTypes.Result);
                return;
            }

            IQuestion nextQuestion = lesson.PeekNextQuestion();
            ChangTypes nextQuestionType = nextQuestion.Type;

            // If demonstration word is required
            if (nextQuestion.Type != ChangTypes.DemonstrationWord)
            {
                HashSet<string> keys = nextQuestion.GetNeedDemonstrationKeys;

                foreach (var fileName in keys)
                {
                    // todo chang add demonstratioin
                    // if (IsNeedDemonstration(fileName))
                    // {
                    //     var demonstration = new Vocabulary.QuestDemonstrationWord
                    //     {
                    //         CorrectWordFileName = fileName
                    //     };
                    //     lesson.InsertNextQuest(demonstration);
                    //     nextQuestionType = ChangTypes.DemonstrationWord;
                    //     break;
                    // }
                }
            }

            lesson.DequeueAndSetSipmlQuestion();
            SwitchState(nextQuestionType);
        }

        private void SwitchState(ChangTypes questionType)
        {
            _pagesFsm.SwitchState(questionType);
            _pagesBus.OnHintUsed.SetSilent(false);
        }

        private bool TryGenerateQuestMatchWordsData(Lesson lesson, out QuestMatchWords questMatchWords)
        {
            questMatchWords = new QuestMatchWords();
            HashSet<string> matchWords = new();

            if (lesson.IsGeneratedMathWordsQuestPlayed)
            {
                return false;
            }

            /*
            HashSet<string> selectWordQuests = lessonProvider.Keys.ToHashSet();
            matchWords.AddRange(selectWordQuests);

            if (matchWords.Count < 2)
            {
                string lessonPath = string.Join("/", new List<string>{lessonProvider.Language.ToString(), "Vocabulary", lessonProvider.Section});
                Debug.LogWarning($"matchWords not generated for lesson : {lessonPath}, count select words {matchWords.Count}");
                return false;
            }

            matchWords = _pagesBus.GameType == GameType.Learn
                ? matchWords.Take(ProjectConstants.MAX_WORDS_IN_LEARN_MATCH_WORD_PAGE).ToHashSet()
                : matchWords.Take(ProjectConstants.MAX_WORDS_IN_REPEAT_MATCHT_WORDS_PAGE).ToHashSet();

            matchWords.Shuffle();
            questMatchWords.MatchWordsKeys = matchWords;
*/
            return true;
        }

        private bool IsNeedDemonstration(string fileName)
        {
            bool logExists = _profileService.TryGetVocabularyLog(fileName, out var questLog);

            if (!logExists)
            {
                Debug.Log($"Demonstration required. No log for: {fileName}");
                return true;
            }

            bool isSmallMark = questLog.Mark < 1;

            if (isSmallMark)
            {
                Debug.Log($"Demonstration required. Mark: {questLog.Mark} for: {fileName}");
            }

            return isSmallMark;
        }
    }
}
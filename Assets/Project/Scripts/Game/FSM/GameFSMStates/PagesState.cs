using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Sirenix.Utilities;
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
        [Inject] private readonly DownloadModel _downloadModel;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly DiContainer _diContainer;

        private PagesBus _pagesBus;
        private PagesFSM _pagesFsm;
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

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            _downloadModel.ShowUi.Value = true;
            _downloadModel.SetProgress(0);

            await PreloadContentAsync();

            _screenManager.SetActivePagesContainer(true);

            _gameOverlayController.OnCheck += OnCheck;
            _gameOverlayController.OnContinue += OnContinue;
            _gameOverlayController.OnReturnFromGame += ExitToLobby;
            _gameOverlayController.OnHint += OnHint;

            _gameOverlayController.EnableReturnButton(true);
            _gameOverlayController.EnableHintButton(true);

            _pagesBus = new PagesBus
            {
                CurrentLesson = Bus.CurrentLesson,
                GameType = Bus.GameType,
            };

            _pagesFsm = new PagesFSM(_diContainer, _pagesBus);
            _pagesFsm.Initialize();

            await OnContinueAsync();

            _downloadModel.SetProgress(100);
            _downloadModel.ShowUi.Value = false;
        }

        public override void Exit()
        {
            base.Exit();

            Dispose();
            _screenManager.SetActivePagesContainer(false);
            _gameOverlayController.OnCheck -= OnCheck;
            _gameOverlayController.OnContinue -= OnContinue;
            _gameOverlayController.OnReturnFromGame -= ExitToLobby;

            _gameOverlayController.OnHint -= OnHint;
            _gameOverlayController.EnableHintButton(false);

            _gameOverlayController.OnExitToLobby();
        }

        private async UniTask PreloadContentAsync()
        {
            HashSet<string> keys = new();
            foreach (ISimpleQuestion quest in Bus.CurrentLesson.SimpleQuestions)
            {
                keys.AddRange(quest.GetConfigKeys().Select(k => _wordPathHelper.GetConfigPath(k)));
                keys.AddRange(quest.GetSoundKeys().Select(k => _wordPathHelper.GetSoundPath(k)));
            }

            await _assetDownloader.PreloadAsync(keys, _cts.Token);
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
            OnCheckAsync().Forget();
        }
        
        private async UniTask OnCheckAsync()
        {
            // get current state result, may be show the hint.... (as hint I will show the correct answer)
            Debug.Log($"{nameof(OnCheck)}");

            switch (_pagesFsm.CurrentStateType)
            {
                case QuestionType.DemonstrationWord:
                case QuestionType.SelectWord:
                    await OnCheckSelectWordAsync();
                    break;
                case QuestionType.MatchWords:
                    await OnCheckMatchWordsAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"simple question not handled {_pagesFsm.CurrentStateType}");
            }
        }

        private async UniTask OnCheckSelectWordAsync()
        {
            Debug.Log($"{nameof(OnCheckSelectWordAsync)}");

            var isCorrect = _pagesBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _pagesBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");
            var needIncrement = !(bool)_pagesBus.QuestionResult.Info[1];
            _profileService.AddLog(_pagesBus.QuestionResult.Key, _pagesBus.QuestionResult.Presentation, QuestionType.SelectWord, isCorrect,
                needIncrement);

            if (!isCorrect)
            {
                _pagesBus.CurrentLesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo();
            info.IsCorrect = isCorrect;
            info.InfoText = (string)_pagesBus.QuestionResult.Info[0];

            _pagesBus.LessonLog.Add(_pagesBus.QuestionResult);

            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
            await _profileService.SaveAsync(); // todo chang in case of bug move before _gameOverlayController.EnableContinueButton(true); 
        }

        private async UniTask OnCheckMatchWordsAsync()
        {
            Debug.Log($"{nameof(OnCheckMatchWordsAsync)}");

            var matchWordsStateResult = _pagesBus.QuestionResult as MatchWordsStateResult;
            if (matchWordsStateResult == null)
                throw new NullReferenceException($"{nameof(MatchWordsStateResult)} is null");

            foreach (SelectWordResult result in matchWordsStateResult.Results)
            {
                _profileService.AddLog(result.Key, result.Presentation, QuestionType.SelectWord, result.IsCorrect, false);
                _pagesBus.LessonLog.Add(result);
            }

            await _profileService.SaveAsync();
            await OnContinueAsync();
        }

        private void OnContinue()
        {
            OnContinueAsync().Forget();
        }

        private async UniTask OnContinueAsync()
        {
            if (_pagesFsm.CurrentStateType == QuestionType.Result)
            {
                ExitToLobby();
                return;
            }

            Lesson lesson = _pagesBus.CurrentLesson;

            // Add generated match words quest at the end of the lesson
            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                if (TryGenerateQuestMatchWordsData(lesson, out var matchWordsQuest))
                {
                    lesson.AddSimpleQuestion(matchWordsQuest);
                    lesson.IsGeneratedMathWordsQuestPlayed = true;
                }
            }

            // If the lesson has finished
            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                SwitchState(QuestionType.Result);
                return;
            }
            
            ISimpleQuestion nextQuestion = lesson.PeekNextQuestion();
            QuestionType nextQuestionType = nextQuestion.QuestionType;

            // If demonstration word is required
            if (nextQuestion.QuestionType != QuestionType.DemonstrationWord)
            {
                HashSet<string> keys = nextQuestion.GetNeedDemonstrationKeys();

                foreach (var fileName in keys)
                {
                    if (IsNeedDemonstration(fileName))
                    {
                        var demonstration = new SimpleQuestDemonstrationWord
                        {
                            CorrectWordFileName = fileName
                        };
                        lesson.InsertNextQuest(demonstration);
                        nextQuestionType = QuestionType.DemonstrationWord;
                        break;
                    }
                }
            }

            lesson.DequeueAndSetSipmlQuestion();
            SwitchState(nextQuestionType);
        }

        private void SwitchState(QuestionType questionType)
        {
            _pagesFsm.SwitchState(questionType);
            _pagesBus.OnHintUsed.SetSilent(false);
        }

        private bool TryGenerateQuestMatchWordsData(Lesson lesson, out SimpleQuestMatchWords matchWordsQuest)
        {
            matchWordsQuest = new SimpleQuestMatchWords();
            HashSet<string> matchWords = new();

            if (!lesson.GenerateQuestMatchWordsData || lesson.IsGeneratedMathWordsQuestPlayed)
            {
                return false;
            }

            var selectWordQuests = lesson.SimpleQuestions.OfType<SimpleQuestSelectWord>().ToList();
            matchWords.AddRange(selectWordQuests.Select(q => q.CorrectWordFileName));
            matchWords.AddRange(selectWordQuests.SelectMany(q => q.MixWordsFileNames));

            if (matchWords.Count < 2)
            {
                Debug.LogWarning($"matchWords not generated for lesson FileName: {lesson.FileName}, count select words {matchWords.Count}");
                return false;
            }

            matchWords = _pagesBus.GameType == GameType.Learn
                ? matchWords.Take(ProjectConstants.MAX_WORDS_IN_LEARN_MATCH_WORD_PAGE).ToHashSet()
                : matchWords.Take(ProjectConstants.MAX_WORDS_IN_REPEAT_MATCHT_WORDS_PAGE).ToHashSet();

            matchWords.Shuffle();
            matchWordsQuest.MatchWordsFileNames = matchWords.ToList();

            return true;
        }

        private bool IsNeedDemonstration(string fileName)
        {
            bool logExists = _profileService.TryGetLog(fileName, out var questLog);

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

public struct ContinueButtonInfo
{
    public bool IsCorrect;
    public string InfoText;
}
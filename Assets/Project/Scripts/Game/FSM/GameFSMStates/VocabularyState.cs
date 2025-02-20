using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Sirenix.Utilities;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class VocabularyState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        public override StateType Type => StateType.PlayVocabulary;

        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly ScreenManager _screenManager;
        [Inject] private readonly IResourcesManager _resourcesManager;

        private readonly DiContainer _diContainer;

        private VocabularyBus _vocabularyBus;
        private VocabularyFSM _vocabularyFSM;

        public VocabularyState(DiContainer diContainer, GameBus gameBus, Action<StateType> onStateResult)
            : base(gameBus, onStateResult)
        {
            _diContainer = diContainer;
        }

        public void Dispose()
        {
            _vocabularyFSM.Dispose();
            _vocabularyBus.Dispose();
        }

        public override void Enter()
        {
            base.Enter();

            _screenManager.SetActivePagesContainer(true);

            _gameOverlayController.OnCheck += OnCheck;
            _gameOverlayController.OnContinue += OnContinue;
            _gameOverlayController.OnReturnFromGame += ExitToLobby;
            _gameOverlayController.OnHint += OnHint;

            _gameOverlayController.EnableReturnButton(true);
            _gameOverlayController.EnableHintButton(true);

            _vocabularyBus = new VocabularyBus
            {
                CurrentLesson = Bus.CurrentLesson,
                GameType = Bus.GameType,
            };

            _vocabularyFSM = new VocabularyFSM(_diContainer, _vocabularyBus);
            _vocabularyFSM.Initialize();
            OnContinue();
        }

        public override void Exit()
        {
            base.Exit();

            _vocabularyBus = null;
            _vocabularyFSM.Dispose();
            _screenManager.SetActivePagesContainer(false);
            _gameOverlayController.OnCheck -= OnCheck;
            _gameOverlayController.OnContinue -= OnContinue;
            _gameOverlayController.OnReturnFromGame -= ExitToLobby;

            _gameOverlayController.OnHint -= OnHint;
            _gameOverlayController.EnableHintButton(false);

            _gameOverlayController.OnExitToLobby();
        }

        private void ExitToLobby()
        {
            // todo remove Temporary solution with exit to lobby, and add game result popup
            OnStateResult.Invoke(StateType.Lobby);
        }

        private void OnHint()
        {
            Debug.Log($"{nameof(OnHint)}");
            _vocabularyBus.OnHintUsed.Value = true;
        }

        private void OnCheck()
        {
            OnCheckAsync().Forget();
        }

        private async UniTask OnCheckAsync()
        {
            // get current state result, may be show the hint.... (as hint I will show the correct answer)
            Debug.Log($"{nameof(OnCheck)}");

            switch (_vocabularyFSM.CurrentStateType)
            {
                case QuestionType.DemonstrationWord:
                case QuestionType.SelectWord:
                    await OnCheckSelectWordAsync();
                    break;
                case QuestionType.MatchWords:
                    await OnCheckMatchWordsAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"simple question not handled {_vocabularyFSM.CurrentStateType}");
            }
        }

        private async UniTask OnCheckSelectWordAsync()
        {
            Debug.Log($"{nameof(OnCheckSelectWordAsync)}");

            var isCorrect = _vocabularyBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _vocabularyBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");
            var needIncrement = !(bool)_vocabularyBus.QuestionResult.Info[1];
            _profileService.AddLog(_vocabularyBus.QuestionResult.Key, _vocabularyBus.QuestionResult.Presentation, QuestionType.SelectWord, isCorrect,
                needIncrement);

            if (!isCorrect)
            {
                _vocabularyBus.CurrentLesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo();
            info.IsCorrect = isCorrect;
            info.InfoText = (string)_vocabularyBus.QuestionResult.Info[0];

            _vocabularyBus.LessonLog.Add(_vocabularyBus.QuestionResult);

            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
            await _profileService.SaveAsync(); // todo chang in case of bug move before _gameOverlayController.EnableContinueButton(true); 
        }

        private async UniTask OnCheckMatchWordsAsync()
        {
            Debug.Log($"{nameof(OnCheckMatchWordsAsync)}");

            var matchWordsStateResult = _vocabularyBus.QuestionResult as MatchWordsStateResult;
            if (matchWordsStateResult == null)
                throw new NullReferenceException($"{nameof(MatchWordsStateResult)} is null");

            foreach (SelectWordResult result in matchWordsStateResult.Results)
            {
                _profileService.AddLog(result.Key, result.Presentation, QuestionType.SelectWord, result.IsCorrect, false);
                _vocabularyBus.LessonLog.Add(result);
            }

            await _profileService.SaveAsync();
            OnContinue();
        }

        // todo chang async
        private async void OnContinue()
        {
            if (_vocabularyFSM.CurrentStateType == QuestionType.Result)
            {
                ExitToLobby();
                return;
            }

            Lesson lesson = _vocabularyBus.CurrentLesson;

            // Add generated match words quest at the end of the lesson
            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                if (TryGenerateQuestMatchWordsData(lesson, out var matchWordsQuest))
                {
                    lesson.AddSimpleQuestion(matchWordsQuest);
                    lesson.IsGeneratedMathWordsQuestPlayed = true;
                }
            }

            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                // the lesson has finished
                // todo chang UML needs to be updated
                SwitchState(QuestionType.Result);
            }
            else
            {
                ISimpleQuestion nextQuestion = lesson.PeekNextQuestion();
                IQuestData questionData = await CreateQuestData(nextQuestion);

                if (nextQuestion.QuestionType == QuestionType.SelectWord)
                {
                    var selectWord = (SimpleQuestSelectWord)nextQuestion;
                    if (IsNeedDemonstration(selectWord.CorrectWordFileName))
                    {
                        var demonstration = new SimpleQuestDemonstrationWord
                        {
                            CorrectWordFileName = selectWord.CorrectWordFileName
                        };

                        lesson.InsertNextQuest(demonstration);
                        var questionSelectWordData = (QuestSelectWordData)questionData;
                        questionData = new QuestDemonstrateWordData(questionSelectWordData.CorrectWord);
                    }
                }
                else if (nextQuestion.QuestionType == QuestionType.MatchWords)
                {
                    var matchWords = (SimpleQuestMatchWords)nextQuestion;
                    foreach (var fileName in matchWords.MatchWordsFileNames)
                    {
                        if (IsNeedDemonstration(fileName))
                        {
                            var demonstration = new SimpleQuestDemonstrationWord
                            {
                                CorrectWordFileName = fileName
                            };
                            lesson.InsertNextQuest(demonstration);

                            var phraseData = await LoadPhraseConfigData(fileName);
                            questionData = new QuestDemonstrateWordData(phraseData);
                            break; // no need to create and load all demonstration screens at once
                        }
                    }
                }

                lesson.DequeueAndSetSipmlQuestion();
                lesson.SetCurrentQuestionData(questionData);
                SwitchState(questionData.QuestionType);
            }
        }

        private void SwitchState(QuestionType questionType)
        {
            _vocabularyFSM.SwitchState(questionType);
            _vocabularyBus.OnHintUsed.SetSilent(false);
        }

        private bool TryGenerateQuestMatchWordsData(Lesson lesson, out SimpleQuestMatchWords matchWordsQuest)
        {
            matchWordsQuest = new SimpleQuestMatchWords();
            HashSet<string> matchWords = new();

            if (!lesson.GenerateQuestMatchWordsData || lesson.IsGeneratedMathWordsQuestPlayed)
            {
                return false;
            }
            // todo chang need to get if this is repetition or learning
            var selectWordQuests = lesson.SimpleQuestions.OfType<SimpleQuestSelectWord>().ToList();
            matchWords.AddRange(selectWordQuests.Select(q => q.CorrectWordFileName));
            matchWords.AddRange(selectWordQuests.SelectMany(q => q.MixWordsFileNames));
            
            if (matchWords.Count < 2)
            {
                Debug.LogWarning($"matchWords not generated for lesson FileName: {lesson.FileName}, count select words {matchWords.Count}");
                return false;
            }

            
            matchWords = _vocabularyBus.GameType == GameType.Learn ? 
                matchWords.Take(ProjectConstants.MAX_WORDS_IN_LEARN_MATCH_WORD_PAGE).ToHashSet() : 
                matchWords.Take(ProjectConstants.MAX_WORDS_IN_REPEAT_MATCHT_WORDS_PAGE).ToHashSet();

            matchWords.Shuffle();
            matchWordsQuest.MatchWordsFileNames = matchWords.ToList();
            
            return true;
        }

        // todo chang implement loading screen
        private async UniTask<QuestDataBase> CreateQuestData(ISimpleQuestion nextQuestion)
        {
            switch (nextQuestion.QuestionType)
            {
                case QuestionType.SelectWord:
                    var selectWord = (SimpleQuestSelectWord)nextQuestion;
                    var selectWordData = new QuestSelectWordData
                    {
                        CorrectWord = await LoadPhraseConfigData(selectWord.CorrectWordFileName),
                        MixWords = new List<PhraseData>()
                    };

                    var mixWordsAmount = _vocabularyBus.GameType == GameType.Learn
                        ? ProjectConstants.MIX_WORDS_AMOUNT_IN_LEARN_SELECT_WORD_PAGE
                        : ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE;
                    
                    var mixWords = selectWord.MixWordsFileNames.Take(mixWordsAmount);
                    
                    foreach (var fileName in mixWords)
                    {
                        var data = await LoadPhraseConfigData(fileName);
                        selectWordData.MixWords.Add(data);
                    }

                    return selectWordData;

                case QuestionType.MatchWords:
                    var matchWords = (SimpleQuestMatchWords)nextQuestion;
                    var matchWordsData = new QuestMatchWordsData(new List<PhraseData>());
                    foreach (var fileName in matchWords.MatchWordsFileNames)
                    {
                        var data = await LoadPhraseConfigData(fileName);
                        matchWordsData.MatchWords.Add(data);
                    }

                    return matchWordsData;

                // case QuestionType.DemonstrationWord:
                //     var demonstration = (SimpleQuestDemonstrationWord)nextQuestion;
                //     var demonstrationWordData = await LoadPhraseConfigData(demonstration.CorrectWordFileName);
                //     return new QuestDemonstrateWordData(demonstrationWordData);

                default:
                    throw new ArgumentOutOfRangeException($"simple question not handled {nextQuestion.QuestionType}");
            }
        }

        private async UniTask<PhraseData> LoadPhraseConfigData(string fileName)
        {
            // todo chang create provider for words and implement call _resourcesManager form it
            var phraseConfigPath = AssetPaths.Addressables.ROOT;
            var path = $"{phraseConfigPath}{fileName}.asset";
            var config = await _resourcesManager.LoadAssetAsync<PhraseConfig>(path);
            return config.PhraseData;
        }

        // private List<FileNameData> GetAssetsNames(ISimpleQuestion nextQuestion)
        // {
        //     List<FileNameData> result = new();
        //     switch (nextQuestion.QuestionType)
        //     {
        //         case QuestionType.SelectWord:
        //             var selectWord = (SimpleQuestSelectWord)nextQuestion;
        //             result.Add(selectWord.CorrectWordFileName);
        //             result.AddRange(selectWord.MixWordsFileNames);
        //             break;
        //
        //         case QuestionType.MatchWords:
        //             var matchWords = (SimpleQuestMatchWords)nextQuestion;
        //             result.AddRange(matchWords.MatchWordsFileNames);
        //             break;
        //
        //         default:
        //             throw new ArgumentOutOfRangeException($"simple question not handled {nextQuestion.QuestionType}");
        //     }
        //
        //     return result;
        // }

        // if no records stored about this question or the question mark is 1 (or 0)
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
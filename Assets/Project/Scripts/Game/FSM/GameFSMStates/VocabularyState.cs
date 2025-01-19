using System;
using System.Collections.Generic;
using Chang.Profile;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
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

            _gameOverlayController.EnableReturnButton(true);

            // todo roman implement phonetic toggle
            //gameOverlayController.OnPhonetic += OnPhonetic;

            _vocabularyBus = new VocabularyBus
            {
                CurrentLesson = Bus.CurrentLesson,
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

            _gameOverlayController.OnExitToLobby();
        }

        private void ExitToLobby()
        {
            // todo remove Temporary solution with exit to lobby, and add game result popup
            OnStateResult.Invoke(StateType.Lobby);
        }

        private async void OnCheck()
        {
            // get current state result, may be show the hint.... (as hint I will show the correct answer)
            Debug.Log($"{nameof(OnCheck)}");

            if (_vocabularyFSM.CurrentStateType == QuestionType.MatchWords)
            {
                OnCheckMatchWords();
                return;
            }

            // if the answer is correct
            var isCorrect = _vocabularyBus.QuestionResult.IsCorrect;
            var isCorrectColor = isCorrect ? "Yellow" : "Red";
            var answer = string.Join(" / ", _vocabularyBus.QuestionResult.Info);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");

            _profileService.AddLog(_vocabularyBus.CurrentLesson.CurrentSimpleQuestion.FileName, new LogUnit(DateTime.UtcNow, isCorrect));

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
            await _profileService.SaveAsync(); // todo roman in case of bug move before _gameOverlayController.EnableContinueButton(true); 
        }

        private async void OnCheckMatchWords()
        {
            Debug.Log($"{nameof(OnCheckMatchWords)}");

            var matchWordsStateResult = _vocabularyBus.QuestionResult as MatchWordsStateResult;
            if (matchWordsStateResult == null)
                throw new NullReferenceException($"{nameof(MatchWordsStateResult)} is null");

            var time = DateTime.UtcNow;
            foreach (SelectWordResult result in matchWordsStateResult.Results)
            {
                _profileService.AddLog(result.Key, new LogUnit(time, result.IsCorrect));
                _vocabularyBus.LessonLog.Add(result);
            }

            await _profileService.SaveAsync();
            OnContinue();
        }

        private async void OnContinue()
        {
            if (_vocabularyFSM.CurrentStateType == QuestionType.Result)
            {
                ExitToLobby();
                return;
            }

            var lesson = _vocabularyBus.CurrentLesson;

            if (lesson.SimpleQuestionQueue.Count == 0)
            {
                // the lesson has finished
                // todo roman UML needs to be updated
                _vocabularyFSM.SwitchState(QuestionType.Result);
            }
            else
            {
                ISimpleQuestion nextQuestion = lesson.PeekNextQuestion();
                IQuestData questionData = await CreateQuestData(nextQuestion);

                if (nextQuestion.QuestionType == QuestionType.SelectWord && IsNeedDemonstration(nextQuestion.FileName))
                {
                    var demonstration = new SimpleQuestDemonstrationWord
                    {
                        FileName = nextQuestion.FileName
                    };

                    lesson.InsertNextQuest(demonstration);
                    var questionSelectWordData = (QuestSelectWordData)questionData;
                    questionData = new QuestDemonstrateWordData(questionSelectWordData.CorrectWord);
                }
                else if (nextQuestion.QuestionType == QuestionType.MatchWords)
                {
                    var matchWords = (SimpleQuestMatchWords)nextQuestion;
                    foreach (var fileName in matchWords.MatchWordsFileNames)
                    {
                        if (IsNeedDemonstration(fileName))
                        {
                            // todo reman implement demonstration for new match words. Then issue with after demonstration.
                            // var demonstration = new SimpleQuestDemonstrationWord
                            // {
                            //     FileName = fileName
                            // };
                            // lesson.InsertNextQuest(demonstration);
                            // var phraseData = await LoadPhraseConfigData(fileName);
                            // questionData = new QuestDemonstrateWordData(phraseData);
                        }
                    }
                }

                lesson.DequeueAndSetSipmQiestion();
                lesson.SetCurrentQuestionData(questionData);
                _vocabularyFSM.SwitchState(questionData.QuestionType);
            }
        }

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

                    foreach (var fileName in selectWord.MixWordsFileNames)
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

                default:
                    throw new ArgumentOutOfRangeException($"simple question not handled {nextQuestion.FileName}, {nextQuestion.QuestionType}");
            }
        }

        private async UniTask<PhraseData> LoadPhraseConfigData(string fileName)
        {
            var config = await _resourcesManager.LoadAssetAsync<PhraseConfig>(fileName);
            return config.PhraseData;
        }

        private List<string> GetAssetsNames(ISimpleQuestion nextQuestion)
        {
            List<string> result = new();
            switch (nextQuestion.QuestionType)
            {
                case QuestionType.SelectWord:
                    var selectWord = (SimpleQuestSelectWord)nextQuestion;
                    result.Add(selectWord.CorrectWordFileName);
                    result.AddRange(selectWord.MixWordsFileNames);
                    break;

                case QuestionType.MatchWords:
                    var matchWords = (SimpleQuestMatchWords)nextQuestion;
                    result.AddRange(matchWords.MatchWordsFileNames);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"simple question not handled {nextQuestion.FileName}, {nextQuestion.QuestionType}");
            }

            return result;
        }

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
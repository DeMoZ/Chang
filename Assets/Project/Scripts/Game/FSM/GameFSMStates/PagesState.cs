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
using Sirenix.Utilities;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class PagesState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        private const string EmptyWordKey = "";

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
                Words = Bus.Words,
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
            IEnumerable<IQuestion> wQuests = Bus.Lesson.Questions.Where(q => IsWordQuest(q.Type));
            IEnumerable<IQuestion> sQuests = Bus.Lesson.Questions.Where(q => IsSentenceQuest(q.Type));

            HashSet<string> wWKeys = Enumerable.ToHashSet(wQuests.Select(q => q.GetWordsKeys)
                .SelectMany(hashSet => hashSet));

            if (sQuests.Any())
            {
                foreach (IQuestion sQuest in sQuests)
                {
                    InitSentenceQuest(sQuest);
                }
            }

            HashSet<string> sWordKeys = Enumerable.ToHashSet(sQuests.Select(q => q.GetWordsKeys)
                .SelectMany(hashSet => hashSet));

            List<Word> words = wWKeys.Select(key => Bus.Words[key]).ToList();
            await _pagesContentProvider.PreloadWordsContentAsync(words, progress, ct);

            HashSet<string> sentenceKeys = Enumerable.ToHashSet(sQuests.OfType<SentenceSelectWords>().Select(q => q.Key));

            List<Sentence> sentences = sentenceKeys.Select(key => Bus.Sentences[key]).ToList();
            if (sentences.Count > 0)
            {
                await _pagesContentProvider.PreloadSentencesContentAsync(sentences, progress, ct);
                await _pagesContentProvider.CacheContentAsync(AssetPaths.Addressables.EmptyWordPlaceHolderPath, ct);
            }
        }

        private void InitSentenceQuest(IQuestion sQuest)
        {
            if (sQuest is SentenceSelectWords sSelectWords)
            {
                if (!Bus.Sentences.TryGetValue(sSelectWords.Key, out Sentence sentence))
                {
                    throw new Exception($"Sentence with key {sSelectWords.Key} not found in Bus.Sentences");
                }

                sSelectWords.Sentence = InitSentence(sentence);
                sSelectWords.CompareWordsKeys = sSelectWords.Sentence.SentenceWords.Select(word => word.WordKey).ToList();
                float sentenceMark = _profileService.GetSentencesMark(sentence.SentenceKey);

                sSelectWords.DisplayWordsKeys = new List<string>();
                sSelectWords.MixWordsKeys = new List<string>();
                // take sWord and get its index then compare with sentenence mark
                foreach (SentenceWord sWord in sSelectWords.Sentence.SentenceWords)
                {
                    if (sWord.DisplayIndex <= sentenceMark)
                    {
                        sSelectWords.DisplayWordsKeys.Add(EmptyWordKey);
                        sSelectWords.MixWordsKeys.Add(sWord.WordKey);

                    }
                    else
                    {
                        sSelectWords.DisplayWordsKeys.Add(sWord.WordKey);
                    }
                }

                // todo chang complete mix word
                if (sSelectWords.MixWordsKeys.Count < 2)
                {
                    Debug.LogWarning("Implement mix words amout based on the sentence mark");
                }

                string defaultTranslation = sSelectWords.Sentence.DefaultTranslation;
                List<SentenceWord> dynamicWords = sSelectWords.Sentence.SentenceWords
                    .Where(w => w.Modifiers.HasFlag(Modifier.Dynamic))
                    .ToList();
                    
                object[] translationArgs = dynamicWords.Select(w => (object)Bus.Words[w.WordKey].Translation).ToArray();
                defaultTranslation = string.Format(defaultTranslation, translationArgs);
                sSelectWords.SetTranslation(defaultTranslation);

                HashSet<string> allWordsKeys = new(sSelectWords.CompareWordsKeys.Concat(sSelectWords.MixWordsKeys));
                sSelectWords.SetImageKeys(allWordsKeys.Select(key => Bus.Words[key].ImageKey));
                sSelectWords.SetSoundKeys(allWordsKeys.Select(key => Bus.Words[key].SoundKey));
            }
        }

        private Sentence InitSentence(Sentence sentence)
        {
            Sentence result = new(sentence);

            foreach (SentenceWord sentenceWord in result.SentenceWords)
            {
                Debug.Log(
                    $"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with modifiers {sentenceWord.Modifiers}");

                if (!Bus.Words.ContainsKey(sentenceWord.WordKey))
                {
                    throw new Exception(
                        $"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with Dynamic modifier, but the word is not in the Bus.Words");
                }

                if (sentenceWord.Modifiers == Modifier.None)
                {
                    continue;
                }

                if (sentenceWord.Modifiers.HasFlag(Modifier.Dynamic))
                {
                    SetDynamicWord(sentenceWord);
                }

                if (sentenceWord.Modifiers.HasFlag(Modifier.Gender))
                {
                    SetGenderWord(sentenceWord);
                }
            }

            return result;

            void SetDynamicWord(SentenceWord sentenceWord)
            {
                Debug.Log($"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with Dynamic modifier");
                Word word = Bus.Words[sentenceWord.WordKey];
                string section =
                    ElementsPaths.VocabularySectionKey(_profileService.ProfileData.LearnLanguage, word.Section);
                List<Lesson> sectionLessons = Bus.VocabularySections[section].Lessons;
                List<string> wordKeys = Enumerable.ToHashSet(sectionLessons.SelectMany(lesson => lesson.Keys)).ToList();
                string randomWordKey = wordKeys[UnityEngine.Random.Range(0, wordKeys.Count)];
                sentenceWord.WordKey = randomWordKey;
            }

            void SetGenderWord(SentenceWord sentenceWord)
            {
                if (_profileService.LearnLanguage != Languages.Thai)
                {
                    Debug.LogError(
                        $"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with Gender modifier, but the language is not Thai");
                    return;
                }

                Debug.Log($"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with Gender modifier");

                /*  phom chan ka krap
                    Thai/Vocabulary/Gender/_Polite male_
                    Thai/Vocabulary/Gender/_Polite female_
                    Thai/Vocabulary/Gender/_Man I_
                    Thai/Vocabulary/Gender/_Woman I_
                 */
                switch (_profileService.ProfileData.Gender)
                {
                    case GenderType.Female:
                        switch (sentenceWord.WordKey)
                        {
                            case "Thai/Vocabulary/Gender/_Polite male_":
                            case "Thai/Vocabulary/Gender/_Polite female_":
                                sentenceWord.WordKey = "Thai/Vocabulary/Gender/_Polite female_";
                                break;
                            case "Thai/Vocabulary/Gender/_Man I_":
                            case "Thai/Vocabulary/Gender/_Woman I_":
                                sentenceWord.WordKey = "Thai/Vocabulary/Gender/_Woman I_";
                                break;
                            default:
                                Debug.LogError(
                                    $"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with Gender modifier");
                                break;
                        }

                        break;
                    case GenderType.Male:
                        switch (sentenceWord.WordKey)
                        {
                            case "Thai/Vocabulary/Gender/_Polite male_":
                            case "Thai/Vocabulary/Gender/_Polite female_":
                                sentenceWord.WordKey = "Thai/Vocabulary/Gender/_Polite male_";
                                break;
                            case "Thai/Vocabulary/Gender/_Man I_":
                            case "Thai/Vocabulary/Gender/_Woman I_":
                                sentenceWord.WordKey = "Thai/Vocabulary/Gender/_Man I_";
                                break;
                            default:
                                Debug.LogError(
                                    $"Sentence {sentence.SentenceKey} has word {sentenceWord.WordKey} with Gender modifier");
                                break;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private bool IsWordQuest(ChangTypes argType)
        {
            return argType == ChangTypes.DemonstrationWord || argType == ChangTypes.SelectWord ||
                   argType == ChangTypes.MatchWords;
        }

        private bool IsSentenceQuest(ChangTypes argType)
        {
            return argType == ChangTypes.SentenceSelectWords;
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
            Debug.Log(
                $"The answer is <color={isCorrectColor}>{isCorrect}</color>; {_pagesBus.QuestionResult.Presentation}");
            var needIncrement = !_pagesBus.QuestionResult.IsHintUsed;
            _profileService.AddVocabularyLog(_pagesBus.QuestionResult.Key, _pagesBus.QuestionResult.Presentation,
                ChangTypes.SelectWord, isCorrect,
                needIncrement);

            if (!isCorrect)
            {
                _pagesBus.Lesson.EnqueueCurrentQuestion();
            }

            var info = new ContinueButtonInfo();
            info.IsCorrect = isCorrect;
            info.InfoText = _pagesBus.QuestionResult.Key;

            _pagesBus.LessonLog.Add(_pagesBus.QuestionResult);

            _gameOverlayController.SetContinueButtonInfo(info);
            _gameOverlayController.EnableContinueButton(true);
            await _profileService.SaveProgressAsync(ct);
        }

        private async UniTaskVoid OnCheckMatchWordsAsync(CancellationToken ct)
        {
            Debug.Log($"{nameof(OnCheckMatchWordsAsync)}");

            MatchWordsResult stateResult = _pagesBus.QuestionResult as MatchWordsResult;
            if (stateResult == null)
            {
                throw new NullReferenceException($"{nameof(MatchWordsResult)} is null");
            }

            foreach (WordResult result in stateResult.WordResults)
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
            var answer = string.Join(" / ", _pagesBus.QuestionResult.Presentation);
            Debug.Log($"The answer is <color={isCorrectColor}>{isCorrect}</color>; {answer}");

            SentenceSelectWordStateResult stateResult = _pagesBus.QuestionResult as SentenceSelectWordStateResult;
            if (stateResult == null)
            {
                throw new NullReferenceException($"{nameof(MatchWordsResult)} is null");
            }

            bool needIncrement = !_pagesBus.QuestionResult.IsHintUsed;

            if (stateResult.Info[2] is List<WordResult> vocabularyResults)
            {
                foreach (WordResult vocabularyResult in vocabularyResults)
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
                InfoText = _pagesBus.QuestionResult.Presentation
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
                    lesson.AddQuestion(matchWordsQuest);
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

                foreach (string key in keys)
                {
                    if (IsNeedDemonstration(key))
                    {
                        var demonstration = new QuestSelectWord
                        {
                            Key = key
                        };
                        lesson.InsertNextQuest(demonstration);
                        nextQuestionType = ChangTypes.DemonstrationWord;
                        break;
                    }
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

            HashSet<string> selectWordQuests = Enumerable.ToHashSet(lesson.Keys);
            matchWords.AddRange(selectWordQuests);

            if (matchWords.Count < 2)
            {
                string lessonPath = string.Join("/",
                    new List<string> { lesson.Language.ToString(), "Vocabulary", lesson.Section });
                Debug.LogWarning(
                    $"matchWords not generated for lesson : {lessonPath}, count select words {matchWords.Count}");
                return false;
            }

            matchWords = _pagesBus.GameType == GameType.Learn
                ? Enumerable.ToHashSet(matchWords.Take(ProjectConstants.MAX_WORDS_IN_LEARN_MATCH_WORD_PAGE))
                : Enumerable.ToHashSet(matchWords.Take(ProjectConstants.MAX_WORDS_IN_REPEAT_MATCHT_WORDS_PAGE));

            matchWords.Shuffle();
            questMatchWords.MatchWordsKeys = matchWords;

            return true;
        }

        private bool IsNeedDemonstration(string key)
        {
            bool logExists = _profileService.TryGetVocabularyLog(key, out var questLog);

            if (!logExists)
            {
                Debug.Log($"Demonstration required. No log for: {key}");
                return true;
            }

            bool isSmallMark = questLog.Mark < 1;

            if (isSmallMark)
            {
                Debug.Log($"Demonstration required. Mark: {questLog.Mark} for: {key}");
            }

            return isSmallMark;
        }
    }
}
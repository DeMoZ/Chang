using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Services;
using Chang.GameBook;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Sentences
{
    public class SentencesController : IViewController, IBookController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly BookSentencesView _view;
        private readonly ProfileService _profileService;
        private readonly SentencesRepetitionService _repetitionService;

        private Dictionary<string, Lesson> _lessons = new();
        private Dictionary<string, SectionBlock> _sectionBlocks = new();
        private CancellationTokenSource _cts;
        private Action _onLobbyExitState;

        [Inject]
        public SentencesController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            BookSentencesView view,
            ProfileService profileService,
            SentencesRepetitionService sentencesRepetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _repetitionService = sentencesRepetitionService;

            _cts = new CancellationTokenSource();
        }

        public void Init(Action onLobbyExitState)
        {
            _onLobbyExitState = onLobbyExitState;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public async UniTask SetAsync(CancellationToken ct)
        {
            _sectionBlocks.Clear();
            _lessons.Clear();
            _view.Clear();

            for (int i = 0; i < _gameBus.SentencesBook.Sections.Count; i++)
            {
                Color baseColor = _view.GetNextColor(i);
                SentencesSection section = _gameBus.SentencesBook.Sections[i];
                SectionBlock sectionBlock = _view.InstantiateSectionBlock();
                sectionBlock.SetBaseColor(baseColor);
                sectionBlock.SectionView.name = $"SectionBlock_{section.Section}";
                _sectionBlocks.Add(section.Section, sectionBlock);

                sectionBlock.SectionView.Init(section.Section,
                    () => OnSectionSortClick(section.Section),
                    () => OnSectionRepetitionClick(section.Section));

                sectionBlock.SectionView.name = $"Section_{section.Section}";
                sectionBlock.SectionView.SetBaseColor(baseColor);

                await PopulateSectionAsync(section, sectionBlock, ct);
            }

            await UniTask.Yield();

            SetScrollPosition();
        }

        public void OnGeneralRepeatClicked()
        {
            throw new NotImplementedException();
        }

        private Color GetLessonColor(Lesson lesson)
        {
            float sum = 0;

            foreach (IQuestion question in lesson.Questions)
            {
                if (question is SentenceSelectWords selectWord)
                {
                    sum += _profileService.GetSentencesMark(selectWord.Key) /
                           (ProjectConstants.MARK_MAX * lesson.Questions.Count);
                }
                else
                {
                    throw new NotImplementedException($"Question type {question.Type} is not implemented");
                }
            }

            return _view.GetLessonColor(sum);
        }

        private void OnSectionSortClick(string key)
        {
            throw new System.NotImplementedException();
            /*
            Debug.Log($"OnSectionSortClick key: {key}");
            SectionData sectionData = _gameBus.SentencesBookData.Sections.Find(s => s.Section == key);

            if (_profileService.ReorderedVocabularySections.TryGetValue(_profileService.ReorderedSectionKey(sectionData.Section), out _))
            {
                _profileService.ReorderedVocabularySections.Remove(_profileService.ReorderedSectionKey(sectionData.Section));
            }
            else
            {
                _profileService.ReorderSentencesSection(sectionData);
            }

            SectionBlock sectionBlock = _sectionBlocks[key];

            foreach (Transform child in sectionBlock.Container)
            {
                if (!child.name.Contains("Section"))
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }

            PopulateSectionAsync(sectionData, sectionBlock, _cts.Token).Forget();
            */
        }

        private async UniTask PopulateSectionAsync(SentencesSection section, SectionBlock sectionBlock,
            CancellationToken ct)
        {
            List<SentenceQuestLog> repetitions = await _repetitionService
                .GetSectionRepetitionAsync(ProjectConstants.SECTION_REPETITION_AMOUNT, section.Section, ct);

            int repetitionsCount = repetitions.Count;
            string reorderedSectionKey = _profileService.ReorderedSectionKey(section.Section);

            sectionBlock.SectionView.SetSortToggle(
                repetitionsCount > 0 && _profileService.ReorderedSentencesSections.ContainsKey(reorderedSectionKey),
                repetitionsCount > 0);

            sectionBlock.SectionView.SetInteractableRepeatButton(repetitionsCount >=
                                                                 ProjectConstants
                                                                     .SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT);

            if (_profileService.ReorderedSentencesSections.TryGetValue(reorderedSectionKey,
                    out SentencesSection reorderedSection))
            {
                section = reorderedSection;
            }

            RectTransform row = null;
            int count = -1;
            for (int m = 0; m < section.SectionLessons.Count; m++)
            {
                if (m / 6 > count)
                {
                    count++;
                    row = _view.InstantiateRow(sectionBlock.Container);
                }

                string sectionName = section.SectionKey;
                int lessonIndex = m + 1;
                string key = $"{section.Section}_{m + 1}";
                _lessons[key] = section.SectionLessons[m];

                GameBookItem lessonItem = m % 2 == 0
                    ? _view.InstantiateUpLesson(row)
                    : _view.InstantiateDownLesson(row);

                lessonItem.Init((m + 1).ToString(), 0, () => OnLessonClick(sectionName, lessonIndex));
                lessonItem.name = $"Item {key}";
                Color color = GetLessonColor(section.SectionLessons[m]);
                lessonItem.SetColor(color);
            }
        }

        private void OnSectionRepetitionClick(string key)
        {
            Debug.Log($"OnSectionRepetitionClick key: {key}");
            SaveScrollPosition();
            OnSectionRepeatClickedAsync(key, _cts.Token).Forget();
        }

        private void OnLessonClick(string sectionName, int lessonIndex)
        {
            Debug.Log($"Clicked on item {sectionName}_{lessonIndex}");
            SaveScrollPosition();
            OnLessonClickedAsync(sectionName, lessonIndex, _cts.Token).Forget();
        }

        private void SaveScrollPosition()
        {
            _profileService.VocabularyProgress.ScrollPosition = _view.ScrollPosition;
            Debug.Log(
                $"Load gamebook scroll position: {_profileService.VocabularyProgress.ScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private void SetScrollPosition()
        {
            _view.ScrollPosition = _profileService.VocabularyProgress.ScrollPosition;
            Debug.Log(
                $"Load gamebook scroll position: {_profileService.VocabularyProgress.ScrollPosition}, scroll position: {_view.ScrollPosition}");
        }

        private async UniTaskVoid OnLessonClickedAsync(string sectionKey, int lessonIndex, CancellationToken ct)
        {
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            if (_mainScreenBus.IsLoading)
            {
                return;
            }

            _mainScreenBus.IsLoading = true;

            Lesson lesson;
            string lessonKey = _profileService.ReorderedSectionKey(sectionKey);

            if (_profileService.ReorderedSentencesSections.TryGetValue(lessonKey, out SentencesSection section))
            {
                lesson = section.SectionLessons[lessonIndex - 1];
            }
            else
            {
                lesson = _gameBus.SentencesSections[sectionKey].SectionLessons[lessonIndex - 1];
            }
            
            lesson.SetQuestions(lesson.Questions);
            InitQuestions(lesson);
            _gameBus.SetLesson(lesson);
            
            _mainScreenBus.IsLoading = false;
            _gameBus.GameType = GameType.Learn;
            _onLobbyExitState?.Invoke();
        }

        private void InitQuestions(Lesson lesson)
        {
            foreach (IQuestion question in lesson.Questions)
            {
                var quest = question as SentenceSelectWords;
                if (quest == null)
                {
                    Debug.LogWarning($"Question is not of type SentenceSelectWords.");
                    throw new InvalidOperationException($"Question is not of type SentenceSelectWords.");
                }

                if (!_gameBus.Sentences.TryGetValue(quest.Key, out Sentence sentence))
                {
                    Debug.LogWarning($"Sentence with key {quest.Key} not found in Sentences.");
                    throw new KeyNotFoundException($"Sentence with key {quest.Key} not found in Sentences.");
                }

                /*
                                public HashSet<string> MatchWordsKeys;
                                public string LocalizationKey { get; set; }
                                public string DefaultTranslation { get; set; }
                                public string ImageKey { get; set; }
                                public List<string> CompareWordsFileNames { get; set; }
                                public List<string> DisplayWordsFileNames { get; set; }
                                public List<string> MixWordsFileNames { get; set; }
                                public string LogKey { get; set; }
                             */
                            
                // MatchWordsKeys = busSentences[sentenceKey].WordsKeys,
                            
                // LocalizationKey = busSentences[sentenceKey].Key,
                // DefaultTranslation = busSentences[sentenceKey].DefaultTranslation,
                // ImageKey = busSentences[sentenceKey].ImageKey,
                // SoundKey = busSentences[sentenceKey].SoundKey,
                // // CompareWordsKeys = busSentences[sentenceKey].CompareWordsKeys,
                // // DisplayWordsKeys = busSentences[sentenceKey].DisplayWordsKeys,
                // // MixWordsKeys = busSentences[sentenceKey].MixWordsKeys,
                // Key = busSentences[sentenceKey].Key,
                // LogKey = busSentences[sentenceKey].SentenceKey,
                
                quest.Sentence = sentence;
                quest.MatchWordsKeys = new HashSet<string>(sentence.SentenceWords.Select(word => word.WordKey));
                // quest.ImageKey = sentence.ImageKey;
                // quest.SoundKey = sentence.SoundKey;
                
            }
        }

        private async UniTaskVoid OnSectionRepeatClickedAsync(string section, CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;

            throw new NotImplementedException();
            // todo chang show loading animation ?
            // var repetitions = await _repetitionService.GetSectionRepetitionAsync(ProjectConstants.SECTION_REPETITION_AMOUNT, section, ct);
            // MakeRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        private async UniTaskVoid OnGeneralRepeatClickedAsync(CancellationToken ct)
        {
            if (_mainScreenBus.IsLoading)
                return;
            throw new NotImplementedException();
            // todo chang show loading animation ?
            // var repetitions = await _repetitionService.GetGeneralRepetitionAsync(ProjectConstants.GENERAL_REPETITION_AMOUNT, ct);
            // MakeRepetitionAsync(repetitions, _cts.Token).Forget();
        }

        private async UniTaskVoid MakeRepetitionAsync(List<VocabularyQuestLog> repetitions, CancellationToken ct)
        {
            throw new NotImplementedException();
            if (repetitions.Count < ProjectConstants.SECTION_REPETITION_MIMIMUM_AVAILABLE_AMOUNT)
            {
                Debug.LogWarning("Not enough logs for general repetition");
                return;
            }

            _mainScreenBus.IsLoading = true;
            await UniTask.DelayFrame(1, cancellationToken: ct); // todo chang remove delay and make method sync ?

            List<IQuestion> questions = new List<IQuestion>();

            foreach (VocabularyQuestLog questLog in repetitions)
            {
                // switch (questLog.QuestionType)
                // {
                //     case QuestionType.SelectWord:
                //         var simpleQuest = new QuestSelectWord();
                //         simpleQuest.CorrectWordFileName = questLog.FileName;
                //         var words = repetitions
                //             .Where(r => r.QuestionType == QuestionType.SelectWord && r.FileName != simpleQuest.CorrectWordFileName)
                //             .ToList();
                //
                //         words.Shuffle();
                //
                //         simpleQuest.MixWordsFileNames = words.Take(ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE)
                //             .Select(w => w.FileName)
                //             .ToList();
                //
                //         questions.Add(simpleQuest);
                //         break;
                //
                //     default:
                //         throw new NotImplementedException($"Not implemented simple quest generation for type: {questLog.QuestionType}");
                // }
            }

            // var lesson = new Lesson();
            // lesson.GenerateQuestMatchWordsData = true;
            // lesson.SetSimpleQuestions(questions);
            //
            // _gameBus.CurrentVocabularyLesson = lesson;
            // _mainScreenBus.IsLoading = false;
            //
            // _gameBus.GameType = GameType.Repetition;
            _onLobbyExitState?.Invoke();
        }
    }
}
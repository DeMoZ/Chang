using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Profile;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    /// <summary>
    /// Repetition screen: words and sentences log ordered by the last answer time, repetition buttons
    /// </summary>
    public class RepetitionController : IViewController
    {
        private const int ShowLogLimitAmount = 30;

        private readonly ProfileService _profileService;
        private readonly GameBus _gameBus;
        private readonly RepetitionView _view;
        private readonly RepetitionService _repetitionService;

        [Inject]
        public RepetitionController(
            ProfileService profileService,
            GameBus gameBus,
            RepetitionView view,
            RepetitionService repetitionService)
        {
            _profileService = profileService;
            _gameBus = gameBus;
            _view = view;
            _repetitionService = repetitionService;
        }

        public void Dispose()
        {
        }

        public void Init(Action onRepeatWordsClick, Action onRepeatSentencesClick, Action onRepeatMixedClick)
        {
            _view.Init(onRepeatWordsClick, onRepeatSentencesClick, onRepeatMixedClick);
        }

        public async UniTask SetAsync(CancellationToken ct)
        {
            await UniTask.Yield(ct);

            List<RepetitionLogItem> items = _repetitionService.GetAllPlayed()
                .Select(candidate => candidate.Kind == RepetitionKind.Word
                    ? CreateWordItem(candidate.Key)
                    : CreateSentenceItem(candidate.Key))
                .OrderByDescending(item => item.UtcTime)
                .Take(ShowLogLimitAmount)
                .ToList();

            _view.Set(items);
            _view.SetSummary(_repetitionService.GetSummary());
            _view.SetInteractableRepeatButtons(
                _repetitionService.CanRepeatVocabulary(),
                _repetitionService.CanRepeatSentences(),
                _repetitionService.CanRepeatMixed());
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        private RepetitionLogItem CreateWordItem(string key)
        {
            Word word = _gameBus.Words[key];
            VocabularyQuestLog log = _profileService.VocabularyProgress.Log[key];

            return new RepetitionLogItem(word.LearnWord, word.Translation, log.Mark, log.Log.Count, log.UtcTime, log.SuccessSequence);
        }

        private RepetitionLogItem CreateSentenceItem(string key)
        {
            Sentence sentence = _gameBus.Sentences[key];
            SentenceQuestLog log = _profileService.SentencesProgress.Log[key];

            // the book sentence, not the player answer
            string learnSentence = string.Join("", sentence.SentenceWords.Select(word => _gameBus.Words[word.WordKey].LearnWord));

            return new RepetitionLogItem(learnSentence, sentence.GetTranslation(_gameBus.Words), log.Mark, log.Log.Count, log.UtcTime, log.SuccessSequence);
        }

        // todo chang implement items interacitons - show popup with word in Thai, translation, mark, info from log - marks, list when played
        // private void OnItemClick(int index)
        // {
        //     Debug.Log($"Clicked on item {index}");
        //
        //     //_mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[index].FileName);
        // }
    }
}

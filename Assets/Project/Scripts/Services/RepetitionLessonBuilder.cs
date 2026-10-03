using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Core;
using Chang.Profile;
using Zenject;

namespace Chang.Services
{
    /// <summary>
    /// Builds a lesson from the repetition keys, words and sentences may be mixed
    /// </summary>
    public class RepetitionLessonBuilder
    {
        private const string RepetitionSection = "Repetition";

        private readonly GameBus _gameBus;
        private readonly ProfileService _profileService;

        [Inject]
        public RepetitionLessonBuilder(GameBus gameBus, ProfileService profileService)
        {
            _gameBus = gameBus;
            _profileService = profileService;
        }

        public Lesson Build(List<RepetitionCandidate> repetitions)
        {
            List<string> keys = repetitions.Select(candidate => candidate.Key).ToList();
            HashSet<string> lessonWordsKeys = repetitions
                .Where(candidate => candidate.Kind == RepetitionKind.Word)
                .Select(candidate => candidate.Key)
                .ToHashSet();

            List<IQuestion> questions = repetitions
                .Select(candidate => candidate.Kind switch
                {
                    RepetitionKind.Word => CreateWordQuestion(candidate.Key, lessonWordsKeys),
                    RepetitionKind.Sentence => CreateSentenceQuestion(candidate.Key),
                    _ => throw new ArgumentOutOfRangeException(nameof(candidate.Kind), candidate.Kind, null),
                })
                .ToList();

            Lesson lesson = new(_profileService.LearnLanguage, RepetitionSection, keys);
            lesson.SetQuestions(questions);
            return lesson;
        }

        private IQuestion CreateWordQuestion(string key, HashSet<string> lessonWordsKeys)
        {
            return new QuestSelectWord
            {
                Key = key,
                WordsKeys = GetMixWordsKeys(key, lessonWordsKeys),
                SectionKey = ElementsPaths.VocabularySectionKey(_profileService.LearnLanguage, _gameBus.Words[key].Section),
            };
        }

        private IQuestion CreateSentenceQuestion(string key)
        {
            Sentence sentence = _gameBus.Sentences[key];

            // the rest of the sentence quest is initialized on the pages start
            return new SentenceSelectWords
            {
                Key = key,
                Sentence = sentence,
            };
        }

        /// <summary>
        /// The mix is taken from the other lesson words, then from the word section, then from all played words.
        /// Played words go first, not played section words fill the mix only when played words are not enough
        /// </summary>
        private HashSet<string> GetMixWordsKeys(string key, HashSet<string> lessonWordsKeys)
        {
            int amount = ProjectConstants.MIX_WORDS_AMOUNT_IN_REPEAT_SELECT_WORD_PAGE;
            HashSet<string> result = new();
            List<string> sectionKeys = _gameBus.VocabularySections.TryGetValue(
                ElementsPaths.VocabularySectionKey(_profileService.LearnLanguage, _gameBus.Words[key].Section),
                out VocabularySection section)
                ? section.Lessons.SelectMany(lesson => lesson.Keys).ToList()
                : new List<string>();

            AddRandomWords(result, lessonWordsKeys, key, amount, true);
            AddRandomWords(result, sectionKeys, key, amount, true);
            AddRandomWords(result, _profileService.VocabularyProgress.Log.Keys, key, amount, true);
            AddRandomWords(result, sectionKeys, key, amount, false);

            return result;
        }

        private void AddRandomWords(HashSet<string> result, IEnumerable<string> keys, string excludeKey, int amount, bool isPlayedOnly)
        {
            if (result.Count >= amount)
            {
                return;
            }

            Dictionary<string, VocabularyQuestLog> playedLog = _profileService.VocabularyProgress.Log;
            List<string> words = keys
                .Where(k => k != excludeKey && !result.Contains(k) && _gameBus.Words.ContainsKey(k))
                .Where(k => !isPlayedOnly || playedLog.ContainsKey(k))
                .Distinct()
                .ToList();

            words.Shuffle();
            result.UnionWith(words.Take(amount - result.Count));
        }
    }
}

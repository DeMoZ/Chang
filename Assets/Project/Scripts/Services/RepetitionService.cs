using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Core;
using Chang.Profile;
using Zenject;

namespace Chang.Services
{
    public enum RepetitionKind
    {
        Word,
        Sentence,
    }

    public readonly struct RepetitionCandidate
    {
        public readonly string Key;
        public readonly RepetitionKind Kind;

        /// <summary>
        /// Time since the last answer / interval of the mark. 1 and more means the repetition is due, the bigger the more overdue
        /// </summary>
        public readonly double Priority;

        public bool IsDue => Priority >= 1;

        public RepetitionCandidate(string key, RepetitionKind kind, double priority)
        {
            Key = key;
            Kind = kind;
            Priority = priority;
        }
    }

    public readonly struct RepetitionSummary
    {
        public readonly int PlayedWords;
        public readonly int PlayedSentences;

        /// <summary>Answers kept in the logs: the recent ones, the log of a key is limited</summary>
        public readonly int Answers;

        public readonly int DueWords;
        public readonly int DueSentences;

        /// <summary>Time until the next not due key becomes due, null when every played key is due</summary>
        public readonly TimeSpan? NextDueIn;

        public RepetitionSummary(int playedWords, int playedSentences, int answers, int dueWords, int dueSentences, TimeSpan? nextDueIn)
        {
            PlayedWords = playedWords;
            PlayedSentences = playedSentences;
            Answers = answers;
            DueWords = dueWords;
            DueSentences = dueSentences;
            NextDueIn = nextDueIn;
        }
    }

    /// <summary>
    /// Selects played keys for repetition lessons. The weak and the long ago repeated keys come first:
    /// a low mark gives a short interval from the config, so the key becomes due sooner
    /// </summary>
    public class RepetitionService
    {
        /// <summary>
        /// Mark 0 has zero interval, the key is due a minute after the answer
        /// </summary>
        private const double MinIntervalHours = 1d / 60;

        /// <summary>
        /// Very overdue keys don't push the others out of the random choice
        /// </summary>
        private const double MaxPickWeight = 10;
        private const double MinPickWeight = 0.01;

        private readonly ProfileService _profileService;
        private readonly GameBus _gameBus;
        private readonly RepetitionConfig _config;
        private readonly Random _random = new();

        [Inject]
        public RepetitionService(ProfileService profileService, GameBus gameBus, RepetitionConfig config)
        {
            _profileService = profileService;
            _gameBus = gameBus;
            _config = config;
        }

        /// <param name="section">null for the whole vocabulary</param>
        public bool CanRepeatVocabulary(VocabularySection section = null)
        {
            return GetVocabularyCandidates(section).Count >= _config.MinAvailableAmount;
        }

        /// <param name="section">null for all sentences</param>
        public bool CanRepeatSentences(SentencesSection section = null)
        {
            return GetSentencesCandidates(section).Count >= _config.MinAvailableAmount;
        }

        public bool CanRepeatMixed()
        {
            return GetVocabularyCandidates(null).Count + GetSentencesCandidates(null).Count >= _config.MinAvailableAmount;
        }

        /// <param name="section">null for the whole vocabulary</param>
        public List<RepetitionCandidate> GetVocabularyRepetition(VocabularySection section = null)
        {
            return Pick(GetVocabularyCandidates(section), _config.LessonAmount);
        }

        /// <param name="section">null for all sentences</param>
        public List<RepetitionCandidate> GetSentencesRepetition(SentencesSection section = null)
        {
            return Pick(GetSentencesCandidates(section), _config.LessonAmount);
        }

        /// <summary>
        /// Words amount is random in the config range, the rest are sentences. A lack of one kind is filled with the other
        /// </summary>
        public List<RepetitionCandidate> GetMixedRepetition()
        {
            List<RepetitionCandidate> words = GetVocabularyCandidates(null);
            List<RepetitionCandidate> sentences = GetSentencesCandidates(null);

            int wordsAmount = _random.Next(_config.MixedWordsMin, _config.MixedWordsMax + 1);
            int sentencesAmount = _config.LessonAmount - wordsAmount;

            wordsAmount = Math.Min(wordsAmount + Math.Max(0, sentencesAmount - sentences.Count), words.Count);
            sentencesAmount = Math.Min(_config.LessonAmount - wordsAmount, sentences.Count);

            List<RepetitionCandidate> result = Pick(words, wordsAmount);
            result.AddRange(Pick(sentences, sentencesAmount));
            result.Shuffle();

            return result;
        }

        /// <returns>all played words and sentences that are in the book</returns>
        public List<RepetitionCandidate> GetAllPlayed()
        {
            return GetVocabularyCandidates(null)
                .Concat(GetSentencesCandidates(null))
                .ToList();
        }

        /// <summary>
        /// Counters of the repetition screen
        /// </summary>
        public RepetitionSummary GetSummary()
        {
            DateTime now = DateTime.UtcNow;
            List<RepetitionCandidate> words = GetVocabularyCandidates(null);
            List<RepetitionCandidate> sentences = GetSentencesCandidates(null);

            int answers = words.Sum(word => _profileService.VocabularyProgress.Log[word.Key].Log.Count)
                          + sentences.Sum(sentence => _profileService.SentencesProgress.Log[sentence.Key].Log.Count);

            IEnumerable<DateTime> dueTimes = words
                .Where(word => !word.IsDue)
                .Select(word => GetDueTime(_profileService.VocabularyProgress.Log[word.Key].Mark, _profileService.VocabularyProgress.Log[word.Key].UtcTime))
                .Concat(sentences
                    .Where(sentence => !sentence.IsDue)
                    .Select(sentence => GetDueTime(_profileService.SentencesProgress.Log[sentence.Key].Mark, _profileService.SentencesProgress.Log[sentence.Key].UtcTime)));

            TimeSpan? nextDueIn = dueTimes.Any() ? dueTimes.Min() - now : null;

            return new RepetitionSummary(words.Count, sentences.Count, answers,
                words.Count(word => word.IsDue), sentences.Count(sentence => sentence.IsDue), nextDueIn);
        }

        private DateTime GetDueTime(int mark, DateTime lastAnswerUtcTime)
        {
            return lastAnswerUtcTime.AddHours(Math.Max(_config.GetIntervalHours(mark), MinIntervalHours));
        }

        /// <summary>
        /// Section keys are taken from the book section lessons, the section in the key path is ignored:
        /// a sentences section may contain sentences of other sections, e.g. Market sentences in Fruits
        /// </summary>
        private List<RepetitionCandidate> GetVocabularyCandidates(VocabularySection section)
        {
            Dictionary<string, VocabularyQuestLog> log = _profileService.VocabularyProgress.Log;
            IEnumerable<string> keys = section == null
                ? log.Keys.Where(_gameBus.Words.ContainsKey)
                : section.Lessons.SelectMany(lesson => lesson.Keys).Distinct().Where(log.ContainsKey);

            DateTime now = DateTime.UtcNow;
            return keys
                .Select(key => new RepetitionCandidate(key, RepetitionKind.Word, GetPriority(log[key].Mark, log[key].UtcTime, now)))
                .ToList();
        }

        /// <inheritdoc cref="GetVocabularyCandidates"/>
        private List<RepetitionCandidate> GetSentencesCandidates(SentencesSection section)
        {
            Dictionary<string, SentenceQuestLog> log = _profileService.SentencesProgress.Log;
            IEnumerable<string> keys = section == null
                ? log.Keys.Where(_gameBus.Sentences.ContainsKey)
                : section.SectionLessons.SelectMany(lesson => lesson.Keys).Distinct().Where(log.ContainsKey);

            DateTime now = DateTime.UtcNow;
            return keys
                .Select(key => new RepetitionCandidate(key, RepetitionKind.Sentence, GetPriority(log[key].Mark, log[key].UtcTime, now)))
                .ToList();
        }

        private double GetPriority(int mark, DateTime lastAnswerUtcTime, DateTime now)
        {
            double hours = (now - lastAnswerUtcTime).TotalHours;
            double interval = Math.Max(_config.GetIntervalHours(mark), MinIntervalHours);
            return hours / interval;
        }

        /// <summary>
        /// Due keys go first, not due keys fill the lesson only when due keys are not enough
        /// </summary>
        private List<RepetitionCandidate> Pick(List<RepetitionCandidate> candidates, int amount)
        {
            List<RepetitionCandidate> sorted = candidates.OrderByDescending(candidate => candidate.Priority).ToList();

            List<RepetitionCandidate> result = PickWeighted(sorted.Where(candidate => candidate.IsDue).ToList(), amount);
            result.AddRange(PickWeighted(sorted.Where(candidate => !candidate.IsDue).ToList(), amount - result.Count));

            return result;
        }

        /// <summary>
        /// Random choice from the top of the sorted candidates, the higher priority the higher chance
        /// </summary>
        private List<RepetitionCandidate> PickWeighted(List<RepetitionCandidate> sorted, int amount)
        {
            List<RepetitionCandidate> pool = sorted.Take(amount * _config.PoolMultiplier).ToList();
            List<RepetitionCandidate> result = new();

            while (result.Count < amount && pool.Count > 0)
            {
                double totalWeight = pool.Sum(GetPickWeight);
                double value = _random.NextDouble() * totalWeight;

                int index = 0;
                for (; index < pool.Count - 1; index++)
                {
                    value -= GetPickWeight(pool[index]);
                    if (value < 0)
                    {
                        break;
                    }
                }

                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private static double GetPickWeight(RepetitionCandidate candidate)
        {
            return Math.Clamp(candidate.Priority, MinPickWeight, MaxPickWeight);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Core;
using Chang.Profile;
using Zenject;

namespace Chang.Services
{
    /// <summary>
    /// Sorts section lessons keys by their marks in the log
    /// </summary>
    public class SectionSortService
    {
        private readonly ProfileService _profileService;
        private readonly PlayerProfile _playerProfile;

        [Inject]
        public SectionSortService(ProfileService profileService, PlayerProfile playerProfile)
        {
            _profileService = profileService;
            _playerProfile = playerProfile;
        }

        /// <returns>true if at least one section key is in the log</returns>
        public bool CanSort(VocabularySection section)
        {
            Dictionary<string, VocabularyQuestLog> log = _profileService.VocabularyProgress.Log;
            return section.Lessons.SelectMany(lesson => lesson.Keys).Any(log.ContainsKey);
        }

        /// <returns>true if at least one section key is in the log</returns>
        public bool CanSort(SentencesSection section)
        {
            Dictionary<string, SentenceQuestLog> log = _profileService.SentencesProgress.Log;
            return section.SectionLessons.SelectMany(lesson => lesson.Keys).Any(log.ContainsKey);
        }

        public bool IsSorted(VocabularySection section)
        {
            return _profileService.ReorderedVocabularySections.ContainsKey(_profileService.ReorderedSectionKey(section.Section));
        }

        public bool IsSorted(SentencesSection section)
        {
            return _profileService.ReorderedSentencesSections.ContainsKey(_profileService.ReorderedSectionKey(section.Section));
        }

        /// <summary>
        /// Sorts the section or restores the original order if it is already sorted
        /// </summary>
        public void ToggleSort(VocabularySection section)
        {
            if (!_profileService.ReorderedVocabularySections.Remove(_profileService.ReorderedSectionKey(section.Section)))
            {
                Sort(section);
            }
        }

        /// <summary>
        /// Sorts the section or restores the original order if it is already sorted
        /// </summary>
        public void ToggleSort(SentencesSection section)
        {
            if (!_profileService.ReorderedSentencesSections.Remove(_profileService.ReorderedSectionKey(section.Section)))
            {
                Sort(section);
            }
        }

        /// <summary>
        /// Redistributes section keys between lessons ordered by mark descending, keeping lessons sizes
        /// </summary>
        private void Sort(VocabularySection section)
        {
            VocabularySection newSection = new VocabularySection
            {
                Language = section.Language,
                Section = section.Section,
                SectionKey = section.SectionKey,
                Lessons = SortLessons(section.Lessons, key => _profileService.GetVocabularyMark(key)),
            };

            newSection.PopulateQuestions();

            _playerProfile.AddReorderVocabularySection(_profileService.ReorderedSectionKey(section.Section), newSection);
        }

        /// <summary>
        /// Redistributes section keys between lessons ordered by mark descending, keeping lessons sizes
        /// </summary>
        private void Sort(SentencesSection section)
        {
            SentencesSection newSection = new SentencesSection
            {
                Language = section.Language,
                Section = section.Section,
                SectionKey = section.SectionKey,
                SectionLessons = SortLessons(section.SectionLessons, _profileService.GetSentencesMark),
            };

            newSection.PopulateQuestions();

            _playerProfile.AddReorderSentencesSection(_profileService.ReorderedSectionKey(section.Section), newSection);
        }

        private static List<Lesson> SortLessons(List<Lesson> lessons, Func<string, float> getMark)
        {
            Queue<string> keysQueue = new Queue<string>(lessons.SelectMany(lesson => lesson.Keys).OrderByDescending(getMark));
            List<Lesson> newLessons = new();

            foreach (Lesson lesson in lessons)
            {
                List<string> keys = new();

                for (int i = 0; i < lesson.Keys.Count; i++)
                {
                    keys.Add(keysQueue.Dequeue());
                }

                newLessons.Add(new Lesson(lesson.Language, lesson.Section, keys));
            }

            return newLessons;
        }
    }
}

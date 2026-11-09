using System;
using System.Collections.Generic;
using System.Linq;

namespace Chang.Profile
{
    /// <summary>
    /// Merges the data of a guest into an account. The account data is the base, the guest data is added on top,
    /// a larger progress is never replaced by a smaller one.
    /// </summary>
    public static class PlayerDataMerge
    {
        /// <returns>the merged data that has to be saved to the account: the profile and the languages the guest has progress in</returns>
        public static StoredPlayerData Merge(StoredPlayerData account, StoredPlayerData guest)
        {
            account ??= new StoredPlayerData();
            StoredPlayerData result = new() { Profile = MergeProfile(account.Profile, guest.Profile) };

            foreach (var (language, guestProgress) in guest.Vocabulary)
            {
                account.Vocabulary.TryGetValue(language, out var accountProgress);
                result.Vocabulary[language] = MergeProgress(accountProgress, guestProgress);
            }

            foreach (var (language, guestProgress) in guest.Sentences)
            {
                account.Sentences.TryGetValue(language, out var accountProgress);
                result.Sentences[language] = MergeProgress(accountProgress, guestProgress);
            }

            return result;
        }

        /// <summary>The account values stay, the empty ones are taken from the guest</summary>
        public static ProfileData MergeProfile(ProfileData account, ProfileData guest)
        {
            if (account == null || guest == null)
            {
                return account ?? guest;
            }

            if (string.IsNullOrWhiteSpace(account.Name))
            {
                account.Name = guest.Name;
            }

            if (account.Gender == GenderType.No)
            {
                account.Gender = guest.Gender;
            }

            if (account.Mascot == null || IsDefault(account.Mascot))
            {
                account.Mascot = guest.Mascot ?? account.Mascot;
            }

            return account;
        }

        public static ProgressData<T> MergeProgress<T>(ProgressData<T> account, ProgressData<T> guest) where T : IQuestLog
        {
            if (account == null || guest == null)
            {
                return account ?? guest;
            }

            foreach (var (key, guestLog) in guest.Log)
            {
                if (account.Log.TryGetValue(key, out T accountLog) && accountLog != null)
                {
                    MergeQuestLog(accountLog, guestLog);
                }
                else
                {
                    account.Log[key] = guestLog;
                }
            }

            account.SetTime(Max(account.UtcTime, guest.UtcTime));
            return account;
        }

        /// <summary>
        /// The account log stays. The guest mark, success sequence and time are taken only when the guest answered later
        /// and its mark is not lower. The answers of both are merged by time and cut to <see cref="ProjectConstants.LOG_LIMIT"/>.
        /// </summary>
        public static void MergeQuestLog<T>(T account, T guest) where T : IQuestLog
        {
            if (guest.UtcTime > account.UtcTime && guest.Mark >= account.Mark)
            {
                account.Mark = guest.Mark;
                account.SuccessSequence = guest.SuccessSequence;
                account.UtcTime = guest.UtcTime;
            }

            IEnumerable<LogUnit> units = (account.Log ?? Enumerable.Empty<LogUnit>())
                .Concat(guest.Log ?? Enumerable.Empty<LogUnit>())
                .Where(unit => unit != null)
                .GroupBy(unit => (unit.UtcTime, unit.IsCorrect))
                .Select(group => group.First())
                .OrderBy(unit => unit.UtcTime)
                .ToList()
                .TakeLast(ProjectConstants.LOG_LIMIT);

            account.Log = new Queue<LogUnit>(units);
        }

        private static bool IsDefault(MascotLook look)
        {
            MascotLook defaults = new();
            return MascotLook.Parts.All(part => look[part] == defaults[part]);
        }

        private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    }
}

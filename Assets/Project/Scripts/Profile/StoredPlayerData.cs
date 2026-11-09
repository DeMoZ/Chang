using System.Collections.Generic;

namespace Chang.Profile
{
    /// <summary>
    /// All the stored data of a player: the profile and the progress of every language that has it
    /// </summary>
    public class StoredPlayerData
    {
        /// <summary>null if there is no stored profile</summary>
        public ProfileData Profile { get; set; }
        public Dictionary<Languages, ProgressData<VocabularyQuestLog>> Vocabulary { get; set; } = new();
        public Dictionary<Languages, ProgressData<SentenceQuestLog>> Sentences { get; set; } = new();

        public bool IsEmpty => Profile == null && Vocabulary.Count == 0 && Sentences.Count == 0;
    }

    /// <summary>
    /// The data of a guest who signed in to an existing account, kept locally until it is merged into the account
    /// </summary>
    public class PendingGuestData
    {
        public string GuestPlayerId { get; set; }
        public StoredPlayerData Data { get; set; }
    }
}

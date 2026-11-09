using System;
using System.Threading;
using System.Threading.Tasks;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services
{
    /// <summary>
    /// A guest signs in to an existing account from the account view:
    /// 1. while the guest is still signed in, all its data is kept locally as <see cref="PendingGuestData"/>;
    /// 2. after the switch the game reboots and <see cref="LoadStoredData"/> merges it into the account (<see cref="PlayerDataMerge"/>).
    /// The pending data survives a failed merge or a closed app and is merged on the next load.
    /// A guest who signs up keeps its player id, so its data needs no merge.
    /// </summary>
    public partial class ProfileService
    {
        private readonly CancellationTokenSource _cts = new();

        private async Task KeepGuestDataForMergeAsync()
        {
            CancellationToken ct = _cts.Token;
            string guestId = PlayerId;

            // the last answers may not be saved yet
            await SaveProgressAsync(ct);

            StoredPlayerData cloud = await _unityCloudDataProvider.LoadAllAsync(ct);
            StoredPlayerData local = await LoadLocalDataAsync(guestId, ct);

            StoredPlayerData guest = new()
            {
                Profile = SelectNewer(cloud.Profile, local.Profile, data => data.UtcTime) ?? _playerProfile.ProfileData,
            };

            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                cloud.Vocabulary.TryGetValue(language, out var cloudVocabulary);
                local.Vocabulary.TryGetValue(language, out var localVocabulary);
                var vocabulary = SelectNewer(cloudVocabulary, localVocabulary, data => data.UtcTime);
                if (vocabulary != null)
                {
                    guest.Vocabulary[language] = vocabulary;
                }

                cloud.Sentences.TryGetValue(language, out var cloudSentences);
                local.Sentences.TryGetValue(language, out var localSentences);
                var sentences = SelectNewer(cloudSentences, localSentences, data => data.UtcTime);
                if (sentences != null)
                {
                    guest.Sentences[language] = sentences;
                }
            }

            _prefsDataProvider.SavePendingGuestData(new PendingGuestData { GuestPlayerId = guestId, Data = guest });
            Debug.Log($"KeepGuestDataForMergeAsync: the data of the guest {guestId} is kept, " +
                      $"vocabulary: {string.Join(", ", guest.Vocabulary.Keys)}, sentences: {string.Join(", ", guest.Sentences.Keys)}");
        }

        private void DropPendingGuestData()
        {
            Debug.Log("DropPendingGuestData: the guest stays signed in");
            _prefsDataProvider.DeletePendingGuestData();
        }

        /// <summary>
        /// Merges the kept guest data into the signed-in account, saves it to the cloud and drops the local data of the guest.
        /// On errors the pending data stays and is merged on the next load.
        /// </summary>
        private async UniTask MergePendingGuestDataAsync(string playerId, CancellationToken ct)
        {
            PendingGuestData pending = _prefsDataProvider.LoadPendingGuestData();
            if (pending == null)
            {
                return;
            }

            // the app was closed before the sign-in to the account: the guest is still signed in
            if (pending.GuestPlayerId == playerId || pending.Data == null || pending.Data.IsEmpty)
            {
                _prefsDataProvider.DeletePendingGuestData();
                return;
            }

            try
            {
                StoredPlayerData account = await _unityCloudDataProvider.LoadAllAsync(ct);
                StoredPlayerData merged = PlayerDataMerge.Merge(account, pending.Data);

                DateTime now = DateTime.UtcNow;
                merged.Profile?.SetPlayerId(playerId);
                merged.Profile?.SetTime(now);
                foreach (var progress in merged.Vocabulary.Values)
                {
                    progress.SetTime(now);
                }

                foreach (var progress in merged.Sentences.Values)
                {
                    progress.SetTime(now);
                }

                await _unityCloudDataProvider.SaveAllAsync(merged, ct);

                // the local data is the guest's one, the load takes the merged data from the cloud
                _prefsDataProvider.Clear();
                _prefsDataProvider.DeletePendingGuestData();
                Debug.Log($"MergePendingGuestDataAsync: the guest {pending.GuestPlayerId} data is merged into {playerId}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"MergePendingGuestDataAsync: failed to merge the guest data, it is merged on the next load:\n{e}");
            }
        }

        /// <returns>the local data if it belongs to the player, otherwise empty data</returns>
        private async UniTask<StoredPlayerData> LoadLocalDataAsync(string playerId, CancellationToken ct)
        {
            StoredPlayerData data = new();
            ProfileData profile = await _prefsDataProvider.LoadProfileDataAsync(ct);
            if (profile == null || profile.UnityCloudSavePlayerId != playerId)
            {
                return data;
            }

            data.Profile = profile;
            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                var vocabulary = await _prefsDataProvider.LoadVocabularyProgressDataAsync(language, ct);
                if (vocabulary != null)
                {
                    data.Vocabulary[language] = vocabulary;
                }

                var sentences = await _prefsDataProvider.LoadSentencesProgressDataAsync(language, ct);
                if (sentences != null)
                {
                    data.Sentences[language] = sentences;
                }
            }

            return data;
        }
    }
}

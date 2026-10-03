using System.Threading;
using Assets.SimpleLocalization.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services
{
    /// <summary>
    /// Sets the interface language from the profile NativeLanguage, the device language is used until the profile is loaded
    /// </summary>
    public class LocalizationService : IInitializable
    {
        private readonly ProfileService _profileService;
        private readonly LanguagesConfig _config;

        [Inject]
        public LocalizationService(ProfileService profileService, LanguagesConfig config)
        {
            _profileService = profileService;
            _config = config;
        }

        public void Initialize()
        {
            LocalizationManager.Read();
            SetLanguage(GetDeviceLanguage());
        }

        /// <returns>the enabled language matching the device one, the default language otherwise</returns>
        public Languages GetDeviceLanguage() => _config.GetDeviceLanguage(Application.systemLanguage);

        /// <summary>
        /// Applies the profile NativeLanguage, the device one if it is disabled. Call after the profile is loaded
        /// </summary>
        public void ApplyProfileLanguage()
        {
            Languages language = _profileService.ProfileData.NativeLanguage;
            SetLanguage(_config.IsEnabled(language) ? language : GetDeviceLanguage());
        }

        /// <summary>
        /// Sets the language chosen by the player and saves it in the profile
        /// </summary>
        public async UniTask SelectLanguageAsync(Languages language, CancellationToken ct)
        {
            if (!_config.IsEnabled(language))
            {
                Debug.LogError($"[{nameof(LocalizationService)}] Language is disabled: {language}");
                return;
            }

            SetLanguage(language);
            _profileService.ProfileData.NativeLanguage = language;
            await _profileService.SaveProfileDataAsync(ct);
        }

        private static void SetLanguage(Languages language)
        {
            string sheetLanguage = language.ToString();

            if (!LocalizationManager.Dictionary.ContainsKey(sheetLanguage))
            {
                Debug.LogWarning($"[{nameof(LocalizationService)}] No sheet column for {language}, English is used");
                sheetLanguage = nameof(Languages.English);
            }

            if (LocalizationManager.Language == sheetLanguage)
            {
                return;
            }

            Debug.Log($"[{nameof(LocalizationService)}] Language: {sheetLanguage}");
            LocalizationManager.Language = sheetLanguage;
        }
    }
}

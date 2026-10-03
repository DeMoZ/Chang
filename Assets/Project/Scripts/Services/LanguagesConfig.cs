using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chang.Services
{
    [CreateAssetMenu(fileName = "LanguagesConfig", menuName = "Chang/Services/Languages Config")]
    public class LanguagesConfig : ScriptableObject
    {
        /// <summary>
        /// Assets/Project/Resources/LanguagesConfig.asset
        /// </summary>
        public const string ResourcePath = "LanguagesConfig";

        [Tooltip("Interface languages, the language name is the localization sheet column")]
        [SerializeField] private List<LanguageEntry> languages = new();

        [Tooltip("Used when the device language is not enabled")]
        [SerializeField] private Languages defaultLanguage = Languages.English;

        public IReadOnlyList<LanguageEntry> Entries => languages;
        public Languages DefaultLanguage => defaultLanguage;

        public IEnumerable<Languages> EnabledLanguages => languages.Where(i => i.Enabled).Select(i => i.Language);

        public bool IsEnabled(Languages language) => languages.Any(i => i.Enabled && i.Language == language);

        /// <returns>the language name in the language itself, the enum name if not set</returns>
        public string GetDisplayName(Languages language)
        {
            LanguageEntry entry = languages.FirstOrDefault(i => i.Language == language);
            return string.IsNullOrEmpty(entry?.DisplayName) ? language.ToString() : entry.DisplayName;
        }

        /// <returns>the enabled language matching the device one, the default language otherwise</returns>
        public Languages GetDeviceLanguage(SystemLanguage systemLanguage)
        {
            LanguageEntry entry = languages.FirstOrDefault(i => i.Enabled && i.DeviceLanguages.Contains(systemLanguage));
            return entry?.Language ?? defaultLanguage;
        }
    }

    [Serializable]
    public class LanguageEntry
    {
        public Languages Language;
        public bool Enabled = true;

        [Tooltip("The language name in the language itself, shown in the language selector")]
        public string DisplayName;

        [Tooltip("Device languages to select this language for, empty if the device can't report it")]
        public SystemLanguage[] DeviceLanguages = Array.Empty<SystemLanguage>();
    }
}

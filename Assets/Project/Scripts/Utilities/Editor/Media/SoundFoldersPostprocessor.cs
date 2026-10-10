using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.Media
{
    /// <summary>
    /// Word sounds of new sections (Word Sounds (Siri) config, the unused Chang Voice app) land in new section folders:
    /// SoundWords/{Language}/{Voice}/{Section} and SoundWordsSlow/{Language}/{Voice}/{Section}.
    /// Each section folder is an Addressables entry with its path as the address, in Remote_{Language}_Sound_Words
    /// (Remote_{Language}_Sound_Words_Slow for the slow ones), created from the Remote_Thai_Sound_Words group.
    /// </summary>
    public class SoundFoldersPostprocessor : AssetPostprocessor
    {
        private const string SoundGroupTemplate = "Remote_Thai_Sound_Words";

        private static readonly Regex SectionFolder =
            new(@"^(Assets/Project/Resources_Bundled/(SoundWords|SoundWordsSlow)/([^/]+)/[^/]+/[^/]+)(/|$)");

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            var folders = new HashSet<(string Path, string Root, string Language)>();
            foreach (string path in imported)
            {
                Match match = SectionFolder.Match(path);
                if (match.Success)
                {
                    folders.Add((match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
                }
            }

            if (folders.Count > 0)
            {
                // the settings can't be changed in the middle of an import
                EditorApplication.delayCall += () => Register(folders);
            }
        }

        private static void Register(IEnumerable<(string Path, string Root, string Language)> folders)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup template = settings != null ? settings.FindGroup(SoundGroupTemplate) : null;
            if (template == null)
            {
                return;
            }

            bool changed = false;
            foreach (var folder in folders)
            {
                string guid = AssetDatabase.AssetPathToGUID(folder.Path);
                if (!AssetDatabase.IsValidFolder(folder.Path) || string.IsNullOrEmpty(guid) || settings.FindAssetEntry(guid) != null)
                {
                    continue;
                }

                string groupName = $"Remote_{folder.Language}_Sound_Words{(folder.Root == "SoundWordsSlow" ? "_Slow" : "")}";
                AddressableAssetGroup group = settings.FindGroup(groupName) ??
                                              settings.CreateGroup(groupName, false, false, true, template.Schemas);

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress(folder.Path);
                changed = true;
                Debug.Log($"[{nameof(SoundFoldersPostprocessor)}] [{nameof(Register)}] {folder.Path} → {groupName}");
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}

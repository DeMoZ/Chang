using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Chang.Editor.Addressables
{
    public class AddressablesResourcesWindow : OdinEditorWindow
    {
        public class ToolConstants
        {
            public const string SOME_CONFIG_PATH = "Assets/Project/Scripts/Editor/Addressables/SomeConfig.asset";
        }

        [MenuItem("Chang/Content/Addressables/Addressables Resources Window")]
        private static void OpenWindow()
        {
            GetWindow<AddressablesResourcesWindow>().Show();
        }

        [PropertySpace(5f)]
        [FoldoutGroup("Cache")]
        [HorizontalGroup("Cache/Line1")]
        [InfoBox("Open downloaded asset bundles folder")]
        [Button(ButtonSizes.Medium, Name = "Open Cache"), GUIColor(1f, 1f, 1f)]
        public void OpenCacheFolder()
        {
            OpenFolder(GetCachePath());
        }

        [PropertySpace(5f)]
        [FoldoutGroup("Cache")]
        [HorizontalGroup("Cache/Line1")]
        [InfoBox("Open downloaded catalogs folder")]
        [Button(ButtonSizes.Medium, Name = "Open Catalog"), GUIColor(1f, 1f, 1f)]
        public void OpenCatalogFolder()
        {
            OpenFolder(GetCatalogPath());
        }

        [PropertySpace(5f)]
        [FoldoutGroup("Cache")]
        [HorizontalGroup("Cache/Line1")]
        [InfoBox("Delete downloaded asset bundles")]
        [Button(ButtonSizes.Medium, Name = "Clear Cache"), GUIColor(1f, 1f, 1f)]
        public void ClearCache()
        {
            if (!CanClear("asset bundles cache", GetCachePath()))
            {
                return;
            }

            if (Caching.ClearCache())
            {
                Debug.Log($"Asset bundles cache cleared: {GetCachePath()}");
            }
            else
            {
                Debug.LogError($"Asset bundles cache was not cleared, some bundles may be in use: {GetCachePath()}");
            }
        }

        [PropertySpace(5f)]
        [FoldoutGroup("Cache")]
        [HorizontalGroup("Cache/Line1")]
        [InfoBox("Delete downloaded catalogs")]
        [Button(ButtonSizes.Medium, Name = "Clear Catalog"), GUIColor(1f, 1f, 1f)]
        public void ClearCatalogFolder()
        {
            var path = GetCatalogPath();
            if (!CanClear("catalogs cache", path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Debug.Log($"Catalogs cache is already empty: {path}");
                return;
            }

            try
            {
                Directory.Delete(path, true);
                Debug.Log($"Catalogs cache cleared: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Catalogs cache was not cleared: {path}\n{e}");
            }
        }

        private static bool CanClear(string cacheName, string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError($"Can't clear {cacheName} in play mode, Addressables may be using it.");
                return false;
            }

            return EditorUtility.DisplayDialog("Clear Addressables cache", $"Delete {cacheName}?\n{path}", "Delete", "Cancel");
        }

        private static void OpenFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Debug.LogError($"Folder not found at path: {path}");
                return;
            }

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    Process.Start("open", $"\"{path}\"");
                    break;
                case RuntimePlatform.WindowsEditor:
                    Process.Start("explorer.exe", $"\"{path.Replace('/', '\\')}\"");
                    break;
                default:
                    EditorUtility.RevealInFinder(path);
                    break;
            }
        }

        [PropertySpace(5f)]
        [FoldoutGroup("Checkers")]
        [HorizontalGroup("Checkers/Line1")]
        [InfoBox("Check if no labels for assets")]
        [Button(ButtonSizes.Medium, Name = "Check Null Labbels"), GUIColor(1f, 1f, 1f)]
        public void CheckNullLabels()
        {
            CheckLabels();
        }

        [PropertySpace(5f)]
        [InfoBox("Open some config file")]
        [Button(ButtonSizes.Medium, Name = "Open some config file"), GUIColor(1f, 1f, 1f)]
        [FoldoutGroup("Config Tools")]
        [HorizontalGroup("Config Tools/Horizontal")]
        public void OpenSomeConfigFile()
        {
            Object configFile = AssetDatabase.LoadAssetAtPath<Object>(ToolConstants.SOME_CONFIG_PATH);
            if (configFile != null)
            {
                Selection.activeObject = configFile;
                EditorGUIUtility.PingObject(configFile);
            }
            else
            {
                Debug.LogError($"Config file not found at path: {ToolConstants.SOME_CONFIG_PATH}");
            }
        }

        private void RestartEditAssetDatabase()
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.StartAssetEditing();
        }

        private void StopEditAssetDatabase()
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static (int, int, int) MoveAsset<T>(
            string[] assetGuids,
            string fromPath,
            string toPath,
            string bundleGroup = null,
            string bundleLabel = null,
            List<string> ignoreAssets = null) where T : Object
        {
            (int, int, int) count = (0, 0, 0);

            AddressableAssetSettings settings = default;
            AddressableAssetGroup group = default;
            if (!string.IsNullOrEmpty(bundleGroup))
            {
                settings = AddressableAssetSettingsDefaultObject.Settings;
                group = settings.FindGroup(bundleGroup);
                if (group == null)
                {
                    Debug.LogError($"Bundle group does not exist: {bundleGroup}");
                    return count;
                }
            }

            foreach (string guid in assetGuids)
            {
                string fromAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                string nameAndExtension = Path.GetFileName(fromAssetPath);
                if (ignoreAssets != null && ignoreAssets.Contains(nameAndExtension))
                {
                    continue;
                }

                T asset = AssetDatabase.LoadAssetAtPath<T>(fromAssetPath);
                if (asset != null)
                {
                    string relativePath = fromAssetPath.Substring(fromPath.Length);
                    string toAssetPath = Path.Join(toPath, relativePath);

                    string result = AssetDatabase.MoveAsset(fromAssetPath, toAssetPath);
                    if (string.IsNullOrEmpty(result))
                    {
                        count.Item2++;
                        Debug.Log($"Asset moved {asset.name}\nfrom: {fromAssetPath}\nto: {toAssetPath}");
                    }
                    else
                    {
                        count.Item3++;
                        Debug.LogError($"Asset not moved {asset.name}\nfrom: {fromAssetPath}\nto: {toAssetPath};\nError: {result}");
                    }

                    if (!string.IsNullOrEmpty(bundleGroup))
                    {
                        AddAssetToGroup(toAssetPath, guid, settings, group, bundleLabel);
                    }

                    count.Item1++;
                }
            }

            return count;
        }

        private static void AddAssetToGroup(string assetPath, string guid, AddressableAssetSettings settings, AddressableAssetGroup group,
            string label)
        {
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            if (entry == null)
            {
                Debug.LogError($"Failed to move entry for asset at path: {assetPath}");
                return;
            }

            if (!string.IsNullOrEmpty(label))
            {
                entry.SetLabel(label, true, true);
            }
        }

        private static void CheckLabels()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            List<AddressableAssetGroup> groups = settings.groups;


            List<string> ignores = new() { "EditorSceneList", "Resources" };
            foreach (AddressableAssetGroup group in groups)
            {
                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry.labels.Count == 0 && !ignores.Contains(entry.address))
                    {
                        Debug.LogError($"No label for group: {group.Name}, Asset {entry.address}");
                    }
                }
            }

            Debug.LogWarning("Labels check finished;");
        }
        
        // downloaded remote asset bundles, the same folder is used by Addressables.ClearDependencyCacheAsync
        private static string GetCachePath()
        {
            return Caching.defaultCache.path;
        }

        // downloaded remote catalogs, see AddressablesImpl.kCacheDataFolder
        private static string GetCatalogPath()
        {
            return Path.Combine(Application.persistentDataPath, "com.unity.addressables");
        }
    }
}
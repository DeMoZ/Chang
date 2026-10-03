using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    public class SheetsToConfigMenu
    {
        private const string RootConfigPath = "Assets/Project/Configs/RootSheetsToConfig.asset";

        /// <summary>
        /// Selects the config with the buttons creating the configs from Google Sheets
        ///</summary>
        [MenuItem("Chang/Utilities/Sheets To Configs", false, 0)]
        public static void SelectRootSheetsToConfigs()
        {
            RootSheetsToConfig config = GetOrCreateRootSheetsToConfig();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
            Debug.Log($"Selected: {RootConfigPath}");
        }

        private static RootSheetsToConfig GetOrCreateRootSheetsToConfig()
        {
            RootSheetsToConfig config = AssetDatabase.LoadAssetAtPath<RootSheetsToConfig>(RootConfigPath);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<RootSheetsToConfig>();
                AssetDatabase.CreateAsset(config, RootConfigPath);
                AssetDatabase.SaveAssets();
                Debug.LogWarning($"Config didn't exist and was created at path: {RootConfigPath}");
            }

            return config;
        }
    }
}
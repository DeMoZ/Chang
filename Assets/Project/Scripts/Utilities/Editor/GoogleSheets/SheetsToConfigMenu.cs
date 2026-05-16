using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    public class SheetsToConfigMenu
    {
        private const string RootConfigPath = "Assets/Project/Configs/RootSheetsToConfig.asset";

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary Config", false, 0)]
        public static void CreateVocabularyConfig()
        {
            GetOrCreateRootSheetsToConfig().CreateVocabularyConfig();
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Vocabulary Book Config", false, 1)]
        public static void CreateVocabularyBookConfig()
        {
            GetOrCreateRootSheetsToConfig().CreateVocabularyBookConfig();
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Sentences Config", false, 2)]
        public static void CreateSentencesConfig()
        {
            GetOrCreateRootSheetsToConfig().CreateSentencesConfig();
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Create Sentences Book Config", false, 3)]
        public static void CreateSentencesBookConfig()
        {
            GetOrCreateRootSheetsToConfig().CreateSentencesBookConfig();
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates JSON files for each sheet.
        ///</summary>
        [MenuItem("Chang/Utilities/Select Root Sheets To Configs", false, 3)]
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
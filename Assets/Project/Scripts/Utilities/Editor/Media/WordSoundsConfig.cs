using Chang.Core;
using UnityEditor;
using UnityEngine;
using VocabularyConfig = Chang.Core.Vocabulary;

namespace Chang.Utilities.Media
{
    /// <summary>
    /// Settings of the word sounds voicing with the macOS voices, Siri voices included (macOS only).
    /// The inspector (<see cref="WordSoundsConfigEditor"/>) lists the words of the Vocabulary config and voices the checked ones.
    /// See Docs/content-pipeline.md#word-sounds.
    /// </summary>
    [CreateAssetMenu(menuName = "Chang/Utilities/Word Sounds Config", fileName = "WordSounds")]
    public class WordSoundsConfig : ScriptableObject
    {
        public const string ConfigPath = "Assets/Project/Configs/WordSounds.asset";
        private const string DefaultVocabulary = "Assets/Project/Resources_Bundled/BookConfigs/Thai/Vocabulary.asset";

        [Tooltip("BookConfigs/<Language>/Vocabulary.asset: download the configs from Google Sheets first to get new words")]
        public VocabularyConfig Vocabulary;

        public bool Male = true;
        public bool Female = true;
        [Tooltip("Voice identifier; empty = the Siri voice of the gender (Thai: Voice 1 male, Voice 2 female)")]
        public string MaleVoice;
        public string FemaleVoice;

        public bool Normal = true;
        public bool Slow;
        [Range(0.3f, 0.6f)] public float NormalRate = 0.5f;
        [Range(0.15f, 0.45f)] public float SlowRate = 0.32f;

        [Tooltip("Off: only the missing files of the checked words are voiced")]
        public bool Rewrite;

        [MenuItem("Chang/Utilities/Word Sounds (Siri)", false, 21)]
        private static void SelectConfig()
        {
            WordSoundsConfig config = Load();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        public static WordSoundsConfig Load()
        {
            WordSoundsConfig config = AssetDatabase.LoadAssetAtPath<WordSoundsConfig>(ConfigPath);
            if (config == null)
            {
                config = CreateInstance<WordSoundsConfig>();
                config.Vocabulary = AssetDatabase.LoadAssetAtPath<VocabularyConfig>(DefaultVocabulary);
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
            }

            return config;
        }
    }
}

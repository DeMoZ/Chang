using System.Collections.Generic;
using System.Linq;
using Chang;
using Chang.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates words configs from sheets jsons
/// </summary>
[CreateAssetMenu(fileName = "_LessonsToBooks", menuName = "Chang/GameBook/LessonsToBooks")]
public class LessonsToBooks : ScriptableObject
{
    [SerializeField, FolderPath] private List<string> _folders;

    [Button]
    private async void MakeLessonsToBooksInFolders()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var folder in _folders)
            {
                var configs = FindLessonsConfigsInFolder(folder);

                GameBookConfig gameBookConfig = CreateInstance<GameBookConfig>();
                gameBookConfig.Language = configs[0].Language;
                gameBookConfig.Section = configs[0].Section;
                gameBookConfig.Name = configs[0].Section;
                gameBookConfig.Lessons = new List<LessonConfig>();

                foreach (var config in configs)
                {
                    gameBookConfig.Lessons.Add(config);
                }
                
                ConfigFileCreator.CreateBookConfig(gameBookConfig);
                Debug.Log($"Book Config: {gameBookConfig.name} created");
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.LogWarning("--Books Configs Created--");
    }

    private List<LessonConfig> FindLessonsConfigsInFolder(string folder)
    {
        string[] assetsGuids = AssetDatabase.FindAssets("t: ScriptableObject", new[] { folder });
        Debug.Log($"assets {assetsGuids.Length} in folder:\n{folder}");

        List<LessonConfig> configs = new();
        foreach (string guid in assetsGuids)
        {
            string fromAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            LessonConfig asset = AssetDatabase.LoadAssetAtPath<LessonConfig>(fromAssetPath);
            if (asset != null)
            {
                configs.Add(asset);
            }
        }

        return configs;
    }
}
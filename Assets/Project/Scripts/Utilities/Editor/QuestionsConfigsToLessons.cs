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
[CreateAssetMenu(fileName = "_QuestionsConfigsToLessons", menuName = "Chang/GameBook/QuestionsConfigsToLessons")]
public class QuestionsConfigsToLessons : ScriptableObject
{
    [SerializeField, FolderPath] private List<string> _folders;

    [Button]
    private async void MakeQuestionsConfigsToLessonsInFolders()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var folder in _folders)
            {
                var configs = FindQuestConfigsInFolder(folder);

                GetHashes(configs, out var hashes, out var configsDict);

                hashes.Sort();

                List<List<int>> groupedHashes = GetGroupedHashes(hashes);

                // create quests from grouped hashes
                int countSectionLessons = 0;
                string section;
                foreach (var groupedHash in groupedHashes)
                {
                    var firstHash = groupedHash[0];
                    section = configsDict[firstHash].Section;
                    countSectionLessons++;
                    LessonConfig lessonConfig = CreateInstance<LessonConfig>();
                    lessonConfig.GenerateQuestMatchWordsData = true;
                    lessonConfig.Language = configsDict[firstHash].Language;
                    lessonConfig.Name = $"{configsDict[firstHash].Section}_{countSectionLessons}";
                    lessonConfig.Section = section;
                    lessonConfig.Questions = new List<QuestionConfig>();

                    foreach (var hash in groupedHash)
                    {
                        // Create lesson config asset.
                        lessonConfig.Questions.Add(configsDict[hash]);
                    }

                    ConfigFileCreator.CreateLessonConfig(section, lessonConfig);
                    Debug.Log($"Lesson Config: {lessonConfig.name} created");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.LogWarning("--Lesson Configs Created--");
    }

    private void GetHashes(List<QuestionConfig> configs, out List<int> hashes, out Dictionary<int, QuestionConfig> configsDict)
    {
        hashes = new();
        configsDict = new();
        foreach (var config in configs)
        {
            int hash = config.name.GetHashCode();
            hashes.Add(hash);
            configsDict.Add(hash, config);
        }
    }

    private List<QuestionConfig> FindQuestConfigsInFolder(string folder)
    {
        string[] assetsGuids = AssetDatabase.FindAssets("t: ScriptableObject", new[] { folder });
        Debug.Log($"assets {assetsGuids.Length} in folder:\n{folder}");

        List<QuestionConfig> configs = new();
        foreach (string guid in assetsGuids)
        {
            string fromAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            QuestionConfig asset = AssetDatabase.LoadAssetAtPath<QuestionConfig>(fromAssetPath);
            if (asset != null)
            {
                configs.Add(asset);
            }
        }

        return configs;
    }

    private List<List<int>> GetGroupedHashes(List<int> hashes)
    {
        List<List<int>> result = new();
        for (int i = 0; i < hashes.Count; i += 4)
        {
            List<int> group = hashes.GetRange(i, Mathf.Min(4, hashes.Count - i));

            if (group.Count < 2 && result.Count > 0)
            {
                result[^1].AddRange(group);
            }
            else
            {
                result.Add(group);
            }
        }

        // check grouping
        foreach (var group in result)
        {
            Debug.Log($"group count: {group.Count}");
        }

        return result;
    }
}
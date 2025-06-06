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
[CreateAssetMenu(fileName = "_WordsConfigsToQuestions", menuName = "Chang/GameBook/WordsConfigsToQuestions")]
public class WordsConfigsToQuestions : ScriptableObject
{
    [SerializeField, FolderPath] private List<string> _folders;

    [Button]
    private void MakeQuestsWithWordsInFolders()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var folder in _folders)
            {
                var configs = FindPhraseConfigsInFolder(folder);

                GetHashes(configs, out var hashes, out var configsDict);

                hashes.Sort();

                List<List<int>> groupedHashes = GetGroupedHashes(hashes);

                // create quests from grouped hashes
                foreach (var groupedHash in groupedHashes)
                {
                    foreach (var hash in groupedHash)
                    {
                        // Create question config asset.
                        QuestionConfig questionConfig = CreateInstance<QuestionConfig>();
                        QuestSelectWord questData = new QuestSelectWord();

                        var mixHashes = groupedHash.FindAll(h => h != hash);
                        var mixConfigs = mixHashes.Select(h => configsDict[h]).ToList();

                        var correctWord = configsDict[hash];
                        questData.CorrectWord = correctWord;
                        questData.MixWords = mixConfigs;

                        questionConfig.SetLanguage(correctWord.Language);
                        questionConfig.SetSection(correctWord.Section);
                        questionConfig.SetQuestionType(QuestionType.SelectWord);
                        questionConfig.SetQuestionData(questData);

                        ConfigFileCreator.CreateQuestSelectWordConfig(correctWord.Key, questionConfig);
                        Debug.Log($"Quest Config: {questionConfig.name} created.");
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.LogWarning("--Quest Configs Created--");
    }

    private void GetHashes(List<PhraseConfig> configs, out List<int> hashes, out Dictionary<int, PhraseConfig> configsDict)
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

    private List<PhraseConfig> FindPhraseConfigsInFolder(string folder)
    {
        string[] assetsGuids = AssetDatabase.FindAssets("t: ScriptableObject", new[] { folder });
        Debug.Log($"assets {assetsGuids.Length} in folder:\n{folder}");

        List<PhraseConfig> configs = new();
        foreach (string guid in assetsGuids)
        {
            string fromAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            PhraseConfig asset = AssetDatabase.LoadAssetAtPath<PhraseConfig>(fromAssetPath);
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
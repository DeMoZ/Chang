using System;
using System.Collections.Generic;
using System.Linq;
using Chang;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PromptsBank", menuName = "Chang/PromptsBank")]
public class PromptsBank : ScriptableObject
{
    [SerializeField, FolderPath, VerticalGroup("Folders")]
    private List<string> _folders;

    [SerializeField] private List<PromptItem> _promptItems;

    [Button, VerticalGroup("Folders")]
    private void PrepareWithWordsInFolders()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            Dictionary<string, List<WordConfig>> wordDict = new();
            foreach (var folder in _folders)
            {
                List<WordConfig> wordConfigs = FindPhraseConfigsInFolder(folder).Select(c => c.Word).ToList();

                if (wordConfigs.Count > 0)
                {
                    string section = wordConfigs[0].Section;
                    wordDict[section] = wordConfigs;
                }
                else
                {
                    Debug.LogWarning($"No word configs found in folder: {folder}");
                }
            }

            _promptItems ??= new();

            foreach (var pair in wordDict)
            {
                PromptItem prompt;
                var promptItem = _promptItems.FirstOrDefault(p => p.Section == pair.Key);

                if (promptItem == null)
                {
                    prompt = new PromptItem();
                    prompt.Section = pair.Key;
                    _promptItems.Add(prompt);
                }
                else
                {
                    prompt = promptItem;
                }

                prompt.Words = pair.Value
                    .Select(w => w.Meanings.FirstOrDefault(m => m.Language == Languages.English))
                    .Where(m => m != null && !string.IsNullOrWhiteSpace(m.Meaning))
                    .Select(m => new WordEntry { Value = m.Meaning, Owner = prompt })
                    .ToList();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while processing folders: {e.Message}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
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
}

[Serializable]
public class PromptItem
{
    [HideLabel] public string Section;
    [HideLabel, Multiline(7)] public string Text;

    public List<WordEntry> Words;

    public void MakePrompt(int index)
    {
        if (string.IsNullOrEmpty(Section) || string.IsNullOrEmpty(Text) || Words == null || Words.Count == 0)
        {
            Debug.LogWarning(
                $"Prompt {index} is not valid. Section: {Section}, Text: {Text}, Words count: {Words?.Count}");
            return;
        }

        var prompt = Text.Replace("****", Words[index].Value);
        GUIUtility.systemCopyBuffer = prompt;
        Debug.Log($"Prompt {index} created with Section: {Section}, Word: {Words[index].Value}, Text:\n{prompt}");
    }

    public void OnValidate()
    {
        if (Words == null) return;
        for (int i = 0; i < Words.Count; i++)
        {
            if (Words[i] != null) Words[i].Owner = this;
        }
    }
}

[Serializable]
public class WordEntry
{
    [HideInInspector] public string Value;
    [NonSerialized] public PromptItem Owner;

#if UNITY_EDITOR
    [OnInspectorGUI, PropertyOrder(-1)]
    private void DrawInline()
    {
        EditorGUILayout.BeginHorizontal();
        var buttonContent = new GUIContent("Do", "Make a prompt for this word");
        if (GUILayout.Button(buttonContent, GUILayout.Width(40)))
        {
            if (Owner?.Words != null)
            {
                int idx = Owner.Words.IndexOf(this);
                if (idx >= 0) Owner.MakePrompt(idx);
            }
        }

        Value = EditorGUILayout.TextField(Value);
        EditorGUILayout.EndHorizontal();
    }
#endif
}
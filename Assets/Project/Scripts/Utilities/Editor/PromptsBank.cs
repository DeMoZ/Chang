using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PromptsBank", menuName = "Chang/PromptsBank")]
public class PromptsBank : ScriptableObject
{
    [SerializeField, FolderPath] private string CreateImagesPath;

    [SerializeField, FolderPath, VerticalGroup("Folders")]
    private List<string> _folders;

    [SerializeField, TableList] private List<PromptItem> _promptItems;

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
                    _promptItems.Add(prompt);
                }
                else
                {
                    prompt = promptItem;
                }

                prompt.Section = pair.Key;
                prompt.CreateImagesPath = CreateImagesPath;

                prompt.Words = pair.Value
                    .Select(w => new
                    {
                        Word = w,
                        Meaning = w.Meanings.FirstOrDefault(m => m.Language == Languages.English)
                    })
                    .Where(x => x.Meaning != null && !string.IsNullOrWhiteSpace(x.Meaning.Meaning))
                    .Select(x => new WordEntry
                    {
                        Value = x.Meaning.Meaning,
                        Owner = prompt,
                        Name = x.Word.Key
                    })
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
    [TableColumnWidth(100, Resizable = false)] [HideLabel, VerticalGroup("Section")]
    public string Section;

    [HideLabel, Multiline(4)] public string Text;

    [HideInInspector] public string CreateImagesPath;

    public List<WordEntry> Words;

    [Button, VerticalGroup("Section")]
    public void MakeImages()
    {
        Debug.Log($"Section: {Section}");
        string appPath = Application.dataPath.Replace("Assets", string.Empty);
        List<string> files = new ();
        AssetDatabase.StartAssetEditing();
        
        try
        {
            if (Words == null || Words.Count == 0 || string.IsNullOrEmpty(CreateImagesPath))
            {
                Debug.LogWarning(
                    $"Cannot create images for section {Section}. Words count: {Words?.Count}, CreateImagesPath: {CreateImagesPath}");
                return;
            }

            // create a folder for the section if it doesn't exist
            string folderPath = $"{CreateImagesPath}/{Section}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(CreateImagesPath, Section);
            }

            for (int i = 0; i < Words.Count; i++)
            {
                var name = Words[i].Name;
                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogWarning($"Word {i} in section {Section} has no name. Skipping.");
                    continue;
                }

                string filePath = $"{folderPath}/{name}";
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(filePath) != null)
                {
                    Debug.Log($"File {filePath} already exists. Skipping.");
                    continue;
                }

                Texture2D texture = new Texture2D(1024, 1024);
                byte[] textureData = texture.EncodeToPNG();

                string fullPath = Path.Combine(appPath, filePath + ".png");
                File.WriteAllBytes(fullPath, textureData);
                files.Add(filePath);
                MonoBehaviour.DestroyImmediate(texture);
            }

            Debug.Log($"Section: {Section} files created: {Words.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            foreach (var file in files)
            {
                AssetDatabase.ImportAsset(file);
            }
            
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
    }

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

    public void MakeName(int index)
    {
        if (string.IsNullOrEmpty(Section) || string.IsNullOrEmpty(Text) || Words == null || Words.Count == 0)
        {
            Debug.LogWarning(
                $"Prompt {index} is not valid. Section: {Section}, Text: {Text}, Words count: {Words?.Count}");
            return;
        }

        string name = Words[index].Name;
        GUIUtility.systemCopyBuffer = name;
        Debug.Log($"Name {index} created with Section: {Section}, Word: {Words[index].Value}, Name:\n{name}");
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
    [HideInInspector] public string Name;
    [NonSerialized] public PromptItem Owner;

#if UNITY_EDITOR
    [OnInspectorGUI, PropertyOrder(-1)]
    private void DrawInline()
    {
        EditorGUILayout.BeginHorizontal();
        var buttonContent = new GUIContent("Prompt", "Make a prompt for this word");
        if (GUILayout.Button(buttonContent, GUILayout.Width(52)))
        {
            if (Owner?.Words != null)
            {
                int idx = Owner.Words.IndexOf(this);
                if (idx >= 0) Owner.MakePrompt(idx);
            }
        }

        buttonContent = new GUIContent("Name", "Make a file name for this word");
        if (GUILayout.Button(buttonContent, GUILayout.Width(45)))
        {
            if (Owner?.Words != null)
            {
                int idx = Owner.Words.IndexOf(this);
                if (idx >= 0) Owner.MakeName(idx);
            }
        }

        Value = EditorGUILayout.TextField(Value);
        Name = EditorGUILayout.TextField(Name);
        EditorGUILayout.EndHorizontal();
    }
#endif
}
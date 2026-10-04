using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang;
using TriInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PromptsBank", menuName = "Chang/PromptsBank")]
[DeclareVerticalGroup("Folders")]
public class PromptsBank : ScriptableObject
{
    [SerializeField, FolderPath] private string CreateImagesPath;

    [SerializeField, FolderPath, Group("Folders")]
    private List<string> _folders;

    // [SerializeField, TableList] private List<PromptItem> _promptItems;

    [Button, Group("Folders")]
    private void PrepareWithWordsInFolders()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            /*
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
                        Name = x.Word.Key
                    })
                    .ToList();
            }
            */
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

    // private List<PhraseConfig> FindPhraseConfigsInFolder(string folder)
    // {
    //     string[] assetsGuids = AssetDatabase.FindAssets("t: ScriptableObject", new[] { folder });
    //     Debug.Log($"assets {assetsGuids.Length} in folder:\n{folder}");
    //
    //     List<PhraseConfig> configs = new();
    //     foreach (string guid in assetsGuids)
    //     {
    //         string fromAssetPath = AssetDatabase.GUIDToAssetPath(guid);
    //         PhraseConfig asset = AssetDatabase.LoadAssetAtPath<PhraseConfig>(fromAssetPath);
    //         if (asset != null)
    //         {
    //             configs.Add(asset);
    //         }
    //     }
    //
    //     return configs;
    // }
}

[Serializable]
[DeclareVerticalGroup("Section")]
public class PromptItem
{
    [HideLabel, Group("Section")]
    public string Section;

    [HideLabel, Multiline(4)] public string Text;

    [HideInInspector] public string CreateImagesPath;

    public List<WordEntry> Words;

    [Button, Group("Section")]
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
            AssetDatabase.Refresh();
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
}

[Serializable]
public class WordEntry
{
    public string Value;
    public string Name;
}

/// <summary>
/// Draws a word in one line with buttons that copy its prompt or file name.
/// </summary>
[CustomPropertyDrawer(typeof(WordEntry))]
public class WordEntryDrawer : PropertyDrawer
{
    private const string WordsPath = ".Words.Array.data[";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty value = property.FindPropertyRelative(nameof(WordEntry.Value));
        SerializedProperty name = property.FindPropertyRelative(nameof(WordEntry.Name));

        Rect promptRect = new(position.x, position.y, 52, position.height);
        Rect nameRect = new(promptRect.xMax + 2, position.y, 45, position.height);
        float fieldWidth = (position.xMax - nameRect.xMax - 4) / 2;
        Rect valueFieldRect = new(nameRect.xMax + 2, position.y, fieldWidth, position.height);
        Rect nameFieldRect = new(valueFieldRect.xMax + 2, position.y, fieldWidth, position.height);

        if (GUI.Button(promptRect, new GUIContent("Prompt", "Make a prompt for this word")))
        {
            if (TryGetOwner(property, out PromptItem owner, out int index)) owner.MakePrompt(index);
        }

        if (GUI.Button(nameRect, new GUIContent("Name", "Make a file name for this word")))
        {
            if (TryGetOwner(property, out PromptItem owner, out int index)) owner.MakeName(index);
        }

        EditorGUI.PropertyField(valueFieldRect, value, GUIContent.none);
        EditorGUI.PropertyField(nameFieldRect, name, GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    // the word is an element of PromptItem.Words, the owner is found by the property path
    private static bool TryGetOwner(SerializedProperty property, out PromptItem owner, out int index)
    {
        owner = null;
        index = -1;

        string path = property.propertyPath;
        int wordsIndex = path.LastIndexOf(WordsPath, StringComparison.Ordinal);
        if (wordsIndex < 0)
        {
            return false;
        }

        string indexString = path.Substring(wordsIndex + WordsPath.Length).TrimEnd(']');
        SerializedProperty ownerProperty = property.serializedObject.FindProperty(path.Substring(0, wordsIndex));
        owner = ownerProperty?.boxedValue as PromptItem;
        return owner != null && int.TryParse(indexString, out index);
    }
}

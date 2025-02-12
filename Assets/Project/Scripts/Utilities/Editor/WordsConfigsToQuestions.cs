using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Creates words configs from sheets jsons
/// </summary>
[CreateAssetMenu(fileName = "_WordsConfigsToQuestions", menuName = "Chang/GameBook/WordsConfigsToQuestions")]
public class WordsConfigsToQuestions : ScriptableObject
{
    // public List<TextAsset> SheetsJsons;
    [FolderPath] public List<string> Folders;

    [Button]
    private async void MakeWordsFromJsons()
    {
        // foreach (var textAsset in SheetsJsons)
        // {
        //     var json = textAsset.text;
        //     var sheet = JsonConvert.DeserializeObject<Sheet>(json);
        //
        //     await WordConfigFileCreator.CreateWordConfigsFromSheet(sheet);
        // }
        
        Debug.LogWarning("--Quest Configs Created--");
    }
}
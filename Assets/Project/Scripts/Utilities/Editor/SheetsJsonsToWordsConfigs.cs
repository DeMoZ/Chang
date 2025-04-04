using System.Collections.Generic;
using Chang.Utilities;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Creates words configs from sheets jsons
/// </summary>
[CreateAssetMenu(fileName = "_SheetsJsonsToWordsConfigs", menuName = "Chang/GameBook/SheetsJsonsToWordsConfigs")]
public class SheetsJsonsToWordsConfigs : ScriptableObject
{
    public List<TextAsset> SheetsJsons;

    [Button]
    private void MakeWordsFromJsons()
    {
        foreach (var textAsset in SheetsJsons)
        {
            var json = textAsset.text;
            var sheet = JsonConvert.DeserializeObject<Sheet>(json);

            ConfigFileCreator.CreateWordConfigsFromSheet(sheet);
        }
        
        Debug.LogWarning("--Word Configs Created--");
    }
}
using System.Collections.Generic;
using Chang.Utilities;
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
            // Sheet sheet = 
            //
            // WordConfigFileCreator.CreateWordsConfigs(sheet);
        }
    }
}

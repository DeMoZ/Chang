using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class WordConfigFileCreator
{
    private static string RelativePath = "Project/Languages";
    private static string AssetsFolder = "Assets";
    private static string WordsFolder = "Words";
    private static string NewFolder = "New";

    public static void Create(Languages language, string name, PhraseData phraseData, bool withDirtyAndSafe = false)
    {

        var meanings = new List<Translation>
        {
            new Translation()
            {
                Language = Languages.English,
                Meaning = phraseData.Meaning
            }
        };

        var word = new WordConfig
        {
            EngWord = name,
            Word = phraseData.Word,
            Phonetic = phraseData.Phonetic,
            Meanings = meanings
        };

        var dataAsset = ScriptableObject.CreateInstance<PhraseConfig>();
        if (withDirtyAndSafe)
            EditorUtility.SetDirty(dataAsset);

        dataAsset.Key = phraseData.Key;
        dataAsset.Language = phraseData.Language;
        dataAsset.AudioClip = phraseData.AudioClip;
        dataAsset.Word = word;
        //phraseConfig.Phrase = todo roman but maybe not needed

        var relativePath = Path.Combine(RelativePath, language.ToString(), WordsFolder, NewFolder, $"{name}.asset");
        var assetRelativePath = Path.Combine(AssetsFolder, relativePath);

        var path = Path.Combine(Application.dataPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        AssetDatabase.CreateAsset(dataAsset, assetRelativePath);

        if (withDirtyAndSafe)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
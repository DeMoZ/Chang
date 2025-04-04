using System.Collections.Generic;
using System.IO;
using Chang;
using Chang.Utilities;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;

/// <summary>
/// Creates words configs from sheets jsons
/// </summary>
[CreateAssetMenu(fileName = "_ConfigsToSpeech", menuName = "Chang/GameBook/ConfigsToSpeech")]
public class ConfigToSpeech : ScriptableObject
{
    [SerializeField] private string text;
    [SerializeField, FolderPath] private List<string> _folders;

    private TextToSpeechService _textToSpeechService;

    private TextToSpeechService TextToSpeechService => _textToSpeechService ??= new TextToSpeechService();

    [Button]
    public void AudioTextTest()
    {
        AudioTextAsync(text).Forget();
    }

    [Button]
    public async void AudioTextFolders()
    {
        AssetDatabase.StartAssetEditing();

        try
        {
            foreach (var folder in _folders)
            {
                var configs = FindConfigsInFolder(folder);

                foreach (var config in configs)
                {
                    var audio = await TextToSpeechService.GetAudioAsync(config.Word.Word);
                    if (audio == null)
                    {
                        continue;
                    }

                    string path = ConfigFileCreator.GetSoundWordsSystemFilePath(config.Language, config.Section);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    
                    string pathName = Path.Combine(path, config.Word.Key + ".mp3");
                    byte[] audioBytes = System.Convert.FromBase64String(audio.audioContent);
                    File.WriteAllBytes(pathName, audioBytes);
                    Debug.Log($"Sound for config: {config.Word.Key} created");

                    await UniTask.WaitForSeconds(2);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.LogWarning("--Sounds Created--");
    }

    private async UniTaskVoid AudioTextAsync(string value)
    {
        await TextToSpeechService.GetAudioAsync(value);
    }
    
    private List<PhraseConfig> FindConfigsInFolder(string folder)
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

// Класс для парсинга JSON-ответа
[System.Serializable]
public class AudioContent
{
    public string audioContent; // Base64-кодированный аудиофайл
}
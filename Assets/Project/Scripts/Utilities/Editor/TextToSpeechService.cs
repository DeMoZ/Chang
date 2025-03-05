using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeechService : IDisposable
{
    private class IpiData
    {
        public string speechUrl;
        public string speechApiKey;
    }

    public class SynthesisInput
    {
        public string text { get; set; }
    }

    public class VoiceSelectionParams
    {
        public string languageCode { get; set; }
        public string name { get; set; }
        public string ssmlGender { get; set; }
    }

    public class AudioConfig
    {
        public string audioEncoding { get; set; }
    }

    public class SpeechRequest
    {
        public SynthesisInput input { get; set; }
        public VoiceSelectionParams voice { get; set; }
        public AudioConfig audioConfig { get; set; }
    }

    private const string ApiFileName = "api_textToSpeech.json";
    private const string LanguageCode = "th-TH"; 
    private const string VoiceName = "ru-RU-Wavenet-A";
    // private const string VoiceName = "th-TH-Wavenet-A";

    private SsmlVoiceGender _ssmlVoiceGender = SsmlVoiceGender.Female;
    private bool _isInitialized;
    private IpiData _apiData;

    public void Dispose()
    {
    }

    private async UniTask Initialize()
    {
        try
        {
            string apiFullPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath, ApiFileName);
            await using var idsStream = new FileStream(apiFullPath, FileMode.Open, FileAccess.Read);
            var apiString = await new StreamReader(idsStream).ReadToEndAsync();
            _apiData = JsonUtility.FromJson<IpiData>(apiString);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    private async UniTask<bool> CheckInit()
    {
        if (!_isInitialized)
        {
            try
            {
                await Initialize();
                _isInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Initialization failed: {e.Message}");
                return false;
            }
        }

        return true;
    }

    public async UniTask<AudioContent> GetAudioAsync(string text)
    {
        var isInitialized = await CheckInit();

        if (!isInitialized)
        {
            Debug.LogError("CheckInit failed.");
            return null;
        }

        var jsonBody = CreateJsonBody(text);

        using UnityWebRequest request = new UnityWebRequest($"{_apiData.speechUrl}?key={_apiData.speechApiKey}", "POST");
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"Send request for text: {text}");
        await request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError( $"Error request for text: {text}; error:\n{request.error}");
            return null;
        }
        
        Debug.Log($"Request success for text: {text}");
        
        string jsonResponse = request.downloadHandler.text;
        return JsonUtility.FromJson<AudioContent>(jsonResponse);
    }

    /*
        string jsonBody = "{\"input\":{\"text\":\"" + text + "\"}," +
                          "\"voice\":{\"languageCode\":\"ru-RU\",\"name\":\"ru-RU-Wavenet-A\",\"ssmlGender\":\"FEMALE\"}," +
                          "\"audioConfig\":{\"audioEncoding\":\"MP3\"}}";

     */
    private string CreateJsonBody(string text)
    {
        var request = new SpeechRequest
        {
            input = new SynthesisInput { text = text },
            voice = new VoiceSelectionParams
            {
                languageCode = LanguageCode,
                name = VoiceName,
                ssmlGender = _ssmlVoiceGender.ToString()
            },
            audioConfig = new AudioConfig
            {
                audioEncoding = "MP3"
            }
        };
        return JsonConvert.SerializeObject(request);
    }
}
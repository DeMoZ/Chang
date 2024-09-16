using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;
using Google.Apis.Auth.OAuth2;
using UnityEngine;

public class TextToVoice
{
    public async Task<bool> TryGetAudioAsync(string text, string languageCode, string filePath)
    {
        var credentialPath = Path.Combine(Application.dataPath, VocabularyUtilitiesConstants.RelativePath,
            VocabularyUtilitiesConstants.SecretFileName);
        
        GoogleCredential googleCredential;
        await using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
        {
            googleCredential = GoogleCredential.FromStream(stream).CreateScoped(TextToSpeechClient.DefaultScopes);
        }

        var client = await new TextToSpeechClientBuilder { Credential = googleCredential }
            .BuildAsync();

        var input = new SynthesisInput { Text = text };
        var audioConfig = new AudioConfig { AudioEncoding = AudioEncoding.Mp3 };

        var voiceSelection = new VoiceSelectionParams
        {
            LanguageCode = languageCode,
            SsmlGender = SsmlVoiceGender.Female
        };

        var response = await client.SynthesizeSpeechAsync(input, voiceSelection, audioConfig);

        await using (var output = File.Create(filePath))
        {
            response.AudioContent.WriteTo(output);
            Console.WriteLine($"audio saved as {filePath}");
        }
        
        return true;
    }
}
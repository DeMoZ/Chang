using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities
{
    /// <summary>
    /// Generates word sounds with Google Text-To-Speech for words from the Vocabulary config
    /// that don't have a sound file at SoundWords/{Language}/{Section}/{Key}.mp3 yet.
    /// </summary>
    public static class WordSoundsGenerator
    {
        private const float RequestDelaySeconds = 0.5f;
        private const int MaxFailedInRow = 3;

        private static readonly WordPathHelper PathHelper = new();

        [MenuItem("Chang/Utilities/Word Sounds/Log Missing Sounds", false, 20)]
        public static void LogMissingSounds()
        {
            List<Core.Word> missing = GetWordsWithoutSound();
            string list = string.Join("\n", missing.Select(w => $"{w.SoundKey} [{w.LearnWord}]"));
            Debug.Log($"Words without sound: {missing.Count}\n{list}");
        }

        [MenuItem("Chang/Utilities/Word Sounds/Generate Missing Sounds", false, 21)]
        public static void GenerateMissingSounds()
        {
            GenerateAsync(GetWordsWithoutSound()).Forget();
        }

        private static List<Core.Word> GetWordsWithoutSound()
        {
            List<Core.Word> missing = new();

            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                string vocabularyPath = AssetPaths.Addressables.VocabularyPath(language);
                Core.Vocabulary vocabulary = AssetDatabase.LoadAssetAtPath<Core.Vocabulary>(vocabularyPath);
                if (vocabulary == null)
                {
                    continue;
                }

                missing.AddRange(vocabulary.Words.Where(word =>
                    !string.IsNullOrWhiteSpace(word.LearnWord) &&
                    !File.Exists(PathHelper.GetSoundPath(word.SoundKey))));
            }

            return missing;
        }

        private static async UniTaskVoid GenerateAsync(List<Core.Word> words)
        {
            Debug.Log($"Generating sounds for {words.Count} words");

            using TextToSpeechService textToSpeech = new();
            int created = 0;
            int failedInRow = 0;

            try
            {
                for (int i = 0; i < words.Count; i++)
                {
                    Core.Word word = words[i];
                    string path = PathHelper.GetSoundPath(word.SoundKey);

                    if (EditorUtility.DisplayCancelableProgressBar("Word Sounds", $"{word.SoundKey} [{word.LearnWord}]",
                            (float)i / words.Count))
                    {
                        Debug.LogWarning("Sounds generation canceled");
                        break;
                    }

                    var audio = await textToSpeech.GetAudioAsync(word.LearnWord);
                    if (string.IsNullOrEmpty(audio?.audioContent))
                    {
                        Debug.LogError($"No audio for {word.SoundKey} [{word.LearnWord}]");
                        if (++failedInRow >= MaxFailedInRow)
                        {
                            Debug.LogError($"Sounds generation stopped after {MaxFailedInRow} failed requests in a row");
                            break;
                        }

                        continue;
                    }

                    failedInRow = 0;

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    await File.WriteAllBytesAsync(path, Convert.FromBase64String(audio.audioContent));
                    created++;
                    Debug.Log($"Sound created: {path}");

                    await UniTask.WaitForSeconds(RequestDelaySeconds);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            Debug.Log($"Sounds created: {created}/{words.Count}");
        }
    }
}

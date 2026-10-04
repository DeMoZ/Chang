using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Chang.Resources;
using Cysharp.Threading.Tasks;
using Assets.SimpleLocalization.Scripts;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.Media
{
    /// <summary>
    /// Generates the word pictures and sounds of the <see cref="MediaPromptsConfig"/>.
    /// Picture: Claude Code CLI writes Tools/WordImages/gen/auto/{Category}/{Key}.py from the prompt,
    /// render.py and install.py put the PNG to ImageWords/{Language}/{Category}/{Key}.png.
    /// Sound: Google Text-To-Speech or macOS say, ffmpeg trims the silence, SoundWords/{Language}/{Voice}/{Section}/{Key}.mp3.
    /// </summary>
    public static class MediaGenerator
    {
        private const string Title = "Media Generation";
        private const string ToolsFolder = "Tools/WordImages";
        private const string SoundGroupTemplate = "Remote_Thai_Sound_Words";
        private const int MaxFailedSoundsInRow = 3;

        // word sounds must have no silence around, sentences are assembled from them
        private const string TrimSilence =
            "silenceremove=start_periods=1:start_threshold=-40dB:start_silence=0.02:detection=rms:window=0.02,areverse";

        private static readonly WordPathHelper PathHelper = new();
        private static bool _isRunning;

        private enum ImageMode
        {
            Draw,
            Render,
        }

        private class SoundJob
        {
            public MediaPromptItem Item;
            public LanguageVoices Language;
            public SoundVoices Voice;
            public string Path;
        }

        /// <param name="confirm">false runs without the confirmation dialog, e.g. from a script</param>
        public static void GenerateAllMissing(MediaPromptsConfig config, bool confirm = true)
        {
            List<MediaPromptItem> images = config.Items.Where(item => !HasImage(item)).ToList();
            List<SoundJob> sounds = config.Items.SelectMany(item => GetSoundJobs(config, item)).Where(job => !File.Exists(job.Path)).ToList();

            if (confirm && !EditorUtility.DisplayDialog(Title,
                    $"Pictures to draw: {images.Count}\nSounds to voice: {sounds.Count}", "Generate", "Cancel"))
            {
                return;
            }

            RunAsync(config, images.Select(item => (item, GetImageMode(item))).ToList(), sounds).Forget();
        }

        public static void GenerateImage(MediaPromptsConfig config, MediaPromptItem item)
        {
            ImageMode mode = ImageMode.Draw;
            if (File.Exists(GetSvgPath(item)))
            {
                int choice = EditorUtility.DisplayDialogComplex(Title, $"{item.WordKey} already has an SVG picture.",
                    "Render the SVG", "Cancel", "Draw again with Claude");
                if (choice == 1)
                {
                    return;
                }

                mode = choice == 0 ? ImageMode.Render : ImageMode.Draw;
            }

            RunAsync(config, new List<(MediaPromptItem, ImageMode)> { (item, mode) }, new List<SoundJob>()).Forget();
        }

        public static void GenerateSound(MediaPromptsConfig config, MediaPromptItem item)
        {
            List<SoundJob> sounds = GetSoundJobs(config, item).ToList();
            if (sounds.Count == 0)
            {
                Debug.LogWarning($"[{nameof(MediaGenerator)}] No language or voice is selected");
                return;
            }

            RunAsync(config, new List<(MediaPromptItem, ImageMode)>(), sounds).Forget();
        }

        public static void LogMissing(MediaPromptsConfig config)
        {
            List<MediaPromptItem> images = config.Items.Where(item => !HasImage(item)).ToList();
            List<SoundJob> sounds = config.Items.SelectMany(item => GetSoundJobs(config, item)).Where(job => !File.Exists(job.Path)).ToList();
            List<MediaPromptItem> noPrompt = images.Where(item => string.IsNullOrWhiteSpace(item.Prompt)).ToList();

            Debug.Log($"[{nameof(MediaGenerator)}] Words without a picture: {images.Count}\n" +
                      string.Join("\n", images.Select(item => $"{item.ImageKey} [{GetImageMode(item)}]")));
            Debug.Log($"[{nameof(MediaGenerator)}] Missing sounds: {sounds.Count}\n" +
                      string.Join("\n", sounds.Select(job => job.Path)));

            if (noPrompt.Count > 0)
            {
                Debug.LogWarning($"[{nameof(MediaGenerator)}] Words without a picture and a prompt: {noPrompt.Count}\n" +
                                 string.Join("\n", noPrompt.Select(item => item.WordKey)));
            }
        }

        public static string GetStatus(MediaPromptsConfig config, MediaPromptItem item)
        {
            List<SoundJob> sounds = GetSoundJobs(config, item).ToList();
            int existingSounds = sounds.Count(job => File.Exists(job.Path));
            string image = HasImage(item) ? "yes" : File.Exists(GetSvgPath(item)) ? "SVG only" : "no";
            return $"Picture: {image}; Sounds: {existingSounds}/{sounds.Count}";
        }

        private static async UniTaskVoid RunAsync(MediaPromptsConfig config, List<(MediaPromptItem item, ImageMode mode)> images,
            List<SoundJob> sounds)
        {
            if (_isRunning)
            {
                Debug.LogWarning($"[{nameof(MediaGenerator)}] The generation is already running");
                return;
            }

            _isRunning = true;
            bool canceled = false;
            int total = images.Count + sounds.Count;
            int done = 0;

            try
            {
                for (int i = 0; i < images.Count && !canceled; i++, done++)
                {
                    string info = $"Picture {i + 1}/{images.Count}: {images[i].item.ImageKey}";
                    canceled = !await GenerateImageAsync(config, images[i].item, images[i].mode,
                        () => EditorUtility.DisplayCancelableProgressBar(Title, info, (float)done / total));
                }

                if (!canceled && sounds.Count > 0)
                {
                    canceled = !await GenerateSoundsAsync(config, sounds, index =>
                        EditorUtility.DisplayCancelableProgressBar(Title, $"Sound {index + 1}/{sounds.Count}: {sounds[index].Path}",
                            (float)(done + index) / total));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(MediaGenerator)}] {e}");
            }
            finally
            {
                _isRunning = false;
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            if (sounds.Count > 0)
            {
                RegisterSoundFolders(sounds.Where(job => File.Exists(job.Path)));
            }

            Debug.Log($"[{nameof(MediaGenerator)}] Generation {(canceled ? "canceled" : "finished")}");
        }

        #region Pictures

        /// <returns>false if canceled</returns>
        private static async UniTask<bool> GenerateImageAsync(MediaPromptsConfig config, MediaPromptItem item, ImageMode mode,
            Func<bool> isCanceled)
        {
            (string category, string key) = GetImageName(item);
            string tools = Path.GetFullPath(ToolsFolder);
            string python = Path.GetFullPath(config.PythonPath);

            if (!File.Exists(python))
            {
                Debug.LogError($"[{nameof(MediaGenerator)}] Python not found: {python}. Setup: {ToolsFolder}/README.md");
                return false;
            }

            if (mode == ImageMode.Draw)
            {
                if (string.IsNullOrWhiteSpace(item.Prompt))
                {
                    Debug.LogError($"[{nameof(MediaGenerator)}] No prompt for {item.WordKey}");
                    return true;
                }

                string claude = FindClaude(config);
                if (claude == null)
                {
                    Debug.LogError($"[{nameof(MediaGenerator)}] Claude Code CLI not found, set its path in the config settings");
                    return false;
                }

                string model = string.IsNullOrWhiteSpace(config.ClaudeModel) ? string.Empty : $" --model {config.ClaudeModel}";
                ExternalProcess.Result claudeResult = await ExternalProcess.RunAsync(claude,
                    "-p --permission-mode acceptEdits --allowedTools \"Read,Write,Edit,Glob,Grep,Bash(venv/bin/python:*)\"" + model,
                    tools, TimeSpan.FromMinutes(config.ClaudeTimeoutMinutes), isCanceled, CreateClaudePrompt(config, item, category, key));

                if (claudeResult.IsCanceled)
                {
                    return false;
                }

                if (!claudeResult.IsSuccess || !File.Exists(GetSvgPath(item)))
                {
                    Debug.LogError($"[{nameof(MediaGenerator)}] Claude didn't draw {category}/{key}\n{claudeResult.Output}");
                    return true;
                }

                Debug.Log($"[{nameof(MediaGenerator)}] Claude drew {category}/{key}\n{claudeResult.Output}");
            }

            foreach (string script in new[] { "render.py", "install.py" })
            {
                ExternalProcess.Result result = await ExternalProcess.RunAsync(python, $"{script} \"{category}/{key}\"", tools,
                    TimeSpan.FromMinutes(2), isCanceled);
                if (!result.IsSuccess)
                {
                    if (result.IsCanceled)
                    {
                        return false;
                    }

                    Debug.LogError($"[{nameof(MediaGenerator)}] {script} failed for {category}/{key}\n{result.Output}");
                    return true;
                }
            }

            Debug.Log($"[{nameof(MediaGenerator)}] Picture installed: {PathHelper.GetTexturePath(item.ImageKey)}");
            return true;
        }

        private static string CreateClaudePrompt(MediaPromptsConfig config, MediaPromptItem item, string category, string key)
        {
            string generator = $"gen/auto/{category}/{key}.py";
            return $@"Draw the word picture for the Chang language learning game.

Word: {item.Translation} ({item.Language}: {item.LearnWord})
Category: {category}
Key: {key}
Picture description: {item.Prompt}
{config.ImageInstructions}

Rules:
- Follow STYLE.md exactly. Look at a couple of existing generators in gen/ and the helpers in gen/common.py to keep the same look.
- Write the generator to ""{generator}"". It draws only this picture and saves it with save(""{category}"", ""{key}"", <background>, <body>) from gen/common.py.
  Before importing common add the gen folder to the import path: sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "".."", ""..""))
- Run it: venv/bin/python ""{generator}""
- Render it: venv/bin/python render.py ""{category}/{key}"", then look at png/{category}/{key}.png.
  Fix the generator until the picture is clear and matches the description and the style.
- Do not change any other files.";
        }

        private static string FindClaude(MediaPromptsConfig config)
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            List<string> candidates = new()
            {
                Path.Combine(home, ".local/bin/claude"),
                Path.Combine(home, ".claude/local/claude"),
            };
            candidates.AddRange(ExternalProcess.InToolFolders("claude"));

            // the CLI of the Claude desktop app, the newest version first
            string desktop = Path.Combine(home, "Library/Application Support/Claude/claude-code");
            if (Directory.Exists(desktop))
            {
                candidates.AddRange(Directory.GetDirectories(desktop)
                    .OrderByDescending(dir => Version.TryParse(Path.GetFileName(dir), out Version version) ? version : new Version())
                    .SelectMany(dir => Directory.GetDirectories(dir))
                    .Select(dir => Path.Combine(dir, "claude.app/Contents/MacOS/claude")));
            }

            return ExternalProcess.FindTool(config.ClaudePath, candidates.ToArray());
        }

        private static bool HasImage(MediaPromptItem item)
        {
            return File.Exists(PathHelper.GetTexturePath(item.ImageKey));
        }

        private static ImageMode GetImageMode(MediaPromptItem item)
        {
            return File.Exists(GetSvgPath(item)) ? ImageMode.Render : ImageMode.Draw;
        }

        private static string GetSvgPath(MediaPromptItem item)
        {
            (string category, string key) = GetImageName(item);
            return Path.Combine(ToolsFolder, "svg", category, key + ".svg");
        }

        // ImageKey = Thai/Vocabulary/Fruits/Coconut, the picture is svg/Fruits/Coconut.svg
        private static (string category, string key) GetImageName(MediaPromptItem item)
        {
            string[] parts = item.ImageKey.Split('/');
            return (parts[^2], parts[^1]);
        }

        #endregion

        #region Sounds

        private static IEnumerable<SoundJob> GetSoundJobs(MediaPromptsConfig config, MediaPromptItem item)
        {
            foreach (LanguageVoices language in config.SelectedLanguages)
            {
                string soundKey = language.Language == item.Language
                    ? item.SoundKey
                    : PathHelper.GetNativeSoundKey(item.SoundKey, language.Language);

                foreach (SoundVoices voice in config.SelectedVoices)
                {
                    yield return new SoundJob
                    {
                        Item = item,
                        Language = language,
                        Voice = voice,
                        Path = PathHelper.GetSoundPath(soundKey, voice),
                    };
                }
            }
        }

        /// <returns>false if canceled</returns>
        private static async UniTask<bool> GenerateSoundsAsync(MediaPromptsConfig config, List<SoundJob> jobs, Func<int, bool> isCanceled)
        {
            string ffmpeg = ExternalProcess.FindTool(config.FfmpegPath, ExternalProcess.InToolFolders("ffmpeg"));
            if (ffmpeg == null)
            {
                Debug.LogError($"[{nameof(MediaGenerator)}] ffmpeg not found, it trims the silence and converts the sounds to mp3: brew install ffmpeg");
                return true;
            }

            LocalizationManager.Read();
            using TextToSpeechService textToSpeech = new();
            int created = 0;
            int failedInRow = 0;

            for (int i = 0; i < jobs.Count; i++)
            {
                SoundJob job = jobs[i];
                if (isCanceled(i))
                {
                    return false;
                }

                string text = GetSoundText(job);
                string voiceName = job.Language.GetVoice(job.Voice);
                string filter = null;
                if (string.IsNullOrWhiteSpace(voiceName) && job.Voice == SoundVoices.Male && config.MaleFromFemaleVoice)
                {
                    voiceName = job.Language.FemaleVoice;
                    filter = config.MaleVoiceFilter;
                }

                if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(voiceName))
                {
                    Debug.LogWarning($"[{nameof(MediaGenerator)}] No {(string.IsNullOrWhiteSpace(text) ? "text" : "voice")} for {job.Path}");
                    continue;
                }

                string source = Path.Combine(Path.GetTempPath(), $"chang_sound_{Guid.NewGuid():N}");
                try
                {
                    source = job.Language.Engine == SoundEngines.GoogleTts
                        ? await SynthesizeGoogleAsync(textToSpeech, text, job.Language.LanguageCode, voiceName, source)
                        : await SynthesizeSayAsync(text, voiceName, source, () => isCanceled(i));

                    if (source == null)
                    {
                        Debug.LogError($"[{nameof(MediaGenerator)}] No audio for {job.Path} [{text}]");
                        if (++failedInRow >= MaxFailedSoundsInRow)
                        {
                            Debug.LogError($"[{nameof(MediaGenerator)}] Sounds generation stopped after {MaxFailedSoundsInRow} failed sounds in a row");
                            break;
                        }

                        continue;
                    }

                    failedInRow = 0;
                    Directory.CreateDirectory(Path.GetDirectoryName(job.Path)!);
                    if (await ConvertAsync(ffmpeg, source, job.Path, filter, () => isCanceled(i)))
                    {
                        created++;
                        Debug.Log($"[{nameof(MediaGenerator)}] Sound created: {job.Path} [{text}]");
                    }
                }
                finally
                {
                    if (source != null && File.Exists(source))
                    {
                        File.Delete(source);
                    }
                }

                if (job.Language.Engine == SoundEngines.GoogleTts)
                {
                    await UniTask.WaitForSeconds(config.SoundRequestDelaySeconds);
                }
            }

            Debug.Log($"[{nameof(MediaGenerator)}] Sounds created: {created}/{jobs.Count}");
            return true;
        }

        /// <returns>the mp3 file or null</returns>
        private static async UniTask<string> SynthesizeGoogleAsync(TextToSpeechService textToSpeech, string text, string languageCode,
            string voiceName, string file)
        {
            AudioContent audio = await textToSpeech.GetAudioAsync(text, languageCode, voiceName);
            if (string.IsNullOrEmpty(audio?.audioContent))
            {
                return null;
            }

            file += ".mp3";
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(audio.audioContent));
            return file;
        }

        /// <returns>the aiff file or null</returns>
        private static async UniTask<string> SynthesizeSayAsync(string text, string voiceName, string file, Func<bool> isCanceled)
        {
            file += ".aiff";
            ExternalProcess.Result result = await ExternalProcess.RunAsync("/usr/bin/say", $"-v \"{voiceName}\" -o \"{file}\"",
                Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), isCanceled, text);

            if (!result.IsSuccess || !File.Exists(file))
            {
                Debug.LogError($"[{nameof(MediaGenerator)}] say failed, voice: {voiceName}\n{result.Output}");
                return null;
            }

            return file;
        }

        private static string GetSoundText(SoundJob job)
        {
            if (job.Language.Language == job.Item.Language)
            {
                return job.Item.LearnWord;
            }

            // the localization key of a word is its WordKey, brackets are not read aloud
            string language = job.Language.Language.ToString();
            if (!LocalizationManager.Dictionary.TryGetValue(language, out Dictionary<string, string> translations) ||
                !translations.TryGetValue(job.Item.WordKey, out string translation))
            {
                return null;
            }

            return Regex.Replace(translation, @"[\(\)\[\]]", string.Empty).Trim();
        }

        // applies the voice filter, trims the silence and converts to mp3 24 kHz mono 32 kbps
        private static async UniTask<bool> ConvertAsync(string ffmpeg, string source, string path, string filter, Func<bool> isCanceled)
        {
            string filters = string.IsNullOrWhiteSpace(filter) ? string.Empty : filter.Trim().TrimEnd(',') + ",";
            ExternalProcess.Result result = await ExternalProcess.RunAsync(ffmpeg,
                $"-y -loglevel error -i \"{source}\" -af \"{filters}{TrimSilence},{TrimSilence}\" -ar 24000 -ac 1 -b:a 32k \"{Path.GetFullPath(path)}\"",
                Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), isCanceled);

            if (!result.IsSuccess)
            {
                Debug.LogError($"[{nameof(MediaGenerator)}] ffmpeg failed for {path}\n{result.Output}");
            }

            return result.IsSuccess;
        }

        // a sound folder SoundWords/{Language}/{Voice}/{Section} is an Addressables entry with its path as the address
        private static void RegisterSoundFolders(IEnumerable<SoundJob> jobs)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup template = settings.FindGroup(SoundGroupTemplate);

            foreach (var folder in jobs.Select(job => (job.Language.Language, Path: Path.GetDirectoryName(job.Path)!.Replace('\\', '/'))).Distinct())
            {
                string guid = AssetDatabase.AssetPathToGUID(folder.Path);
                if (string.IsNullOrEmpty(guid) || settings.FindAssetEntry(guid) != null)
                {
                    continue;
                }

                string groupName = $"Remote_{folder.Language}_Sound_Words";
                AddressableAssetGroup group = settings.FindGroup(groupName) ??
                                              settings.CreateGroup(groupName, false, false, true, template.Schemas);

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress(folder.Path);
                Debug.Log($"[{nameof(MediaGenerator)}] Addressables entry added to {groupName}: {folder.Path}");
            }

            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}

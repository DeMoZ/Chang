using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Chang.Core;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using VocabularyConfig = Chang.Core.Vocabulary;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.Media
{
    /// <summary>
    /// The inspector of <see cref="WordSoundsConfig"/>: voices the word sounds of a Vocabulary config with the macOS voices,
    /// Siri voices included (macOS only): Male and/or Female, Normal and/or Slow, only the missing files or all of them,
    /// for the words checked in the list.
    /// Siri voices are only given to Apple-signed programs, so the words are spoken by Tools/ChangVoice/Resources/siri-tts.swift
    /// run by the Swift interpreter from Xcode; ffmpeg then trims the silence and writes mp3 24 kHz mono 32 kbps.
    /// New section folders get their Addressables entries from <see cref="SoundFoldersPostprocessor"/>.
    /// See Docs/content-pipeline.md#word-sounds.
    /// </summary>
    [CustomEditor(typeof(WordSoundsConfig))]
    public class WordSoundsConfigEditor : UnityEditor.Editor
    {
        private const string Helper = "Tools/ChangVoice/Resources/siri-tts.swift";
        private const string SoundsRoot = "Assets/Project/Resources_Bundled";

        // word sounds must have no silence around them: sentences are assembled from words
        private const string Trim = "silenceremove=start_periods=1:start_threshold=-40dB:start_silence=0.02:detection=rms:window=0.02,areverse";

        private static readonly Dictionary<Languages, string> Locales = new()
        {
            { Languages.Thai, "th" }, { Languages.English, "en" }, { Languages.Russian, "ru" }, { Languages.Vietnamese, "vi" },
            { Languages.Lao, "lo" }, { Languages.ChineseSimplified, "zh" }, { Languages.Japanese, "ja" }, { Languages.Korean, "ko" },
        };

        // voices are listed by the helper once per Editor session: it takes a few seconds (the interpreter compiles it)
        private static List<VoiceInfo> _voices;
        private static bool _voicesLoading;

        private enum Gender { Male, Female }

        private enum Speed { Normal, Slow }

        private readonly struct Output
        {
            public readonly Gender Gender;
            public readonly Speed Speed;

            public Output(Gender gender, Speed speed)
            {
                Gender = gender;
                Speed = speed;
            }

            public string Root => Speed == Speed.Normal ? "SoundWords" : "SoundWordsSlow";
            public string Badge => (Gender == Gender.Male ? "M" : "F") + (Speed == Speed.Slow ? "s" : "");
        }

        private class VoiceInfo
        {
            public string id;
            public string name;
            public string language;
            public string gender;
            public bool siri;

            public string Title => $"{name}{(siri ? " (Siri)" : "")}{(string.IsNullOrEmpty(gender) ? "" : " · " + gender)}";
        }

        private readonly HashSet<string> _checked = new();
        private readonly HashSet<string> _existing = new();
        private readonly HashSet<string> _collapsed = new();
        private readonly List<string> _log = new();
        private string _filter = "";
        private Vector2 _scroll;
        private Vector2 _logScroll;
        private bool _running;
        private bool _cancel;
        private float _progress;
        private string _status = "";

        private WordSoundsConfig Config => (WordSoundsConfig)target;

        private IEnumerable<Word> Words => Config.Vocabulary == null
            ? Enumerable.Empty<Word>()
            : Config.Vocabulary.Words.Where(w => !string.IsNullOrEmpty(w.SoundKey) && !string.IsNullOrEmpty(w.LearnWord));

        private void OnEnable()
        {
            Refresh();
            if (_voices == null && !_voicesLoading)
            {
                LoadVoicesAsync().Forget();
            }
        }

        // ---- data -----------------------------------------------------------------------------------

        /// <summary>"Thai/Vocabulary/Fruits/Mango" → "Fruits/Mango"</summary>
        private static string Relative(Word word)
        {
            string[] parts = word.SoundKey.Split(new[] { '/' }, 3);
            return parts.Length == 3 ? parts[2] : $"{word.Section}/{word.Key}";
        }

        private string FilePath(Word word, Output output) =>
            $"{SoundsRoot}/{output.Root}/{Config.Vocabulary.Language}/{output.Gender}/{Relative(word)}.mp3";

        private static string FileId(Word word, Output output) => $"{output.Root}/{output.Gender}/{word.SoundKey}";

        private bool Exists(Word word, Output output) => _existing.Contains(FileId(word, output));

        private static IEnumerable<Output> AllOutputs =>
            from g in new[] { Gender.Male, Gender.Female } from s in new[] { Speed.Normal, Speed.Slow } select new Output(g, s);

        private List<Output> SelectedOutputs => AllOutputs
            .Where(o => (o.Gender == Gender.Male ? Config.Male : Config.Female) && (o.Speed == Speed.Normal ? Config.Normal : Config.Slow))
            .ToList();

        /// <summary>Reads which files exist and checks the words with a missing selected file.</summary>
        private void Refresh()
        {
            _existing.Clear();
            if (Config.Vocabulary == null)
            {
                return;
            }

            foreach (Word word in Words)
            {
                foreach (Output output in AllOutputs)
                {
                    if (File.Exists(FilePath(word, output)))
                    {
                        _existing.Add(FileId(word, output));
                    }
                }
            }

            ResetChecks();
        }

        private void ResetChecks()
        {
            List<Output> outputs = SelectedOutputs;
            _checked.Clear();
            _checked.UnionWith(Words.Where(w => outputs.Any(o => !Exists(w, o))).Select(w => w.SoundKey));
        }

        private int PlannedCount
        {
            get
            {
                List<Output> outputs = SelectedOutputs;
                return Words.Where(w => _checked.Contains(w.SoundKey)).Sum(w => outputs.Count(o => Config.Rewrite || !Exists(w, o)));
            }
        }

        /// <summary>The text without the marks that are not read aloud ("ไป...มา", "ร้าน + ...", brackets); a slash is a pause.</summary>
        private static string SpeechText(string text)
        {
            text = Regex.Replace(text, @"\.{2,}|…|\+|[()\[\]]", " ");
            text = Regex.Replace(text, @"\s*/\s*", ", ");
            return Regex.Replace(text, @"\s+", " ").Trim(' ', ',');
        }

        // ---- voices ---------------------------------------------------------------------------------

        private async UniTaskVoid LoadVoicesAsync()
        {
            _voicesLoading = true;
            _status = "Loading voices…";
            try
            {
                ExternalProcess.Result result = await RunHelperAsync("--list-json", TimeSpan.FromMinutes(2), () => false);
                string json = result.Output.Split('\n').LastOrDefault(l => l.StartsWith("["));
                if (!result.IsSuccess || json == null)
                {
                    _status = "Voices not loaded: the Siri voices need Xcode";
                    Debug.LogError($"[{nameof(WordSoundsConfigEditor)}] [{nameof(LoadVoicesAsync)}] {result.Output}");
                    return;
                }

                _voices = JsonConvert.DeserializeObject<List<VoiceInfo>>(json);
                _status = $"{_voices.Count} voices";
            }
            finally
            {
                _voicesLoading = false;
                Repaint();
            }
        }

        private List<VoiceInfo> LanguageVoices
        {
            get
            {
                VocabularyConfig vocabulary = Config.Vocabulary;
                if (vocabulary == null || _voices == null)
                {
                    return new List<VoiceInfo>();
                }

                string prefix = Locales.TryGetValue(vocabulary.Language, out string locale) ? locale : vocabulary.Language.ToString().Substring(0, 2).ToLower();
                return _voices.Where(v => v.language.StartsWith(prefix)).OrderBy(v => v.siri ? 0 : 1).ThenBy(v => v.name).ToList();
            }
        }

        /// <summary>The chosen voice, or a Siri voice of the gender (Thai: Voice 1 is male, Voice 2 female), or any voice of it.</summary>
        private string VoiceFor(Gender gender)
        {
            List<VoiceInfo> voices = LanguageVoices;
            string chosen = gender == Gender.Male ? Config.MaleVoice : Config.FemaleVoice;
            if (voices.Any(v => v.id == chosen))
            {
                return chosen;
            }

            return (voices.FirstOrDefault(v => v.siri && v.gender == gender.ToString()) ?? voices.FirstOrDefault(v => v.gender == gender.ToString()))?.id;
        }

        private static UniTask<ExternalProcess.Result> RunHelperAsync(string arguments, TimeSpan timeout, Func<bool> poll)
        {
            string script = Path.GetFullPath(Helper);
            return ExternalProcess.RunAsync("/usr/bin/xcrun", $"swift \"{script}\" {arguments}", Directory.GetCurrentDirectory(), timeout, poll);
        }

        // ---- GUI ------------------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(_running))
            {
                DrawSettings();
                EditorGUILayout.Space(6);
                DrawToolbar();
                DrawList();
            }

            EditorGUILayout.Space(6);
            DrawFooter();
        }

        private void DrawSettings()
        {
            WordSoundsConfig config = Config;
            EditorGUI.BeginChangeCheck();
            var vocabulary = (VocabularyConfig)EditorGUILayout.ObjectField(new GUIContent("Vocabulary",
                "BookConfigs/<Language>/Vocabulary.asset: download the configs from Google Sheets first to get new words"),
                config.Vocabulary, typeof(VocabularyConfig), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Word sounds vocabulary");
                config.Vocabulary = vocabulary;
                EditorUtility.SetDirty(config);
                Refresh();
            }

            if (vocabulary == null)
            {
                return;
            }

            List<VoiceInfo> voices = LanguageVoices;
            string[] titles = voices.Select(v => v.Title).ToArray();

            EditorGUI.BeginChangeCheck();
            bool male = VoiceRow("Male", config.Male, Gender.Male, voices, titles, out string maleVoice);
            bool female = VoiceRow("Female", config.Female, Gender.Female, voices, titles, out string femaleVoice);
            float normalRate = config.NormalRate, slowRate = config.SlowRate;
            bool normal = SpeedRow("Normal", config.Normal, ref normalRate, 0.3f, 0.6f);
            bool slow = SpeedRow("Slow", config.Slow, ref slowRate, 0.15f, 0.45f);
            bool rewrite = EditorGUILayout.ToggleLeft(new GUIContent("Rewrite existing", "Off: only the missing files of the checked words are voiced"), config.Rewrite);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Word sounds settings");
                bool outputsChanged = male != config.Male || female != config.Female || normal != config.Normal || slow != config.Slow;
                (config.Male, config.Female, config.Normal, config.Slow, config.Rewrite) = (male, female, normal, slow, rewrite);
                (config.MaleVoice, config.FemaleVoice, config.NormalRate, config.SlowRate) = (maleVoice, femaleVoice, normalRate, slowRate);
                EditorUtility.SetDirty(config);
                if (outputsChanged)
                {
                    ResetChecks();
                }
            }
        }

        private bool VoiceRow(string label, bool on, Gender gender, List<VoiceInfo> voices, string[] titles, out string voice)
        {
            voice = gender == Gender.Male ? Config.MaleVoice : Config.FemaleVoice;
            using (new EditorGUILayout.HorizontalScope())
            {
                on = EditorGUILayout.ToggleLeft(label, on, GUILayout.Width(70));
                if (voices.Count == 0)
                {
                    GUILayout.Label(_voicesLoading ? "loading voices…" : "no voices", EditorStyles.miniLabel);
                    return on;
                }

                int index = voices.FindIndex(v => v.id == VoiceFor(gender));
                int picked = EditorGUILayout.Popup(index, titles, GUILayout.Width(260));
                if (picked != index && picked >= 0)
                {
                    voice = voices[picked].id;
                }
            }

            return on;
        }

        private static bool SpeedRow(string label, bool on, ref float rate, float min, float max)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                on = EditorGUILayout.ToggleLeft(label, on, GUILayout.Width(70));
                rate = EditorGUILayout.Slider(rate, min, max, GUILayout.Width(260));
            }

            return on;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _filter = EditorGUILayout.TextField(_filter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
                if (GUILayout.Button(new GUIContent("Missing", "Check the words with a missing selected file"), GUILayout.Width(64))) ResetChecks();
                if (GUILayout.Button("All", GUILayout.Width(40))) _checked.UnionWith(Filtered().Select(w => w.SoundKey));
                if (GUILayout.Button("None", GUILayout.Width(46))) _checked.ExceptWith(Filtered().Select(w => w.SoundKey));
                if (GUILayout.Button(new GUIContent("↻", "Read the files again"), GUILayout.Width(26))) Refresh();
            }

            GUILayout.Label($"{_checked.Count} of {Words.Count()} words checked · {PlannedCount} files to write", EditorStyles.miniLabel);
        }

        private IEnumerable<Word> Filtered()
        {
            string f = _filter.Trim().ToLowerInvariant();
            return string.IsNullOrEmpty(f) ? Words : Words.Where(w => w.SoundKey.ToLowerInvariant().Contains(f) || w.LearnWord.Contains(f));
        }

        private void DrawList()
        {
            if (Config.Vocabulary == null)
            {
                return;
            }

            List<Output> selected = SelectedOutputs;
            _scroll = EditorGUILayout.BeginScrollView(_scroll, EditorStyles.helpBox, GUILayout.Height(420));
            foreach (IGrouping<string, Word> section in Filtered().GroupBy(w => w.Section))
            {
                List<Word> words = section.ToList();
                bool open = !_collapsed.Contains(section.Key);
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool all = words.All(w => _checked.Contains(w.SoundKey));
                    bool toggled = EditorGUILayout.Toggle(all, GUILayout.Width(16));
                    if (toggled != all)
                    {
                        if (toggled) _checked.UnionWith(words.Select(w => w.SoundKey));
                        else _checked.ExceptWith(words.Select(w => w.SoundKey));
                    }

                    bool nowOpen = EditorGUILayout.Foldout(open, $"{section.Key} ({words.Count(w => _checked.Contains(w.SoundKey))}/{words.Count})", true);
                    if (nowOpen != open)
                    {
                        if (nowOpen) _collapsed.Remove(section.Key);
                        else _collapsed.Add(section.Key);
                    }
                }

                if (open)
                {
                    foreach (Word word in words)
                    {
                        DrawRow(word, selected);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(Word word, List<Output> selected)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(18);
                bool on = _checked.Contains(word.SoundKey);
                if (EditorGUILayout.Toggle(on, GUILayout.Width(16)) != on)
                {
                    if (on) _checked.Remove(word.SoundKey);
                    else _checked.Add(word.SoundKey);
                }

                GUILayout.Label(new GUIContent(word.Key, word.SoundKey), GUILayout.MinWidth(80));
                GUILayout.Label(word.LearnWord, GUILayout.MinWidth(80));
                GUILayout.FlexibleSpace();

                foreach (Output output in AllOutputs)
                {
                    bool exists = Exists(word, output);
                    bool isSelected = selected.Any(o => o.Gender == output.Gender && o.Speed == output.Speed);
                    Color previous = GUI.backgroundColor;
                    GUI.backgroundColor = exists ? new Color(0.35f, 0.85f, 0.4f, isSelected ? 1f : 0.45f) : new Color(0.5f, 0.5f, 0.5f, isSelected ? 0.8f : 0.3f);
                    var content = new GUIContent(output.Badge, $"{output.Gender} {output.Speed}: {(exists ? "exists, click to play" : "missing")}");
                    if (GUILayout.Button(content, EditorStyles.miniButton, GUILayout.Width(26)) && exists)
                    {
                        Play(FilePath(word, output));
                    }

                    GUI.backgroundColor = previous;
                }
            }
        }

        private void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (_running)
                {
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Width(220)), _progress, $"{Mathf.RoundToInt(_progress * 100)}%");
                    if (GUILayout.Button("Cancel", GUILayout.Width(70)))
                    {
                        _cancel = true;
                    }
                }
                else
                {
                    using (new EditorGUI.DisabledScope(Config.Vocabulary == null || PlannedCount == 0))
                    {
                        if (GUILayout.Button($"Voice {PlannedCount} files", GUILayout.Height(26), GUILayout.Width(160)))
                        {
                            GenerateAsync().Forget();
                        }
                    }
                }
            }

            GUILayout.Label(_status, EditorStyles.miniLabel);
            if (_log.Count > 0)
            {
                _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.Height(110));
                EditorGUILayout.SelectableLabel(string.Join("\n", _log), EditorStyles.wordWrappedMiniLabel, GUILayout.Height(Mathf.Max(100, _log.Count * 13)));
                EditorGUILayout.EndScrollView();
            }
        }

        private static void Play(string path)
        {
            Process.Start(new ProcessStartInfo("/usr/bin/afplay", $"\"{Path.GetFullPath(path)}\"") { UseShellExecute = false, CreateNoWindow = true });
        }

        // ---- generation -----------------------------------------------------------------------------

        private async UniTaskVoid GenerateAsync()
        {
            string ffmpeg = ExternalProcess.FindTool(null, ExternalProcess.InToolFolders("ffmpeg"));
            if (ffmpeg == null)
            {
                Log("ffmpeg not found: brew install ffmpeg");
                return;
            }

            var jobs = new List<(Output Output, string Voice, List<Word> Words)>();
            foreach (Output output in SelectedOutputs)
            {
                List<Word> words = Words.Where(w => _checked.Contains(w.SoundKey) && (Config.Rewrite || !Exists(w, output))).ToList();
                if (words.Count == 0)
                {
                    continue;
                }

                string voice = VoiceFor(output.Gender);
                if (voice == null)
                {
                    Log($"No {output.Gender} voice for {Config.Vocabulary.Language}");
                    continue;
                }

                jobs.Add((output, voice, words));
            }

            int total = jobs.Sum(j => j.Words.Count);
            if (total == 0)
            {
                return;
            }

            _running = true;
            _cancel = false;
            _progress = 0f;
            _log.Clear();
            int done = 0;
            try
            {
                foreach ((Output output, string voice, List<Word> words) in jobs)
                {
                    if (_cancel)
                    {
                        break;
                    }

                    string temp = Path.Combine(Path.GetTempPath(), "chang-voice-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(temp);
                    try
                    {
                        _status = $"{output.Gender} · {output.Speed}: speaking {words.Count} words";
                        List<string> cafs = words.Select((_, i) => Path.Combine(temp, $"{i}.caf")).ToList();
                        var job = new JObject
                        {
                            ["voice"] = voice,
                            ["rate"] = output.Speed == Speed.Slow ? Config.SlowRate : Config.NormalRate,
                            ["items"] = new JArray(words.Select((w, i) => new JObject { ["text"] = SpeechText(w.LearnWord), ["out"] = cafs[i] })),
                        };
                        string jobPath = Path.Combine(temp, "job.json");
                        File.WriteAllText(jobPath, job.ToString());

                        int before = done;
                        ExternalProcess.Result spoken = await RunHelperAsync($"--render \"{jobPath}\"", TimeSpan.FromMinutes(60), () =>
                        {
                            // a .caf appears when its word starts: the ones before it are finished
                            int written = Math.Max(0, Directory.GetFiles(temp, "*.caf").Length - 1);
                            _progress = (before + written * 0.5f) / total;
                            Repaint();
                            return _cancel;
                        });

                        if (!spoken.IsSuccess)
                        {
                            Log($"{output.Gender} {output.Speed}: {(spoken.IsCanceled ? "canceled" : spoken.Output)}");
                            continue;
                        }

                        _status = $"{output.Gender} · {output.Speed}: writing mp3";
                        for (int i = 0; i < words.Count && !_cancel; i++)
                        {
                            string mp3 = Path.GetFullPath(FilePath(words[i], output));
                            Directory.CreateDirectory(Path.GetDirectoryName(mp3)!);
                            ExternalProcess.Result converted = await ExternalProcess.RunAsync(ffmpeg,
                                $"-y -loglevel error -i \"{cafs[i]}\" -af \"{Trim},{Trim}\" -ar 24000 -ac 1 -b:a 32k \"{mp3}\"",
                                Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), () => _cancel);
                            done++;
                            _progress = (float)done / total;
                            Log(converted.IsSuccess
                                ? $"✓ {output.Gender} {output.Speed} {Relative(words[i])} [{SpeechText(words[i].LearnWord)}]"
                                : $"✗ {Relative(words[i])}: {converted.Output}");
                            Repaint();
                        }
                    }
                    finally
                    {
                        Directory.Delete(temp, true);
                    }
                }
            }
            finally
            {
                _running = false;
                _status = _cancel ? $"Canceled after {done} of {total}" : $"Done: {done} files";
                Debug.Log($"[{nameof(WordSoundsConfigEditor)}] [{nameof(GenerateAsync)}] {_status}");
                // imports the new files; new section folders get their Addressables entries (SoundFoldersPostprocessor)
                AssetDatabase.Refresh();
                Refresh();
                Repaint();
            }
        }

        private void Log(string line)
        {
            _log.Add(line);
            _logScroll.y = float.MaxValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Editor.Inspector;
using Chang.Resources;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.Media
{
    /// <summary>
    /// Prompts of the word pictures and the settings of the word pictures and sounds generation.
    /// A picture is drawn by Claude Code CLI as a Python generator in Tools/WordImages, Python renders and installs the PNG.
    /// A sound is made by Google Text-To-Speech or macOS say for every selected language and voice.
    /// </summary>
    [DeclareBoxGroup("Generation", Title = "Generation")]
    [DeclareHorizontalGroup("Generation/Voices")]
    [DeclareHorizontalGroup("Generation/Tools")]
    [DeclareFoldoutGroup("Languages", Title = "Languages")]
    [DeclareFoldoutGroup("Settings", Title = "Settings")]
    public class MediaPromptsConfig : ScriptableObject
    {
        public const string ConfigPath = "Assets/Project/Configs/MediaPrompts.asset";

        [Group("Generation/Voices"), LabelText("Female voice")]
        public bool FemaleVoice = true;

        [Group("Generation/Voices"), LabelText("Male voice")]
        public bool MaleVoice = true;

        [Group("Languages")]
        [InfoBox("The book language is voiced with the learn word, the other languages with the word translation from the localization. " +
                 "An empty male voice is made from the female one, see Settings.")]
        [TableList(Draggable = false, HideAddButton = true, HideRemoveButton = true, AlwaysExpanded = true)]
        public List<LanguageVoices> SoundLanguages = LanguageVoices.CreateDefaults();

        [Group("Settings"), Tooltip("Empty: found automatically (~/.local/bin, Homebrew, Claude desktop app)")]
        public string ClaudePath;

        [Group("Settings"), Tooltip("Empty: the default Claude Code model")]
        public string ClaudeModel;

        [Group("Settings")]
        public int ClaudeTimeoutMinutes = 15;

        [Group("Settings"), Tooltip("Python with cairosvg and Pillow, relative to the project folder")]
        public string PythonPath = "Tools/WordImages/venv/bin/python";

        [Group("Settings"), Tooltip("Empty: found in Homebrew. Trims silence of the word sounds")]
        public string FfmpegPath;

        [Group("Settings"), Tooltip("A language without a male voice gets it from the female voice processed by the filter below")]
        public bool MaleFromFemaleVoice = true;

        [Group("Settings"), TextArea(2, 4), Tooltip("ffmpeg audio filter that makes a male voice from the female one, the silence is trimmed after it")]
        public string MaleVoiceFilter =
            "rubberband=pitch=0.75:pitchq=quality:formant=preserved:phase=independent:window=standard:transients=smooth," +
            "highpass=f=90,equalizer=f=300:t=o:w=1.2:g=-5,equalizer=f=3000:t=o:w=1:g=3";

        [Group("Settings"), Tooltip("Delay between Text-To-Speech requests")]
        public float SoundRequestDelaySeconds = 0.5f;

        [Group("Settings"), TextArea(2, 6), Tooltip("Added to every picture prompt sent to Claude")]
        public string ImageInstructions;

        [Title("Words")]
        [ListDrawerSettings(Draggable = false, HideAddButton = true, ShowElementLabels = true)]
        public List<MediaPromptSection> Sections = new();

        [MenuItem("Chang/Utilities/Media Prompts", false, 20)]
        private static void SelectConfig()
        {
            MediaPromptsConfig config = Load();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        public static MediaPromptsConfig Load()
        {
            MediaPromptsConfig config = AssetDatabase.LoadAssetAtPath<MediaPromptsConfig>(ConfigPath);
            if (config == null)
            {
                config = CreateInstance<MediaPromptsConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
            }

            return config;
        }

        public IEnumerable<MediaPromptItem> Items => Sections.SelectMany(section => section.Items);

        public IEnumerable<SoundVoices> SelectedVoices
        {
            get
            {
                if (FemaleVoice) yield return SoundVoices.Female;
                if (MaleVoice) yield return SoundVoices.Male;
            }
        }

        public IEnumerable<LanguageVoices> SelectedLanguages => SoundLanguages.Where(language => language.Enabled);

        [Group("Generation")]
        [InfoBox("Draw the missing pictures and voice the missing sounds of all words")]
        [Button(ButtonSizes.Large, "Generate All Missing")]
        [ButtonTooltip("Words without a PNG: Claude Code draws the picture from the prompt, Python renders and installs it. " +
                       "Words without an mp3 for the selected languages and voices: Google Text-To-Speech voices them.")]
        private void GenerateAllMissing()
        {
            MediaGenerator.GenerateAllMissing(this);
        }

        [Group("Generation/Tools")]
        [InfoBox("List what is missing")]
        [Button(ButtonSizes.Medium, "Log Missing")]
        [ButtonTooltip("Logs the words without a picture and the sounds missing for the selected languages and voices")]
        private void LogMissing()
        {
            MediaGenerator.LogMissing(this);
        }

        [Group("Generation/Tools")]
        [InfoBox("Update the words from the vocabulary")]
        [Button(ButtonSizes.Medium, "Sync With Vocabulary")]
        [ButtonTooltip("Adds new words of the Vocabulary configs, removes deleted ones and updates the learn words and translations. Prompts are kept")]
        private void SyncWithVocabulary()
        {
            Sync();
        }

        public void Sync()
        {
            Dictionary<string, MediaPromptItem> oldItems = Items
                .Where(item => !string.IsNullOrEmpty(item.WordKey))
                .GroupBy(item => item.WordKey)
                .ToDictionary(group => group.Key, group => group.First());

            List<MediaPromptSection> sections = new();
            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                Core.Vocabulary vocabulary =
                    AssetDatabase.LoadAssetAtPath<Core.Vocabulary>(AssetPaths.Addressables.VocabularyPath(language));
                if (vocabulary == null)
                {
                    continue;
                }

                foreach (Core.Word word in vocabulary.Words)
                {
                    MediaPromptSection section = sections.FirstOrDefault(s => s.Language == language && s.Section == word.Section);
                    if (section == null)
                    {
                        section = new MediaPromptSection { Section = word.Section, Language = language };
                        sections.Add(section);
                    }

                    oldItems.Remove(word.WordKey, out MediaPromptItem item);
                    item ??= new MediaPromptItem();
                    item.SetWord(word);
                    section.Items.Add(item);
                }
            }

            foreach (MediaPromptItem removed in oldItems.Values)
            {
                Debug.LogWarning($"[{nameof(MediaPromptsConfig)}] Word removed from the vocabulary: {removed.WordKey}\nPrompt: {removed.Prompt}");
            }

            Sections = sections;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            Debug.Log($"[{nameof(MediaPromptsConfig)}] Synced: {Items.Count()} words in {Sections.Count} sections, removed {oldItems.Count}");
        }

        private void OnValidate()
        {
            // every language has a row, new enum values are added to the end
            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                if (SoundLanguages.All(row => row.Language != language))
                {
                    SoundLanguages.Add(LanguageVoices.CreateDefaults().First(row => row.Language == language));
                }
            }
        }
    }

    [Serializable]
    public class MediaPromptSection
    {
        [ReadOnly] public string Section;
        [ReadOnly] public Languages Language;

        [ListDrawerSettings(Draggable = false, HideAddButton = true, HideRemoveButton = true, ShowElementLabels = true)]
        public List<MediaPromptItem> Items = new();
    }

    [Serializable]
    [DeclareHorizontalGroup("Buttons")]
    public class MediaPromptItem
    {
        [ReadOnly] public string Key;
        [ReadOnly] public Languages Language;
        [ReadOnly] public string Section;
        [ReadOnly] public string WordKey;
        [ReadOnly] public string ImageKey;
        [ReadOnly] public string SoundKey;
        [ReadOnly] public string LearnWord;
        [ReadOnly] public string Translation;

        [TextArea(2, 10)] public string Prompt;

        [ShowInInspector, ReadOnly]
        private string Status => MediaGenerator.GetStatus(MediaPromptsConfig.Load(), this);

        public void SetWord(Core.Word word)
        {
            Key = word.Key;
            Language = word.Language;
            Section = word.Section;
            WordKey = word.WordKey;
            ImageKey = word.ImageKey;
            SoundKey = word.SoundKey;
            LearnWord = word.LearnWord;
            Translation = word.DefaultTranslation;
        }

        [Group("Buttons")]
        [Button(ButtonSizes.Medium, "Image")]
        [ButtonTooltip("Draws the picture with Claude Code from the prompt (or re-renders the existing SVG), renders and installs the PNG")]
        private void GenerateImage()
        {
            MediaGenerator.GenerateImage(MediaPromptsConfig.Load(), this);
        }

        [Group("Buttons")]
        [Button(ButtonSizes.Medium, "Sound")]
        [ButtonTooltip("Voices the word for the selected languages and voices, existing sounds are replaced")]
        private void GenerateSound()
        {
            MediaGenerator.GenerateSound(MediaPromptsConfig.Load(), this);
        }
    }

    public enum SoundEngines
    {
        GoogleTts,
        MacOsSay,
    }

    /// <summary>
    /// The sound engine and the voices of a language.
    /// Google Text-To-Speech voice names are listed in the Google Cloud docs, macOS voices by `say -v '?'`.
    /// </summary>
    [Serializable]
    public class LanguageVoices
    {
        public bool Enabled;
        [ReadOnly] public Languages Language;

        [OnValueChanged(nameof(SetDefaultVoices))]
        public SoundEngines Engine;

        [Tooltip("Google Text-To-Speech language code")]
        public string LanguageCode;

        [Tooltip("Empty: the language has no female voice for this engine")]
        public string FemaleVoice;

        [Tooltip("Empty: the language has no male voice for this engine")]
        public string MaleVoice;

        public string GetVoice(SoundVoices voice)
        {
            return voice == SoundVoices.Female ? FemaleVoice : MaleVoice;
        }

        public static List<LanguageVoices> CreateDefaults()
        {
            return DefaultVoices.Select(defaults => defaults.Create()).ToList();
        }

        private void SetDefaultVoices()
        {
            Defaults defaults = DefaultVoices.FirstOrDefault(d => d.Language == Language);
            (FemaleVoice, MaleVoice) = Engine == SoundEngines.GoogleTts ? defaults.Google : defaults.Say;
        }

        private struct Defaults
        {
            public Languages Language;
            public string Code;
            public (string female, string male) Google;
            public (string female, string male) Say;
            public SoundEngines Engine;

            public LanguageVoices Create()
            {
                (string female, string male) voices = Engine == SoundEngines.GoogleTts ? Google : Say;
                return new LanguageVoices
                {
                    // Thai is the only book language now
                    Enabled = Language == Languages.Thai,
                    Language = Language,
                    Engine = Engine,
                    LanguageCode = Code,
                    FemaleVoice = voices.female,
                    MaleVoice = voices.male,
                };
            }
        }

        private static Defaults Voices(Languages language, string code, (string, string) say,
            SoundEngines engine = SoundEngines.GoogleTts, (string, string)? google = null)
        {
            return new Defaults
            {
                Language = language,
                Code = code,
                Google = google ?? ($"{code}-Chirp3-HD-Aoede", $"{code}-Chirp3-HD-Charon"),
                Say = say,
                Engine = engine,
            };
        }

        private static readonly Defaults[] DefaultVoices =
        {
            Voices(Languages.English, "en-US", ("Samantha", "Eddy (English (US))")),
            Voices(Languages.Spanish, "es-ES", ("Mónica", "Eddy (Spanish (Spain))")),
            Voices(Languages.Russian, "ru-RU", ("Milena", "")),
            Voices(Languages.ChineseSimplified, "cmn-CN", ("Tingting", "Eddy (Chinese (China mainland))")),
            Voices(Languages.Hindi, "hi-IN", ("Lekha", "")),
            Voices(Languages.French, "fr-FR", ("Flo (French (France))", "Thomas")),
            // Google needs billing enabled in the Cloud project, macOS has only a female Thai voice
            Voices(Languages.Thai, "th-TH", ("Kanya", ""), SoundEngines.MacOsSay),
            Voices(Languages.German, "de-DE", ("Anna", "Eddy (German (Germany))")),
            Voices(Languages.ChineseTraditional, "cmn-TW", ("Meijia", "Eddy (Chinese (Taiwan))"), google: ("cmn-TW-Wavenet-A", "cmn-TW-Wavenet-B")),
            Voices(Languages.Malay, "ms-MY", ("Amira", ""), google: ("ms-MY-Wavenet-A", "ms-MY-Wavenet-B")),
            Voices(Languages.Indonesian, "id-ID", ("Damayanti", "")),
            Voices(Languages.Korean, "ko-KR", ("Yuna", "Eddy (Korean (South Korea))")),
            Voices(Languages.Japanese, "ja-JP", ("Kyoko", "Eddy (Japanese (Japan))")),
            // neither Google nor macOS has Lao voices
            Voices(Languages.Lao, "lo-LA", ("", ""), google: ("", "")),
            Voices(Languages.Vietnamese, "vi-VN", ("Linh", "")),
        };
    }
}

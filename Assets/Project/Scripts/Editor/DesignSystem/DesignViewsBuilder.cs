using System.Linq;
using Chang.GameBook;
using Chang.Sentences;
using Chang.UI;
using Chang.UI.DesignSystem;
using Popup;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Builds the runtime views of the redesign: prefab variants of the generated screens and components
    /// (Assets/Project/UI) with the existing View scripts attached and wired. Generated prefabs are never edited,
    /// so a design re-import updates the looks and keeps the logic. Run it again after a re-import.
    /// See Docs/ui-design-system.md.
    /// </summary>
    public static partial class DesignViewsBuilder
    {
        public const string ViewsRoot = PenpotImporter.UiRoot + "/Views";
        public const string ItemsRoot = ViewsRoot + "/Items";
        public const string PopupRoot = ViewsRoot + "/Popup";

        // Space under scrolled content so the last item is not hidden by the tab bar.
        private static float TabBarClearance => DesignSize(Component("TabBar/Words")).y + 24f;

        // Zig-zag of the lesson path in "Words · Section open": node centers relative to the section center,
        // in units of the free half-width (section half-width minus half a node).
        private static readonly float[] LessonPathPattern = { 0f, 0.6f, 1f, 0.6f, 0f, -0.6f, -1f, -0.6f };

        private const float SectionHalfWidth = 224f;

        private static Vector2 LessonNodeSize => DesignSize(Component("LessonNode/Score 25pct"));

        /// <summary>Horizontal offsets of consecutive lessons, scaled to the node size so nodes stay inside the section.</summary>
        private static float[] LessonPathOffsets
        {
            get
            {
                var reach = SectionHalfWidth - LessonNodeSize.x * 0.5f;
                return LessonPathPattern.Select(k => Mathf.Round(k * reach)).ToArray();
            }
        }

        /// <summary>Vertical step of the lesson path: the score badge of a node may overlap the next row a little.</summary>
        private static float LessonStep => Mathf.Round(LessonNodeSize.y * 0.87f);

        /// <summary>Runs the build on the next editor update (for calls from tools that can't block the main thread).</summary>
        public static void BuildAllDeferred()
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    BuildAll();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            };
            EditorApplication.QueuePlayerLoopUpdate();
        }

        [MenuItem("Chang/Design System/Build Views", false, 20)]
        public static void BuildAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Build views", "Items", 0.1f);
                var items = BuildItems();

                EditorUtility.DisplayProgressBar("Build views", "Tabs", 0.4f);
                BuildMainUi(items);

                EditorUtility.DisplayProgressBar("Build views", "Lesson", 0.6f);
                BuildOverlay();
                BuildPages(items);

                EditorUtility.DisplayProgressBar("Build views", "Popups", 0.85f);
                BuildPopup();
                BuildLoading();
                BuildLogin();

                AssetDatabase.SaveAssets();
                Debug.Log($"[{nameof(DesignViewsBuilder)}] [{nameof(BuildAll)}] Views built in {ViewsRoot}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ---- items ------------------------------------------------------------------------------------

        private class Items
        {
            public CToggle Option;
            public CToggle Match;
            public CToggle Chip;
            public CToggle ChipFixed;
            public GameBookItem Lesson;
            public RectTransform Row;
            public GameBookSection Section;
            public SectionBlock SectionBlock;
            public OverviewLogItem LogItem;
            public ResultItem ResultRow;
        }

        private static Items BuildItems()
        {
            return new Items
            {
                Option = BuildToggle("OptionToggle", "OptionCard", "Correct", true,
                    new[] { "Default", "Selected", "Correct", "Wrong" }, "Default", "Selected", "Correct", "Wrong", "Default",
                    word: "Thai", phonetics: "Phonetics"),
                Match = BuildToggle("MatchToggle", "MatchTile", "Default", false,
                    new[] { "Default", "Selected", "Matched", "Wrong" }, "Default", "Selected", "Matched", "Wrong", "Matched",
                    word: "Text", phonetics: "Phonetics"),
                Chip = BuildToggle("ChipToggle", "WordChip", "Default", false,
                    new[] { "Default", "Placed" }, "Default", "Default", "Default", "Default", "Placed",
                    word: "Thai", phonetics: "Phonetics"),
                ChipFixed = BuildToggle("ChipFixedToggle", "WordChip", "Default", false,
                    new[] { "Default", "Fixed" }, "Default", "Default", "Default", "Default", "Fixed",
                    word: "Thai", phonetics: "Phonetics"),
                Lesson = BuildLessonItem(),
                Row = BuildRow(),
                Section = BuildSectionHeader(),
                SectionBlock = BuildSectionBlock(),
                LogItem = BuildLogItem(),
                ResultRow = BuildResultRow()
            };
        }

        /// <summary>A CToggle on a component whose variants are the toggle states.</summary>
        private static CToggle BuildToggle(string name, string group, string baseVariant, bool fixedWidth, string[] variants,
            string normal, string selected, string correct, string wrong, string inactive, string word, string phonetics)
        {
            var size = DesignSize(Component($"{group}/{baseVariant}"));
            var width = fixedWidth ? size.x : -1f;
            var height = size.y;
            var prefab = BuildVariant(Component($"{group}/{baseVariant}"), $"{ItemsRoot}/{name}.prefab", root =>
            {
                // Lets states with opacity (Matched) fade the whole element.
                GetOrAdd<CanvasGroup>(root);
                var toggle = ToggleOn(root.transform);
                var phoneticsText = phonetics != null ? Q<TMP_Text>(root, phonetics) : null;
                var states = States(root, group, variants, normal,
                    codeVisibility: phoneticsText != null ? new[] { phoneticsText.gameObject } : null);

                var le = GetOrAdd<LayoutElement>(root);
                le.minHeight = le.preferredHeight = height;
                if (width > 0f)
                {
                    le.minWidth = le.preferredWidth = width;
                }
                else
                {
                    le.flexibleWidth = 1f;
                }

                // Translations can be long and in another script: shrink the word to fit the card.
                var wordText = Q<TMP_Text>(root, word);
                if (width > 0f)
                {
                    var source = PrefabUtility.GetCorrespondingObjectFromSource(wordText);
                    wordText.enableAutoSizing = true;
                    wordText.fontSizeMax = source != null && !source.enableAutoSizing ? source.fontSize : wordText.fontSizeMax;
                    wordText.fontSizeMin = 16f;
                    wordText.textWrappingMode = TextWrappingModes.Normal;
                    // The text box is as wide as the card now, so center the word like the design does.
                    wordText.alignment = TextAlignmentOptions.Center;
                    var wordLayout = GetOrAdd<LayoutElement>(wordText.gameObject);
                    wordLayout.preferredWidth = width - 24f;
                    wordLayout.minWidth = -1f;
                    var noShrink = wordText.GetComponent<LayoutNoShrink>();
                    if (noShrink != null)
                    {
                        noShrink.enabled = false;
                    }
                }

                var cToggle = GetOrAdd<CToggle>(root);
                Set(cToggle,
                    ("_toggle", toggle),
                    ("_word", Q<TMP_Text>(root, word)),
                    ("_phonetics", phoneticsText),
                    ("_states", states),
                    ("_stateNormal", normal),
                    ("_stateSelected", selected),
                    ("_stateCorrect", correct),
                    ("_stateWrong", wrong),
                    ("_stateInactive", inactive));
            });
            return prefab.GetComponent<CToggle>();
        }

        /// <summary>One lesson of the book: a full-width slot with the LessonNode moved sideways for the zig-zag path.</summary>
        private static GameBookItem BuildLessonItem()
        {
            var prefab = BuildPrefab($"{ItemsRoot}/LessonItem.prefab", root =>
            {
                var le = GetOrAdd<LayoutElement>(root);
                le.minHeight = le.preferredHeight = LessonStep;
                le.flexibleWidth = 1f;

                var node = Instance(Component("LessonNode/Score 25pct"), root.transform, "LessonNode");
                node.anchorMin = node.anchorMax = new Vector2(0.5f, 1f);
                node.pivot = new Vector2(0.5f, 1f);
                node.anchoredPosition = Vector2.zero;
                node.sizeDelta = LessonNodeSize;

                var scoreText = Q<TMP_Text>(node.gameObject, "Score|ScoreText");
                var states = States(node.gameObject, "LessonNode",
                    new[] { "New", "Score 0pct", "Score 25pct", "Score 50pct", "Score 75pct", "Score 100pct" }, "New",
                    stateTexts: new[] { scoreText });

                var button = ButtonOn(Q(node, "Circle"));
                var item = GetOrAdd<GameBookItem>(root);
                Set(item,
                    ("label", Q<TMP_Text>(node.gameObject, "Circle|Number")),
                    ("button", button),
                    ("states", states),
                    ("scoreText", scoreText),
                    ("shift", node));
            });
            return prefab.GetComponent<GameBookItem>();
        }

        private static RectTransform BuildRow()
        {
            var prefab = BuildPrefab($"{ItemsRoot}/LessonRow.prefab", root =>
            {
                var layout = GetOrAdd<VerticalLayoutGroup>(root);
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(16, 16, 8, 8);
            });
            return (RectTransform)prefab.transform;
        }

        /// <summary>Section header: the Expanded variant (it has every layer), collapsing switches to the Collapsed look.</summary>
        private static GameBookSection BuildSectionHeader()
        {
            var prefab = BuildVariant(Component("SectionHeader/Expanded"), $"{ItemsRoot}/SectionHeader.prefab", root =>
            {
                var le = GetOrAdd<LayoutElement>(root);
                le.minHeight = le.preferredHeight = DesignSize(Component("SectionHeader/Expanded")).y;

                // The code hides the learn-language title when the sheet has no Section.<Key>.Learn.
                Show(Q(root, "Titles|Meta"));
                var progressBar = FillBar(Q(root, "Titles|Meta|Bar"));

                var sort = Q(root, "Sort");
                var sortToggle = ToggleOn(sort);
                // The sorted look comes from the Sorted variant; the old overlay is not used any more.
                var oldOverlay = sort.Find("SortOn");
                if (oldOverlay != null)
                {
                    Object.DestroyImmediate(oldOverlay.gameObject);
                }

                var repeat = ButtonOn(Q(root, "RepeatSection"));
                // The whole header collapses and expands the section.
                var collapse = ButtonOn(root.transform);
                collapse.transition = Selectable.Transition.None;
                var states = States(root, "SectionHeader", new[] { "Expanded", "Collapsed", "Sorted" }, "Expanded",
                    codeVisibility: new[] { Q(root, "Titles|TitleRow|Thai").gameObject });

                var section = GetOrAdd<GameBookSection>(root);
                Set(section,
                    ("baseColors", Objects(new[] { Q(root, PenpotUiBuilder.BackgroundName).GetComponent<Image>() })),
                    ("label", Q<TMP_Text>(root, "Titles|TitleRow|Title")),
                    ("sortSectionToggle", sortToggle),
                    ("repeatSectionButton", repeat),
                    ("states", states),
                    ("collapseButton", collapse),
                    ("learnLabel", Q<TMP_Text>(root, "Titles|TitleRow|Thai")),
                    ("progressText", Q<TMP_Text>(root, "Titles|Meta|Progress")),
                    ("progressBar", progressBar));
            });
            return prefab.GetComponent<GameBookSection>();
        }

        /// <summary>White card holding the section header and the lesson rows (the design's "Section · Food").</summary>
        private static SectionBlock BuildSectionBlock()
        {
            var prefab = BuildPrefab($"{ItemsRoot}/SectionBlock.prefab", root =>
            {
                var layout = GetOrAdd<VerticalLayoutGroup>(root);
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(0, 0, 0, 14);

                var image = GetOrAdd<UnityEngine.UI.ProceduralImage.ProceduralImage>(root);
                GetOrAdd<FreeModifier>(root).Radius = new Vector4(24f, 24f, 24f, 24f);
                image.color = Color.white;
                image.raycastTarget = false;

                var block = GetOrAdd<SectionBlock>(root);
                Set(block, ("container", root.transform), ("baseColors", Objects(new Image[0])));
            });
            return prefab.GetComponent<SectionBlock>();
        }

        private static OverviewLogItem BuildLogItem()
        {
            var prefab = BuildVariant(Component("LogItem/LogItem"), $"{ItemsRoot}/LogItem.prefab", root =>
            {
                // The view puts the translation under the word in one text.
                Hide(root, "Text|Translation");
                Hide(root, "Meta|History");
                var item = GetOrAdd<OverviewLogItem>(root);
                Set(item,
                    ("text", Q<TMP_Text>(root, "Text|Thai")),
                    ("mark", Q<TMP_Text>(root, "Mark|Value")),
                    ("date", Q<TMP_Text>(root, "Meta|Date")),
                    ("totalShown", null),
                    ("timeStep", null));
            });
            return prefab.GetComponent<OverviewLogItem>();
        }

        private static ResultItem BuildResultRow()
        {
            var prefab = BuildVariant(Component("ResultRow/Up"), $"{ItemsRoot}/ResultRow.prefab", root =>
            {
                Hide(root, "Text|Translation");
                var meter = Q(root, "MasteryMeter");
                var segments = meter.Cast<Transform>()
                    .Where(t => t.name.StartsWith("seg"))
                    .OrderBy(t => int.Parse(t.name.Substring(3)))
                    .Select(t => (t.Find(PenpotUiBuilder.BackgroundName) ?? t).GetComponent<Graphic>())
                    .ToList();
                var states = States(root, "ResultRow", new[] { "Up", "Down" }, "Up",
                    codeVisibility: new[] { Q(root, "Text|Translation").gameObject });

                var item = GetOrAdd<ResultItem>(root);
                Set(item,
                    ("_word", Q<TMP_Text>(root, "Text|Thai")),
                    ("_mark", null),
                    ("_changeUp", null),
                    ("_changeDown", null),
                    ("_bg", null),
                    ("_states", states),
                    ("_change", Q(root, "Delta").gameObject),
                    ("_meterSegments", Objects(segments)),
                    ("_meterOnColor", segments[0].color),
                    ("_meterOffColor", segments[segments.Count - 1].color));
            });
            return prefab.GetComponent<ResultItem>();
        }
    }
}

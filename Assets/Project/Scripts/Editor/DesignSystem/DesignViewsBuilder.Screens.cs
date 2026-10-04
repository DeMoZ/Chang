using System.Linq;
using Chang.GameBook;
using Chang.Sentences;
using Chang.UI;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.Editor.DesignSystem
{
    public static partial class DesignViewsBuilder
    {
        // Section header colors of "Words · Book", used in turn for the sections.
        private static readonly string[] SectionColors = { "#2f3c9e", "#14916b", "#d9487a", "#b87a0e", "#3a73d6", "#8b57d1", "#0e8a8a" };

        // ---- main screen with tabs ------------------------------------------------------------------------

        private static void BuildMainUi(Items items)
        {
            var vocabulary = BuildBook<BookVocabularyView>("Words - Book", "BookVocabularyView", items, "Lobby.Tab.Words", null);
            var sentences = BuildBook<BookSentencesView>("Sentences - Book", "BookSentencesView", items, "Lobby.Tab.Sentences", "Content|Preview");
            var repetition = BuildRepetition(items);
            var profile = BuildProfile();

            BuildPrefab($"{ViewsRoot}/MainUI.prefab", root =>
            {
                var content = CanvasRoot(root, out var column);

                var tabs = Child(content, "Tabs", 0);
                Stretch(tabs);
                foreach (var tabView in new[] { vocabulary, sentences, repetition, profile })
                {
                    Stretch(Instance(tabView, tabs, tabView.name));
                }

                var tabBar = Instance(Component("TabBar/Words"), content, "TabBar");
                AnchorBottom(tabBar, DesignSize(Component("TabBar/Words")).y, 0f);
                var group = GetOrAdd<ToggleGroup>(tabBar);
                group.allowSwitchOff = false;
                var states = States(tabBar.gameObject, "TabBar", new[] { "Words", "Sentences", "Repeat", "Profile" }, "Words");

                TabToggle Tab(string name, string key)
                {
                    var t = Q(tabBar, $"Tab / {name}");
                    var toggle = ToggleOn(t);
                    toggle.group = group;
                    var label = Q(t, "Label");
                    Localize(label, key);
                    FitWidth(label, DesignSize(Component("TabBar/Words")).x / 4f - 12f);
                    var tabToggle = GetOrAdd<TabToggle>(t);
                    Set(tabToggle, ("_toggle", toggle), ("_word", label.GetComponent<TMP_Text>()));
                    return tabToggle;
                }

                var view = GetOrAdd<MainUiView>(root);
                Set(view,
                    ("content", column),
                    ("toggleGroup", group),
                    ("vocabularyToggle", Tab("Words", "Lobby.Tab.Words")),
                    ("sentencesToggle", Tab("Sentences", "Lobby.Tab.Sentences")),
                    ("repetitionToggle", Tab("Repeat", "Lobby.Tab.Repeat")),
                    ("profileToggle", Tab("Profile", "Lobby.Tab.Profile")),
                    ("tabBarStates", states));
            });
        }

        /// <summary>
        /// Canvas (540×1080 design units, expanded to fit any screen), background, safe area and a centered column
        /// of limited width for landscape screens. Returns the column content.
        /// </summary>
        private static RectTransform CanvasRoot(GameObject root, out RectTransform column)
        {
            var canvas = GetOrAdd<Canvas>(root);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 |
                                              AdditionalCanvasShaderChannels.TexCoord3;
            ConfigureScaler(GetOrAdd<CanvasScaler>(root));
            GetOrAdd<GraphicRaycaster>(root);

            var background = Child(root.transform, "Background", 0);
            Stretch(background);
            var bgImage = GetOrAdd<Image>(background);
            bgImage.color = PenpotDocument.ParseColor("#fbf6ec");
            bgImage.raycastTarget = false;

            var safeArea = Child(root.transform, "SafeArea", 1);
            Stretch(safeArea);
            GetOrAdd<SafeArea.SafeArea>(safeArea);

            column = Child(safeArea, "Column", 0);
            GetOrAdd<WidthLimiter>(column);
            return column;
        }

        public static void ConfigureScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        /// <summary>A vocabulary or sentences book: the design screen made scrollable, sections created by the view.</summary>
        private static GameObject BuildBook<TView>(string screen, string name, Items items, string titleKey, string extraToHide) where TView : BookView
        {
            return BuildVariant(Screen(screen), $"{ViewsRoot}/{name}.prefab", root =>
            {
                var content = (RectTransform)Q(root, "Content");
                Hide(root, $"Chang DS / TabBar / {(typeof(TView) == typeof(BookSentencesView) ? "Sentences" : "Words")}");
                HideAll(content.Cast<Transform>().Where(t => t.name.StartsWith("Chang DS") || t.name.StartsWith("Section ·")));
                Hide(root, "Content|Header|StreakChip");
                if (extraToHide != null)
                {
                    Hide(root, extraToHide);
                }

                Localize(Q(root, "Content|Header|Titles|Title"), titleKey);
                var scroll = Scroll(root, content, TabBarClearance);

                var sectionsColors = new Gradient();
                sectionsColors.SetKeys(
                    SectionColors.Select((hex, i) => new GradientColorKey(PenpotDocument.ParseColor(hex), i / (float)(SectionColors.Length - 1))).ToArray(),
                    new[] { new GradientAlphaKey(1f, 0f) });
                var markColors = new Gradient();
                markColors.SetKeys(new[]
                {
                    new GradientColorKey(PenpotDocument.ParseColor("#e04f4a"), 0f),
                    new GradientColorKey(PenpotDocument.ParseColor("#e3a21a"), 0.5f),
                    new GradientColorKey(PenpotDocument.ParseColor("#14916b"), 1f)
                }, new[] { new GradientAlphaKey(1f, 0f) });

                var view = GetOrAdd<TView>(root);
                Set(view,
                    ("scrollRect", scroll),
                    ("sectionBlockPrefab", items.SectionBlock),
                    ("rowPrefab", items.Row),
                    ("sectionPrefab", items.Section),
                    ("upLessonPrefab", items.Lesson),
                    ("downLessonPrefab", items.Lesson),
                    ("content", content),
                    ("sectionsColors", sectionsColors),
                    ("lessonMarkColors", markColors),
                    ("lessonPathOffsets", LessonPathOffsets));
            });
        }

        private static GameObject BuildRepetition(Items items)
        {
            return BuildVariant(Screen("Repeat"), $"{ViewsRoot}/RepetitionView.prefab", root =>
            {
                var content = (RectTransform)Q(root, "Content");
                Hide(root, "Chang DS / TabBar / Repeat");
                HideAll(Children(content, "Chang DS / LogItem"));
                // Counters have no data source yet (the old view never filled them).
                Hide(root, "Content|Stats");
                Hide(root, "Content|Chang DS / DueCard|Title");
                Hide(root, "Content|Chang DS / DueCard|Next");

                var log = Child(content, "Log");
                var logLayout = GetOrAdd<VerticalLayoutGroup>(log);
                logLayout.childControlWidth = logLayout.childControlHeight = true;
                logLayout.childForceExpandWidth = true;
                logLayout.childForceExpandHeight = false;
                logLayout.spacing = 12f;
                var logElement = GetOrAdd<LayoutElement>(log);
                logElement.flexibleWidth = 1f;

                Scroll(root, content, TabBarClearance);

                var segmented = Q(root, "Content|Chang DS / Segmented / Repeat mode");
                var segments = new[] { "Words", "Sentences", "Mixed" }.Select(n => (RectTransform)Q(segmented, n)).ToList();
                foreach (var segment in segments)
                {
                    ButtonOn(segment);
                }

                Localize(Q(segments[0], "Words"), "Lobby.Repetition.Words");
                Localize(Q(segments[1], "Sentences"), "Lobby.Repetition.Sentences");
                Localize(Q(segments[2], "Mixed"), "Lobby.Repetition.Mixed");

                var selection = GetOrAdd<DesignSelection>(segmented);
                Set(selection, ("_items", Objects(segments)), ("_designSelectedIndex", 0), ("_designNormalIndex", 1));

                var review = ButtonOn(Q(root, "Content|Chang DS / DueCard|Start"));

                var view = GetOrAdd<RepetitionView>(root);
                Set(view,
                    ("questions", null),
                    ("words", null),
                    ("sentences", null),
                    ("logContainer", log),
                    ("overviewLogItemPrefab", items.LogItem),
                    ("repeatWordsBtn", null),
                    ("repeatSentencesBtn", null),
                    ("repeatMixedBtn", null),
                    ("modeSelection", selection),
                    ("reviewBtn", review));
            });
        }

        private static GameObject BuildProfile()
        {
            return BuildVariant(Screen("Profile"), $"{ViewsRoot}/ProfileView.prefab", root =>
            {
                var content = (RectTransform)Q(root, "Content");
                Hide(root, "Chang DS / TabBar / Profile");
                // No data for streak, daily goal and the mascot editor yet.
                Hide(root, "Content|Cards");
                Hide(root, "Content|Hero|Avatar|EditMascot");
                var rows = Q(root, "Content|Rows");
                var nameRows = Children(rows, "Chang DS / ProfileRow / Name").ToList();
                // The first "Name" row of the design is "My mascot", the second one is the name.
                Hide(nameRows[0]);
                var nameRow = nameRows[1];
                var languageRow = Q(rows, "Chang DS / ProfileRow / Language");
                Scroll(root, content, TabBarClearance);

                var logout = Q(root, "Content|Chang DS / Button / Ghost");
                Localize(Q(logout, "Label"), "Lobby.Profile.LogOut");

                var card = Q(root, "Content|Chang DS / PolitenessCard");
                var options = Q(card, "Options");
                var selection = GetOrAdd<DesignSelection>(card);
                Set(selection, ("_items", Objects(new[] { (RectTransform)Q(options, "Male"), (RectTransform)Q(options, "Female") })),
                    ("_designSelectedIndex", 0), ("_designNormalIndex", 1));

                var nameText = Q(root, "Content|Hero|Name").Cast<Transform>().First().GetComponent<TMP_Text>();

                var view = GetOrAdd<ProfileView>(root);
                Set(view,
                    ("logoutBtn", logout.GetComponent<Button>()),
                    ("userNameText", Q<TMP_Text>(nameRow.gameObject, "Value")),
                    ("userNameTitle", nameText),
                    ("userIdText", null),
                    ("changeNameBtn", ButtonOn(nameRow)),
                    ("genderText", null),
                    ("changeGenderBtn", ButtonOn(card)),
                    ("languageText", Q<TMP_Text>(languageRow.gameObject, "Value")),
                    ("changeLanguageBtn", ButtonOn(languageRow)),
                    ("genderSelection", selection));
            });
        }

        // ---- lesson ---------------------------------------------------------------------------------------

        /// <summary>Top bar, Check button and the feedback sheet over the lesson pages.</summary>
        private static void BuildOverlay()
        {
            BuildPrefab($"{ViewsRoot}/GameOverlay.prefab", root =>
            {
                Stretch((RectTransform)root.transform);
                var column = Child(root.transform, "Column", 0);
                GetOrAdd<WidthLimiter>(column);

                var topBar = Instance(Component("LessonTopBar/LessonTopBar"), column, "TopBar");
                topBar.SetSiblingIndex(0);
                AnchorTop(topBar, DesignSize(Component("LessonTopBar/LessonTopBar")).y, 8f);
                // No lesson progress data yet.
                Hide(topBar.gameObject, "Progress|Value");

                var check = Instance(Component("Button/Primary"), column, "Check");
                AnchorBottom(check, 64f, 24f, 440f);

                var blocker = Blocker(column, "Blocker");
                blocker.transform.SetSiblingIndex(check.GetSiblingIndex() + 1);

                var sheet = Instance(Component("FeedbackSheet/Wrong"), column, "Feedback");
                AnchorBottom(sheet, DesignSize(Component("FeedbackSheet/Wrong")).y, 0f);
                var title = Q<TMP_Text>(sheet.gameObject, "Head|Title");
                var states = States(sheet.gameObject, "FeedbackSheet", new[] { "Correct", "Wrong" }, "Correct", stateTexts: new[] { title });
                var continueView = GetOrAdd<PagesContinueView>(sheet);
                Set(continueView,
                    ("_continueBtn", Q<Button>(sheet.gameObject, "Chang DS / Button / Success")),
                    ("_wrongContinueBtn", Q<Button>(sheet.gameObject, "Chang DS / Button / Danger")),
                    ("_info", Q<TMP_Text>(sheet.gameObject, "Answer|Meaning")),
                    ("_infoTitle", Q<TMP_Text>(sheet.gameObject, "Answer|Thai")),
                    ("_background", null),
                    ("_states", states));

                // Everything is shown by the lesson states; nothing is visible in the lobby.
                foreach (var hidden in new[] { topBar, check, (RectTransform)blocker.transform, sheet })
                {
                    Hide(hidden);
                }

                var view = GetOrAdd<GameOverlayView>(root);
                Set(view,
                    ("_topBar", topBar.gameObject),
                    ("_blocker", blocker.gameObject),
                    ("_returnBtn", Q<Button>(topBar.gameObject, "Chang DS / IconButton / Close")),
                    ("_hintBtn", Q<Button>(topBar.gameObject, "Chang DS / IconButton / Hint")),
                    ("_checkBtn", check.GetComponent<Button>()),
                    ("_continue", continueView));
            });
        }

        /// <summary>Lesson page: the design screen without the parts the overlay shows (top bar, Check button).</summary>
        private static GameObject BuildPage(string screen, string name, System.Action<GameObject, RectTransform> build)
        {
            return BuildVariant(Screen(screen), $"{ViewsRoot}/{name}.prefab", root =>
            {
                foreach (Transform t in root.transform)
                {
                    if (t.name.StartsWith("Chang DS / LessonTopBar") || t.name.StartsWith("Chang DS / Button /"))
                    {
                        Hide(t);
                    }
                }

                var content = (RectTransform)Q(root, "Content");
                RemoveStatusBarOffset(content);
                build(root, content);
            });
        }

        private static void BuildPages(Items items)
        {
            BuildPage("Lesson - New word", "DemonstrationWordView", (root, content) =>
            {
                var card = Q(content, "Chang DS / WordCard");
                SingleLine(Q(card, "Phonetics"));
                var questionWord = GetOrAdd<ChangText>(card);
                Set(questionWord, ("_word", Q<TMP_Text>(card.gameObject, "Thai")), ("_phonetic", Q<TMP_Text>(card.gameObject, "Phonetics")));

                var actions = Q(card, "Actions");
                Hide(Q(actions, "Chang DS / IconButton / Sound Slow"));
                var group = GetOrAdd<ToggleGroup>(actions);
                group.allowSwitchOff = true;

                var reveal = Q(actions, "Reveal");
                var revealToggle = ToggleOn(reveal);
                revealToggle.graphic = Highlight(reveal, PenpotDocument.ParseColor("#e6e8f8"), 28f);
                var translation = GetOrAdd<CToggle>(reveal);
                Set(translation, ("_toggle", revealToggle), ("_word", Q<TMP_Text>(reveal.gameObject, "Translation")), ("_phonetics", null));

                var view = GetOrAdd<DemonstrationWordView>(root);
                Set(view,
                    ("_questionImage", ContentImage(Q(card, "Image"), "Illustration")),
                    ("_questionWord", questionWord),
                    ("_mixWordPrefab", items.Option),
                    ("_mixWordContent", actions),
                    ("_toggleGroup", group),
                    ("_playStopBtn", PlayStop(Q(actions, "Chang DS / IconButton / Sound"))),
                    ("_translationToggle", translation));
            });

            BuildPage("Lesson - Select word", "SelectWordView", (root, content) =>
            {
                var prompt = Q(content, "Prompt");
                var questionWord = GetOrAdd<ChangText>(prompt);
                Set(questionWord, ("_word", Q<TMP_Text>(prompt.gameObject, "Title")), ("_phonetic", Q<TMP_Text>(prompt.gameObject, "Phonetics")));

                var options = Q(content, "Options");
                HideAll(options.Cast<Transform>());
                var group = GetOrAdd<ToggleGroup>(options);
                group.allowSwitchOff = true;

                var view = GetOrAdd<SelectWordView>(root);
                Set(view,
                    ("_questionImage", ContentImage(Q(prompt, "Picture"), "Illustration")),
                    ("_questionWord", questionWord),
                    ("_mixWordPrefab", items.Option),
                    ("_mixWordContent", options),
                    ("_toggleGroup", group),
                    ("_playStopBtn", null));
            });

            BuildPage("Lesson - Match pairs", "MatchWordsView", (root, content) =>
            {
                var columns = Q(content, "Columns");
                var left = Q(columns, "Thai");
                var right = Q(columns, "Translation");
                HideAll(left.Cast<Transform>());
                HideAll(right.Cast<Transform>());
                var leftGroup = GetOrAdd<ToggleGroup>(left);
                leftGroup.allowSwitchOff = true;
                var rightGroup = GetOrAdd<ToggleGroup>(right);
                rightGroup.allowSwitchOff = true;

                // The design has no button on this page; the view shows Continue when all pairs are matched.
                var continueButton = Instance(Component("Button/Primary"), root.transform, "Continue");
                AnchorBottom(continueButton, 64f, 24f, 440f);
                Q<TMP_Text>(continueButton.gameObject, "Label").text = "Continue";

                var view = GetOrAdd<MatchWordsView>(root);
                Set(view,
                    ("_matchWordPrefab", items.Match),
                    ("_leftWordsContent", left),
                    ("_rightWordsContent", right),
                    ("_leftTogglesGroup", leftGroup),
                    ("_rightTogglesGroup", rightGroup),
                    ("_continuteBtn", continueButton.GetComponent<Button>()));
            });

            BuildPage("Lesson - Build sentence", "SentenceSelectWordView", (root, content) =>
            {
                var card = Q(content, "Card");
                var sentence = Q(content, "Sentence");
                var pool = Q(content, "Pool");
                HideAll(sentence.Cast<Transform>());
                HideAll(pool.Cast<Transform>());
                var sentenceGroup = GetOrAdd<ToggleGroup>(sentence);
                sentenceGroup.allowSwitchOff = true;
                var poolGroup = GetOrAdd<ToggleGroup>(pool);
                poolGroup.allowSwitchOff = true;

                var translation = card.Cast<Transform>().First(t => t.GetComponent<TMP_Text>() != null);

                var view = GetOrAdd<SentenceSelectWordView>(root);
                Set(view,
                    ("_questionImage", ContentImage(Q(card, "Pic"), "Illustration")),
                    ("_translation", translation.GetComponent<TMP_Text>()),
                    ("_displaySequenceContent", sentence),
                    ("_mixSequenceContent", pool),
                    ("_displayWordPrefab", items.ChipFixed),
                    ("_mixWordPrefab", items.Chip),
                    ("_displayTogglesGroup", sentenceGroup),
                    ("_mixTogglesGroup", poolGroup),
                    ("_playStopBtn", PlayStop(Q(card, "Chang DS / IconButton / Sound"))));
            });

            BuildVariant(Screen("Lesson - Complete"), $"{ViewsRoot}/PlayResultView.prefab", root =>
            {
                var content = (RectTransform)Q(root, "Content");
                RemoveStatusBarOffset(content);
                // Totals and the lesson number have no data source yet.
                Hide(content, "Stats");
                Hide(content, "Lesson 4 complete");
                var list = Q(content, "List");
                HideAll(list.Cast<Transform>());
                Scroll(root, content, 120f);

                var continueButton = root.transform.Cast<Transform>().First(t => t.name.StartsWith("Chang DS / Button /"));
                var view = GetOrAdd<PlayResultView>(root);
                Set(view,
                    ("_contentParent", list),
                    ("_itemPrefab", items.ResultRow),
                    ("_continuteBtn", continueButton.GetComponent<Button>()));
            });
        }

        private static PlayStopButton PlayStop(Transform iconButton)
        {
            var playStop = GetOrAdd<PlayStopButton>(iconButton);
            var icon = iconButton.Cast<Transform>().FirstOrDefault(t => t.name.StartsWith("icon /") && t.gameObject.activeSelf);
            Set(playStop, ("button", iconButton.GetComponent<Button>()), ("playObject", icon != null ? icon.gameObject : null), ("stopObject", null));
            return playStop;
        }

        private static void Hide(Transform root, string path) => Hide(Q(root, path));

        /// <summary>One-line text that shrinks (down from its design size) to fit the given width: long words and translations.</summary>
        private static void FitWidth(Transform text, float width)
        {
            var tmp = text.GetComponent<TMP_Text>();
            // Auto-size rewrites the current size, so the design size is read from the generated prefab.
            var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(tmp);
            var designSize = source != null && !source.enableAutoSizing ? source.fontSize : tmp.fontSizeMax;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = designSize;
            tmp.fontSizeMin = Mathf.Min(14f, designSize);
            // With wrapping on and the height of one line, a word that is too wide makes TMP shrink the size.
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            var le = GetOrAdd<LayoutElement>(text);
            le.minWidth = le.preferredWidth = width;
            le.minHeight = le.preferredHeight = Mathf.Ceil(designSize * 1.3f);
            var noShrink = text.GetComponent<LayoutNoShrink>();
            if (noShrink != null)
            {
                noShrink.enabled = false;
            }
        }

        /// <summary>A design text with a fixed width that should grow with its content instead of wrapping.</summary>
        private static void SingleLine(Transform text)
        {
            text.GetComponent<TMP_Text>().textWrappingMode = TextWrappingModes.NoWrap;
            var le = GetOrAdd<LayoutElement>(text);
            le.minWidth = le.preferredWidth = -1f;
            GetOrAdd<LayoutNoShrink>(text);
        }

        /// <summary>A rounded overlay behind the content of a toggle, shown while it is on.</summary>
        private static Graphic Highlight(Transform target, Color color, float radius)
        {
            var on = Child(target, "On", 1);
            Stretch(on);
            GetOrAdd<LayoutElement>(on).ignoreLayout = true;
            var image = GetOrAdd<UnityEngine.UI.ProceduralImage.ProceduralImage>(on);
            GetOrAdd<FreeModifier>(on).Radius = Vector4.one * radius;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // ---- loading ----------------------------------------------------------------------------------------

        private static void BuildLoading()
        {
            BuildVariant(Screen("Loading"), $"{ViewsRoot}/LoadingScreen.prefab", root =>
            {
                var progress = Q(root, "Progress");
                // A fixed fact in English: no localization for it yet.
                Hide(progress, "Tip");
                var bar = Q(progress, "Bar");
                var fillBar = GetOrAdd<LoadingFillBar>(bar);
                Set(fillBar, ("_fill", (RectTransform)Q(bar, "Value")));
                var percent = progress.Cast<Transform>().First(t => t.GetComponent<TMP_Text>() != null);
                var blocker = Blocker(root.transform, "Blocker", 0);

                var view = GetOrAdd<LoadingUiView>(root);
                Set(view,
                    ("background", Q(root, PenpotUiBuilder.BackgroundName).gameObject),
                    ("blocker", blocker.gameObject),
                    ("percents", percent.GetComponent<TMP_Text>()),
                    ("bytes", null),
                    ("progressSlider", fillBar),
                    ("loadingAnimation", Q(root, "Brand|Mascot / Chang").gameObject));
            });
        }
    }
}

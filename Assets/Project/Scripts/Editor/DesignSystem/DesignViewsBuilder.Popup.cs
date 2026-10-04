using Popup;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.Editor.DesignSystem
{
    public static partial class DesignViewsBuilder
    {
        /// <summary>
        /// Generic popup of PopupManager in the look of the design's Dialog / Confirm card.
        /// Its elements (header, label, input, buttons) are separate prefabs that PopupView fills in at runtime.
        /// </summary>
        private static void BuildPopup()
        {
            var dialog = Component("Dialog/Confirm");
            var titleSource = Q<TMP_Text>(dialog, "Title");
            var bodySource = Q<TMP_Text>(dialog, "Body");

            var header = BuildPrefab($"{PopupRoot}/Header.prefab", root =>
            {
                var text = CopyText(root, titleSource);
                Set(GetOrAdd<HeaderView>(root), ("text", text));
            });

            var label = BuildPrefab($"{PopupRoot}/Label.prefab", root =>
            {
                var text = CopyText(root, bodySource);
                Set(GetOrAdd<LabelView>(root), ("text", text));
            });

            var button = BuildVariant(Component("Button/Primary"), $"{PopupRoot}/Button.prefab", root =>
            {
                var le = GetOrAdd<LayoutElement>(root);
                le.minHeight = le.preferredHeight = 56f;
                le.flexibleWidth = 1f;
                Set(GetOrAdd<ButtonView>(root), ("text", Q<TMP_Text>(root, "Label")), ("button", root.GetComponent<Button>()));
            });

            var buttons = BuildPrefab($"{PopupRoot}/ButtonsContainer.prefab", root =>
            {
                var layout = GetOrAdd<HorizontalLayoutGroup>(root);
                layout.spacing = 12f;
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            });

            var input = BuildVariant(Component("TextField/TextField"), $"{PopupRoot}/LabelAndInput.prefab", root =>
            {
                var field = Q(root, "Field");
                var placeholder = Q<TMP_Text>(root, "Field|Placeholder");

                var area = Child(field, "Text Area");
                Stretch(area, 20f, 20f, 6f, 6f);
                GetOrAdd<LayoutElement>(area).ignoreLayout = true;
                GetOrAdd<RectMask2D>(area);

                var textRt = Child(area, "Text");
                Stretch(textRt);
                var text = GetOrAdd<TextMeshProUGUI>(textRt);
                text.font = placeholder.font;
                text.fontSize = placeholder.fontSize;
                text.color = PenpotDocument.ParseColor("#1d2140");
                text.alignment = TextAlignmentOptions.Left;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.raycastTarget = false;

                var inputField = GetOrAdd<TMP_InputField>(field);
                inputField.textViewport = area;
                inputField.textComponent = text;
                inputField.placeholder = placeholder;
                inputField.targetGraphic = field.Find(PenpotUiBuilder.BackgroundName)?.GetComponent<Graphic>();
                inputField.fontAsset = placeholder.font;
                inputField.pointSize = placeholder.fontSize;

                Set(GetOrAdd<LabelAndInputView>(root), ("labelText", Q<TMP_Text>(root, "Label")), ("inputField", inputField));
            });

            BuildPrefab($"{PopupRoot}/PopupView.prefab", root =>
            {
                Stretch((RectTransform)root.transform);
                var dim = Blocker(root.transform, "Blocker", 0);
                dim.color = new Color(0.114f, 0.129f, 0.251f, 0.45f);

                var card = Instance(dialog, root.transform, "Popup");
                card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
                card.pivot = new Vector2(0.5f, 0.5f);
                card.anchoredPosition = Vector2.zero;
                var fitter = GetOrAdd<ContentSizeFitter>(card);
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                // The card keeps its look; its sample content is hidden, the elements go to a separate
                // container because PopupView.Clear destroys every child of the content.
                foreach (Transform t in card)
                {
                    if (!t.name.StartsWith("#") && t.name != "Content")
                    {
                        Hide(t);
                    }
                }

                var content = Child(card, "Content");
                var layout = GetOrAdd<VerticalLayoutGroup>(content);
                layout.spacing = 16f;
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                GetOrAdd<LayoutElement>(content).flexibleWidth = 1f;

                Set(GetOrAdd<PopupView>(root),
                    ("headerPrefab", header.GetComponent<HeaderView>()),
                    ("labelPrefab", label.GetComponent<LabelView>()),
                    ("labelAndInputPrefab", input.GetComponent<LabelAndInputView>()),
                    ("buttonPrefab", button.GetComponent<ButtonView>()),
                    ("buttonsContainerPrefab", buttons.transform),
                    ("content", content));
            });
        }

        private static TMP_Text CopyText(GameObject root, TMP_Text source)
        {
            var text = GetOrAdd<TextMeshProUGUI>(root);
            EditorUtility.CopySerialized(source, text);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            var le = GetOrAdd<LayoutElement>(root);
            le.flexibleWidth = 1f;
            return text;
        }
    }
}

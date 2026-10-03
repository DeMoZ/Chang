using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.Localization
{
    [CustomEditor(typeof(LocalizationSheetView))]
    public class LocalizationSheetViewEditor : UnityEditor.Editor
    {
        private const float Indent = 14f;
        private const float TreeHeight = 250f;
        private const float KeysHeight = 400f;
        private const float KeyColumnWidth = 280f;
        private const float ValueColumnWidth = 160f;

        private static readonly Color SelectedColor = new(0.24f, 0.48f, 0.9f, 0.35f);

        private readonly HashSet<string> _expanded = new();

        private TextAsset _parsedCsv;
        private Hash128 _parsedHash;
        private char _parsedSeparator;
        private LocalizationSheetData _data;

        private string _selectedPath = string.Empty;
        private string _search = string.Empty;
        private Vector2 _treeScroll;
        private Vector2 _keysScroll;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var view = (LocalizationSheetView)target;

            if (!view.Csv)
            {
                EditorGUILayout.HelpBox("Drop a sheet csv from the SimpleLocalization save folder (Resources/Localization)", MessageType.Info);
                return;
            }

            RefreshData(view, false);

            EditorGUILayout.Space();
            DrawSummary(view);

            foreach (string error in _data.Errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Keys hierarchy", EditorStyles.boldLabel);
            DrawTree();

            EditorGUILayout.Space();
            DrawKeys();
        }

        private void RefreshData(LocalizationSheetView view, bool force)
        {
            TextAsset csv = view.Csv;
            char separator = view.KeySeparatorChar;
            Hash128 hash = AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(csv));

            if (!force && _data != null && _parsedCsv == csv && _parsedHash == hash && _parsedSeparator == separator)
            {
                return;
            }

            // tree paths are built with the separator, so the expanded and selected paths are stale after its change
            bool isNewTree = _data == null || _parsedCsv != csv || _parsedSeparator != separator;

            _parsedCsv = csv;
            _parsedHash = hash;
            _parsedSeparator = separator;
            _data = LocalizationSheetData.Parse(csv.text, separator);

            if (isNewTree)
            {
                _expanded.Clear();
                _expanded.Add(_data.Root.Path);
                _selectedPath = string.Empty;
            }
        }

        private void DrawSummary(LocalizationSheetView view)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Keys: {_data.Entries.Count}, languages: {_data.Languages.Count}", EditorStyles.boldLabel);

                if (GUILayout.Button("Reload", GUILayout.Width(70)))
                {
                    RefreshData(view, true);
                }
            }

            EditorGUILayout.LabelField(string.Join(", ", _data.Languages), EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawTree()
        {
            using var scroll = new EditorGUILayout.ScrollViewScope(_treeScroll, EditorStyles.helpBox, GUILayout.Height(TreeHeight));
            _treeScroll = scroll.scrollPosition;

            DrawNode(_data.Root, 0, "All keys");
        }

        private void DrawNode(LocalizationKeyNode node, int depth, string label = null)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));

            if (node.Path == _selectedPath)
            {
                EditorGUI.DrawRect(rect, SelectedColor);
            }

            rect.xMin += depth * Indent;
            bool hasChildren = node.Children.Count > 0;
            bool isExpanded = _expanded.Contains(node.Path);

            if (hasChildren)
            {
                bool expanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, Indent, rect.height), isExpanded, GUIContent.none, true);

                if (expanded != isExpanded)
                {
                    SetExpanded(node.Path, expanded);
                    isExpanded = expanded;
                }
            }

            var labelRect = new Rect(rect.x + Indent, rect.y, rect.width - Indent, rect.height);
            string text = label ?? node.Name;
            GUI.Label(labelRect, hasChildren ? $"{text}  ({node.KeysCount})" : text,
                node.Entry != null && !hasChildren ? EditorStyles.label : EditorStyles.boldLabel);

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                _selectedPath = node.Path;

                if (Event.current.clickCount == 2 && hasChildren)
                {
                    SetExpanded(node.Path, !isExpanded);
                }

                Event.current.Use();
                Repaint();
            }

            if (!isExpanded)
            {
                return;
            }

            foreach (LocalizationKeyNode child in node.Children)
            {
                DrawNode(child, depth + 1);
            }
        }

        private void SetExpanded(string path, bool expanded)
        {
            if (expanded)
            {
                _expanded.Add(path);
            }
            else
            {
                _expanded.Remove(path);
            }
        }

        private void DrawKeys()
        {
            LocalizationKeyNode selected = FindNode(_data.Root, _selectedPath) ?? _data.Root;
            List<LocalizationEntry> entries = selected.GetEntries().Where(IsMatchSearch).ToList();

            string title = selected == _data.Root ? "All keys" : selected.Path;
            EditorGUILayout.LabelField($"{title}: {entries.Count}", EditorStyles.boldLabel);
            _search = EditorGUILayout.TextField("Search", _search);

            using var scroll = new EditorGUILayout.ScrollViewScope(_keysScroll, EditorStyles.helpBox, GUILayout.Height(KeysHeight));
            _keysScroll = scroll.scrollPosition;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(KeyColumnWidth));

                foreach (string language in _data.Languages)
                {
                    GUILayout.Label(language, EditorStyles.boldLabel, GUILayout.Width(ValueColumnWidth));
                }
            }

            float height = EditorGUIUtility.singleLineHeight;

            foreach (LocalizationEntry entry in entries)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.SelectableLabel(entry.Key, GUILayout.Width(KeyColumnWidth), GUILayout.Height(height));

                    for (var i = 0; i < _data.Languages.Count; i++)
                    {
                        string value = entry.GetValue(i);
                        EditorGUILayout.SelectableLabel(value == null ? "<missing>" : value.Replace("\n", "\\n"),
                            GUILayout.Width(ValueColumnWidth), GUILayout.Height(height));
                    }
                }
            }
        }

        private bool IsMatchSearch(LocalizationEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_search))
            {
                return true;
            }

            return entry.Key.Contains(_search, StringComparison.OrdinalIgnoreCase)
                   || entry.Values.Any(value => value.Contains(_search, StringComparison.OrdinalIgnoreCase));
        }

        private static LocalizationKeyNode FindNode(LocalizationKeyNode node, string path)
        {
            if (node.Path == path)
            {
                return node;
            }

            return node.Children
                .Where(child => path.StartsWith(child.Path, StringComparison.Ordinal))
                .Select(child => FindNode(child, path))
                .FirstOrDefault(found => found != null);
        }
    }
}

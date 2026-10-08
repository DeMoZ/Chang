using System.Collections.Generic;
using Chang.Mascot;
using Chang.Profile;
using Chang.Services.DataProvider;
using UnityEditor;
using UnityEngine;

namespace Chang.Editor.Inspector
{
    /// <summary>
    /// Read-only view of <see cref="PrefsDataViewEditor"/>: the values can't be changed, lists and texts can still be opened,
    /// scrolled and copied. The mascot is drawn as a picture (the game's own SVG generator) with the chosen option of every part.
    /// </summary>
    [CustomEditor(typeof(PrefsDataViewEditor))]
    public class PrefsDataViewInspector : UnityEditor.Editor
    {
        private const int PictureSize = 160;
        private const int MaxTextHeight = 400;
        private const string PartsExpandedKey = "Chang.PrefsDataViewInspector.MascotPartsExpanded";

        private readonly Dictionary<string, Vector2> _scrolls = new();
        private Texture2D _picture;
        private string _pictureLook;
        private string _pictureError;

        private void OnEnable()
        {
            // Outside Play mode the view shows what PlayerPrefs hold now, not what the last save wrote into the asset.
            // Not saved: the asset changes only when the game saves.
            if (!Application.isPlaying)
            {
                var view = (PrefsDataViewEditor)target;
                view.Refresh(view.LearnLanguage);
            }
        }

        private void OnDisable()
        {
            DestroyPicture();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var view = (PrefsDataViewEditor)target;

            var property = serializedObject.GetIterator();
            var enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.name == nameof(PrefsDataViewEditor.Mascot))
                {
                    DrawMascot(view.Mascot);
                    continue;
                }

                DrawReadOnly(property, null);
            }
        }

        private void DrawReadOnly(SerializedProperty property, string label)
        {
            var content = new GUIContent(label ?? property.displayName, property.tooltip);

            if (property.propertyType == SerializedPropertyType.String && property.name == "Json")
            {
                DrawText(property);
                return;
            }

            if (!property.hasVisibleChildren)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(property, content, false);
                }

                return;
            }

            if (property.isArray)
            {
                content.text += $" ({property.arraySize})";
            }

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, content, true);
            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            if (property.isArray)
            {
                for (var i = 0; i < property.arraySize; i++)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    // log entries are named by their key
                    var key = element.FindPropertyRelative("Key");
                    DrawReadOnly(element, key != null ? key.stringValue : null);
                }
            }
            else
            {
                var child = property.Copy();
                var end = property.GetEndProperty();
                var enter = true;
                while (child.NextVisible(enter) && !SerializedProperty.EqualContents(child, end))
                {
                    enter = false;
                    DrawReadOnly(child, null);
                }
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>Long json: selectable, so it can be copied, and scrolled when it is taller than the limit.</summary>
        private void DrawText(SerializedProperty property)
        {
            var text = property.stringValue;
            var style = EditorStyles.textArea;
            var width = EditorGUIUtility.currentViewWidth - 40f;
            var height = style.CalcHeight(new GUIContent(text), width);

            var path = property.propertyPath;
            _scrolls.TryGetValue(path, out var scroll);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(Mathf.Min(height + 4f, MaxTextHeight)));
            EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(height));
            EditorGUILayout.EndScrollView();
            _scrolls[path] = scroll;
        }

        private void DrawMascot(MascotLook look)
        {
            EditorGUILayout.LabelField("Mascot", EditorStyles.boldLabel);
            if (look == null)
            {
                EditorGUILayout.HelpBox("No mascot in the profile", MessageType.None);
                return;
            }

            UpdatePicture(look);
            var rect = GUILayoutUtility.GetRect(PictureSize, PictureSize, GUILayout.ExpandWidth(false));
            if (_picture != null)
            {
                GUI.DrawTexture(rect, _picture, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.HelpBox(rect, _pictureError, MessageType.Warning);
            }

            var partsExpanded = EditorPrefs.GetBool(PartsExpandedKey, false);
            partsExpanded = EditorGUILayout.Foldout(partsExpanded, "Parts", true);
            EditorPrefs.SetBool(PartsExpandedKey, partsExpanded);
            if (!partsExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var part in MascotLook.Parts)
            {
                var option = look[part];
                var row = EditorGUILayout.GetControlRect();
                var valueRect = EditorGUI.PrefixLabel(row, new GUIContent(MascotCatalog.Label(part)));
                // the swatch place is kept on every row, so the values line up
                var swatch = new Rect(valueRect.x, valueRect.y + 2f, valueRect.height - 4f, valueRect.height - 4f);
                if (MascotCatalog.IsColor(part) && ColorUtility.TryParseHtmlString(MascotCatalog.Color(part, option), out var color))
                {
                    EditorGUI.DrawRect(swatch, color);
                }

                valueRect.xMin += swatch.width + 6f;
                EditorGUI.LabelField(valueRect, $"{option + 1} / {MascotLook.OptionCount} · {MascotCatalog.OptionName(part, option)}");
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>Renders the picture again only when the look changes (the view refreshes after every save).</summary>
        private void UpdatePicture(MascotLook look)
        {
            var key = JsonUtility.ToJson(look);
            if (key == _pictureLook)
            {
                return;
            }

            DestroyPicture();
            _pictureLook = key;
            try
            {
                _picture = MascotRenderer.Render(MascotSvg.Build(look, MascotFraming.Full, 1f), PictureSize * 2, PictureSize * 2);
                _picture.hideFlags = HideFlags.HideAndDontSave;
            }
            catch (System.Exception e)
            {
                _pictureError = $"The mascot can't be drawn: {e.Message}";
                Debug.LogException(e);
            }
        }

        private void DestroyPicture()
        {
            if (_picture != null)
            {
                DestroyImmediate(_picture);
            }

            _picture = null;
            _pictureLook = null;
        }
    }
}

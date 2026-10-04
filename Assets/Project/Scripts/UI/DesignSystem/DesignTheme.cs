using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Single source of design tokens (colors and fonts) for the redesigned UI.
    /// UI elements reference tokens by name through <see cref="ThemeColor"/> and <see cref="ThemeFont"/>,
    /// so changing a token here changes every element that uses it.
    /// Generated and updated by the Penpot importer (Chang/Design System/Import from Penpot).
    /// Editor-only: the importer bakes resolved colors and fonts into the prefabs, so the theme is not
    /// loaded at runtime and is not included in builds.
    /// </summary>
    [CreateAssetMenu(menuName = "Chang/Design System/Theme", fileName = "DesignTheme")]
    public class DesignTheme : ScriptableObject
    {
        public const string AssetPath = "Assets/Project/UI/DesignTheme.asset";

        [Serializable]
        public class ColorToken
        {
            public string Name;
            public Color Color = Color.white;
        }

        [Serializable]
        public class FontToken
        {
            public string Name;
            public TMP_FontAsset Font;
        }

        [SerializeField] private List<ColorToken> _colors = new();
        [SerializeField] private List<FontToken> _fonts = new();

#if UNITY_EDITOR
        private static DesignTheme _current;

        /// <summary>Raised when the theme asset is edited in the inspector, so elements re-apply tokens.</summary>
        public static event Action Changed;

        public static DesignTheme Current
        {
            get
            {
                if (_current == null)
                {
                    _current = UnityEditor.AssetDatabase.LoadAssetAtPath<DesignTheme>(AssetPath);
                }

                return _current;
            }
        }
#endif

        public List<ColorToken> Colors => _colors;
        public List<FontToken> Fonts => _fonts;

        public bool TryGetColor(string token, out Color color)
        {
            foreach (var c in _colors)
            {
                if (c.Name == token)
                {
                    color = c.Color;
                    return true;
                }
            }

            color = Color.magenta;
            return false;
        }

        public bool TryGetFont(string token, out TMP_FontAsset font)
        {
            foreach (var f in _fonts)
            {
                if (f.Name == token)
                {
                    font = f.Font;
                    return font != null;
                }
            }

            font = null;
            return false;
        }

        public void SetColor(string token, Color color)
        {
            var existing = _colors.Find(c => c.Name == token);
            if (existing != null)
            {
                existing.Color = color;
            }
            else
            {
                _colors.Add(new ColorToken { Name = token, Color = color });
            }
        }

        public void SetFont(string token, TMP_FontAsset font)
        {
            var existing = _fonts.Find(f => f.Name == token);
            if (existing != null)
            {
                existing.Font = font;
            }
            else
            {
                _fonts.Add(new FontToken { Name = token, Font = font });
            }
        }

#if UNITY_EDITOR
        public static void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private void OnValidate()
        {
            NotifyChanged();
        }
#endif
    }
}

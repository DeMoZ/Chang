using TMPro;
using UnityEngine;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Binds a TMP text font to a <see cref="DesignTheme"/> font token (family + weight, e.g. "Prompt/SemiBold").
    /// Size stays on the text itself.
    /// Applied in the editor only: the importer bakes the resolved value into the prefab, which is used as is at runtime.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class ThemeFont : MonoBehaviour
    {
        [SerializeField] private string _token;

#if UNITY_EDITOR
        private TMP_Text _text;
#endif

        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                Apply();
            }
        }

        public void Apply()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            var theme = DesignTheme.Current;
            if (theme == null || string.IsNullOrEmpty(_token) || !theme.TryGetFont(_token, out var font))
            {
                return;
            }

            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            if (_text.font != font)
            {
                _text.font = font;
            }
#endif
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            DesignTheme.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            DesignTheme.Changed -= Apply;
        }

        private void OnValidate()
        {
            Apply();
        }
#endif
    }
}

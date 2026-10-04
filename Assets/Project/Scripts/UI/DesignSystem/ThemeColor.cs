using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Binds a Graphic (Image, TMP text, …) color to a <see cref="DesignTheme"/> color token.
    /// The final color is token color with alpha multiplied by <see cref="Alpha"/>.
    /// Applied in the editor only: the importer bakes the resolved value into the prefab, which is used as is at runtime.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public class ThemeColor : MonoBehaviour
    {
        [SerializeField] private string _token;
        [SerializeField, Range(0f, 1f)] private float _alpha = 1f;

#if UNITY_EDITOR
        private Graphic _graphic;
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

        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
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
            if (theme == null || string.IsNullOrEmpty(_token) || !theme.TryGetColor(_token, out var color))
            {
                return;
            }

            if (_graphic == null)
            {
                _graphic = GetComponent<Graphic>();
            }

            color.a *= _alpha;
            if (_graphic.color != color)
            {
                _graphic.color = color;
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

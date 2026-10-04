using UnityEngine;
using UnityEngine.EventSystems;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Keeps the portrait design column centered and no wider than <see cref="_maxWidth"/> when the screen is wide
    /// (landscape phones, tablets, WebGL on desktop). Stretches to the parent height.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class WidthLimiter : UIBehaviour
    {
        [SerializeField] private float _maxWidth = 600f;

        protected override void OnEnable()
        {
            base.OnEnable();
            Apply();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        protected override void OnTransformParentChanged()
        {
            Apply();
        }

        private void Apply()
        {
            var rt = (RectTransform)transform;
            var parent = rt.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            var width = Mathf.Min(parent.rect.width, _maxWidth);
            var anchorMin = new Vector2(0.5f, 0f);
            var anchorMax = new Vector2(0.5f, 1f);
            var size = new Vector2(width, 0f);
            if (rt.anchorMin != anchorMin) rt.anchorMin = anchorMin;
            if (rt.anchorMax != anchorMax) rt.anchorMax = anchorMax;
            if (rt.sizeDelta != size) rt.sizeDelta = size;
            if (rt.anchoredPosition != Vector2.zero) rt.anchoredPosition = Vector2.zero;
        }
    }
}

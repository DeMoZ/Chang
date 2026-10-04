using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Reports the preferred size of the other layout elements on this object as its minimum size,
    /// so a layout group never squeezes it (Penpot "hug content" items don't shrink).
    /// TMP texts report min width 0, which makes overflowing rows collapse without this.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class LayoutNoShrink : UIBehaviour, ILayoutElement
    {
        [SerializeField] private bool _width = true;
        [SerializeField] private bool _height;

        private bool _measuring;

        public float minWidth => _width ? Measure(0) : -1f;
        public float minHeight => _height ? Measure(1) : -1f;
        public float preferredWidth => -1f;
        public float preferredHeight => -1f;
        public float flexibleWidth => -1f;
        public float flexibleHeight => -1f;
        public int layoutPriority => 2;

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }

        private float Measure(int axis)
        {
            if (_measuring)
            {
                return -1f;
            }

            // LayoutUtility asks every ILayoutElement here, this one included: guard against recursion.
            _measuring = true;
            var size = LayoutUtility.GetPreferredSize((RectTransform)transform, axis);
            _measuring = false;
            return size;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
    }
}

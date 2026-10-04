using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Horizontal swipe of a row laid out by a <see cref="HorizontalLayoutGroup"/> that is wider than its frame
    /// (the design draws such rows as one clipped frame, so there is no separate viewport and content for a ScrollRect).
    /// The row is moved by the left padding of the layout group.
    /// </summary>
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class HorizontalDragScroll : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private float revealMargin = 16f;

        private HorizontalLayoutGroup _layout;
        private int _baseLeft;
        private float _offset;

        private RectTransform Rect => (RectTransform)transform;

        private HorizontalLayoutGroup Layout
        {
            get
            {
                if (_layout == null)
                {
                    _layout = GetComponent<HorizontalLayoutGroup>();
                    _baseLeft = _layout.padding.left;
                }

                return _layout;
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, eventData.position, eventData.pressEventCamera, out var current) ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, eventData.position - eventData.delta, eventData.pressEventCamera, out var previous))
            {
                return;
            }

            SetOffset(_offset - (current.x - previous.x));
        }

        /// <summary>Scrolls the row so the item is fully visible.</summary>
        public void Reveal(RectTransform item)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
            var left = item.localPosition.x - item.rect.width * item.pivot.x - Rect.rect.xMin;
            var right = left + item.rect.width;
            if (left < revealMargin)
            {
                SetOffset(_offset - (revealMargin - left));
            }
            else if (right > Rect.rect.width - revealMargin)
            {
                SetOffset(_offset + right - (Rect.rect.width - revealMargin));
            }
        }

        private void SetOffset(float offset)
        {
            var layout = Layout;
            // Width of the row without the scroll offset.
            var rowWidth = LayoutUtility.GetPreferredWidth(Rect) + (layout.padding.left - _baseLeft) * -1f;
            var max = Mathf.Max(0f, rowWidth - Rect.rect.width);
            _offset = Mathf.Clamp(offset, 0f, max);
            layout.padding.left = _baseLeft - Mathf.RoundToInt(_offset);
            LayoutRebuilder.MarkLayoutForRebuild(Rect);
        }
    }
}

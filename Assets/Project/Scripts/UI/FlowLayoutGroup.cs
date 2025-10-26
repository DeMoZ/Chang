using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Chang.UI
{
    [AddComponentMenu("Layout/Flow Layout Group")]
    public class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField] private float _horizontalSpacing;
        [SerializeField] private float _verticalSpacing;

        public float HorizontalSpacing
        {
            get => _horizontalSpacing;
            set => SetProperty(ref _horizontalSpacing, value);
        }

        public float VerticalSpacing
        {
            get => _verticalSpacing;
            set => SetProperty(ref _verticalSpacing, value);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnDisable()
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
        }
        
        public void _CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            float containerWidth = rectTransform.rect.width;
            float currentX = padding.left;
            float currentY = padding.top;
            float rowHeight = 0f;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childWidth = child.rect.width;
                float childHeight = child.rect.height;

                if (currentX + childWidth > containerWidth - padding.right)
                {
                    currentX = padding.left;
                    currentY += rowHeight + VerticalSpacing;
                    rowHeight = 0f;
                }

                rowHeight = Mathf.Max(rowHeight, childHeight);
                currentX += childWidth + HorizontalSpacing;
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            float containerWidth = rectTransform.rect.width;
            float currentX = padding.left;
            float currentY = padding.top;
            float rowHeight = 0f;

            // If the container is too small, we can't calculate anything.
            if (containerWidth <= 0)
            {
                SetLayoutInputForAxis(padding.top + padding.bottom, padding.top + padding.bottom, -1, 1);
                return;
            }

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childWidth = LayoutUtility.GetPreferredWidth(child);
                float childHeight = LayoutUtility.GetPreferredHeight(child);

                // Check if the element needs to wrap to the next line.
                if (currentX + childWidth > containerWidth - padding.right && currentX > padding.left)
                {
                    currentY += rowHeight + VerticalSpacing;
                    currentX = padding.left;
                    rowHeight = 0f;
                }

                rowHeight = Mathf.Max(rowHeight, childHeight);
                currentX += childWidth + HorizontalSpacing;
            }

            float totalHeight = currentY + rowHeight + padding.bottom;
            SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
        }
        
        public void _CalculateLayoutInputVertical()
        {
            float containerWidth = rectTransform.rect.width;
            float currentY = padding.top;
            float rowHeight = 0f;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childWidth = child.rect.width;
                float childHeight = child.rect.height;

                if (padding.left + childWidth > containerWidth - padding.right)
                {
                    currentY += rowHeight + VerticalSpacing;
                    rowHeight = 0f;
                }

                rowHeight = Mathf.Max(rowHeight, childHeight);
            }

            float totalHeight = currentY + rowHeight + padding.bottom;
            SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            float containerWidth = rectTransform.rect.width;
            float currentX = padding.left;
            float currentY = padding.top;
            float rowHeight = 0f;

            List<RectTransform> rowChildren = new List<RectTransform>();

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childWidth = LayoutUtility.GetPreferredWidth(child);
                float childHeight = LayoutUtility.GetPreferredHeight(child);

                if (currentX + childWidth > containerWidth - padding.right && rowChildren.Count > 0)
                {
                    PositionRow(rowChildren, currentY, rowHeight);
                    rowChildren.Clear();

                    currentX = padding.left;
                    currentY += rowHeight + VerticalSpacing;
                    rowHeight = 0f;
                }

                rowHeight = Mathf.Max(rowHeight, childHeight);
                currentX += childWidth + HorizontalSpacing;
                rowChildren.Add(child);
            }

            PositionRow(rowChildren, currentY, rowHeight);
        }

        public override void SetLayoutVertical()
        {
            // The vertical layout is handled by SetLayoutHorizontal
        }

        private void PositionRow(List<RectTransform> children, float y, float rowHeight)
        {
            float currentX = padding.left;
            foreach (var child in children)
            {
                float childWidth = LayoutUtility.GetPreferredWidth(child);
                float childHeight = LayoutUtility.GetPreferredHeight(child);
                float yPos = y + (rowHeight - childHeight) * 0.5f; // Align center vertically in row

                SetChildAlongAxis(child, 0, currentX, childWidth);
                SetChildAlongAxis(child, 1, yPos, childHeight);

                currentX += childWidth + HorizontalSpacing;
            }
        }

        protected override void OnTransformChildrenChanged()
        {
            base.OnTransformChildrenChanged();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}
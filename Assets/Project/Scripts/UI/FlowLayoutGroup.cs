using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Chang.UI
{
    /// <summary>
    /// Places children in rows, a child that does not fit the row width is wrapped to the next row
    /// </summary>
    [AddComponentMenu("Layout/Flow Layout Group")]
    public class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField] private float _horizontalSpacing;
        [SerializeField] private float _verticalSpacing;

        // the row index of each child, rows are calculated by the children preferred widths
        private readonly List<int> _childRows = new();

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

        // the children preferred heights are calculated by the layout system before this call
        public override void CalculateLayoutInputVertical()
        {
            CalculateRows();

            float totalHeight = padding.top + padding.bottom;
            int rowsCount = 0;
            for (int row = 0; GetRowHeight(row, out float rowHeight); row++)
            {
                totalHeight += rowHeight;
                rowsCount++;
            }

            if (rowsCount > 1)
            {
                totalHeight += (rowsCount - 1) * VerticalSpacing;
            }

            SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
        }

        // the vertical positions are set in SetLayoutVertical, the children preferred heights are not calculated yet here
        public override void SetLayoutHorizontal()
        {
            CalculateRows();

            float currentX = padding.left;
            int currentRow = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                if (_childRows[i] != currentRow)
                {
                    currentRow = _childRows[i];
                    currentX = padding.left;
                }

                RectTransform child = rectChildren[i];
                float childWidth = LayoutUtility.GetPreferredWidth(child);
                SetChildAlongAxis(child, 0, currentX, childWidth);
                currentX += childWidth + HorizontalSpacing;
            }
        }

        public override void SetLayoutVertical()
        {
            float currentY = padding.top;
            int currentRow = 0;
            GetRowHeight(currentRow, out float rowHeight);

            for (int i = 0; i < rectChildren.Count; i++)
            {
                if (_childRows[i] != currentRow)
                {
                    currentY += rowHeight + VerticalSpacing;
                    currentRow = _childRows[i];
                    GetRowHeight(currentRow, out rowHeight);
                }

                RectTransform child = rectChildren[i];
                float childHeight = LayoutUtility.GetPreferredHeight(child);
                float yPos = currentY + (rowHeight - childHeight) * 0.5f; // Align center vertically in row
                SetChildAlongAxis(child, 1, yPos, childHeight);
            }
        }

        protected override void OnTransformChildrenChanged()
        {
            base.OnTransformChildrenChanged();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private void CalculateRows()
        {
            _childRows.Clear();

            float maxX = rectTransform.rect.width - padding.right;
            float currentX = padding.left;
            int row = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                float childWidth = LayoutUtility.GetPreferredWidth(rectChildren[i]);

                // a child wider than the container takes the whole row
                if (currentX + childWidth > maxX && currentX > padding.left)
                {
                    row++;
                    currentX = padding.left;
                }

                _childRows.Add(row);
                currentX += childWidth + HorizontalSpacing;
            }
        }

        private bool GetRowHeight(int row, out float rowHeight)
        {
            rowHeight = 0f;
            bool exists = false;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                if (_childRows[i] == row)
                {
                    rowHeight = Mathf.Max(rowHeight, LayoutUtility.GetPreferredHeight(rectChildren[i]));
                    exists = true;
                }
            }

            return exists;
        }
    }
}

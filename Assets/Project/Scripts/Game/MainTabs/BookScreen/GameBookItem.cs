using System;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject doneState;
        [SerializeField] private Image doneStateImage;
        [SerializeField] private GameObject nextState;
        [SerializeField] private GameObject waitState;
        [SerializeField] private Button button;

        [Header("Design (optional)")]
        [Tooltip("LessonNode variants: New, Score 0pct … Score 100pct")]
        [SerializeField] private DesignStates states;
        [SerializeField] private TMP_Text scoreText;
        [Tooltip("Moved sideways to draw the zig-zag lesson path")]
        [SerializeField] private RectTransform shift;

        public void Init(string labelText, int state, Action onItemClick)
        {
            label.text = labelText;

            SetActive(doneState, state == 0);
            SetActive(nextState, state == 1);
            SetActive(waitState, state == 2);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onItemClick.Invoke);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        public void SetColor(Color color)
        {
            if (doneStateImage != null)
            {
                doneStateImage.color = color;
            }
        }

        /// <summary>Mark progress of the lesson, 0…1 (sum of question marks ÷ max). 0 means not played yet.</summary>
        public void SetProgress(float progress)
        {
            if (states == null)
            {
                return;
            }

            var state = progress <= 0f ? "New"
                : progress < 0.125f ? "Score 0pct"
                : progress < 0.375f ? "Score 25pct"
                : progress < 0.625f ? "Score 50pct"
                : progress < 0.875f ? "Score 75pct"
                : "Score 100pct";
            states.Apply(state);

            if (scoreText != null && progress > 0f)
            {
                scoreText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        public void SetHorizontalShift(float offset)
        {
            if (shift != null)
            {
                shift.anchoredPosition = new Vector2(offset, shift.anchoredPosition.y);
            }
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
}

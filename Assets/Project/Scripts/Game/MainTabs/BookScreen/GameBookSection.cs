using System;
using Chang.Services;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookSection : Colorizable
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Toggle sortSectionToggle;
        [SerializeField] private Button repeatSectionButton;

        [Header("Design (optional)")]
        [Tooltip("SectionHeader variants: Expanded, Collapsed, Sorted (expanded with the sort switched on)")]
        [SerializeField] private DesignStates states;
        [Tooltip("Pressing it collapses or expands the section (the whole header)")]
        [SerializeField] private Button collapseButton;
        [Tooltip("Section title in the learn language, hidden when the sheet has no Section.<Key>.Learn")]
        [SerializeField] private TMP_Text learnLabel;
        [Tooltip("\"62% · 6 lessons\"")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private LoadingFillBar progressBar;

        private bool _isCollapsed;
        private bool _isSorted;

        /// <summary>Raised with the new collapsed value when the header is pressed.</summary>
        public event Action<bool> CollapseToggled;

        public bool IsCollapsed => _isCollapsed;

        private void Awake()
        {
            if (collapseButton != null)
            {
                collapseButton.onClick.AddListener(() => SetCollapsed(!_isCollapsed));
            }
        }

        public void SetCollapsed(bool collapsed)
        {
            _isCollapsed = collapsed;
            ShowState();
            CollapseToggled?.Invoke(collapsed);
        }

        /// <summary>The sort button is shown only while the section is expanded; its look tells whether the section is sorted.</summary>
        private void ShowState()
        {
            if (states == null)
            {
                return;
            }

            states.Apply(_isCollapsed ? "Collapsed" : _isSorted ? "Sorted" : "Expanded");
            ReapplyBaseColor();
        }
        
        public void Init(string key, Action onSectionSortClick, Action onSectionRepetitionClick)
        {
            label.text = LocalizationService.Localize($"Section.{key}", key);

            if (learnLabel != null)
            {
                string learnTitle = LocalizationService.Localize($"Section.{key}.Learn", null);
                learnLabel.text = learnTitle;
                learnLabel.gameObject.SetActive(!string.IsNullOrEmpty(learnTitle));
            }
            
            sortSectionToggle.onValueChanged.RemoveAllListeners();
            sortSectionToggle.onValueChanged.AddListener(isOn =>
            {
                _isSorted = isOn;
                ShowState();
                onSectionSortClick.Invoke();
            });
            
            repeatSectionButton.onClick.RemoveAllListeners();
            repeatSectionButton.onClick.AddListener(onSectionRepetitionClick.Invoke);
        }

        private void OnDestroy()
        {
            sortSectionToggle.onValueChanged.RemoveAllListeners();
            repeatSectionButton.onClick.RemoveAllListeners();
            if (collapseButton != null)
            {
                collapseButton.onClick.RemoveAllListeners();
            }
        }
        
        public void SetSortToggle(bool isOn, bool isInteractable)
        {
            sortSectionToggle.SetIsOnWithoutNotify(isOn);
            sortSectionToggle.interactable = isInteractable;
            // the toggle has no transition: an unavailable sort (nothing would move) is shown dimmed
            var sortGroup = sortSectionToggle.GetComponent<CanvasGroup>();
            if (sortGroup == null)
            {
                sortGroup = sortSectionToggle.gameObject.AddComponent<CanvasGroup>();
            }

            sortGroup.alpha = isInteractable ? 1f : 0.4f;
            _isSorted = isOn;
            ShowState();
            
            // Debug.Log($"SetSortToggle, isOn: {isOn}, interactable: {isInteractable}");
        }
        
        /// <param name="progress">mean lesson progress, 0…1</param>
        public void SetProgress(float progress, int lessonsCount)
        {
            if (progressText != null)
            {
                progressText.text = string.Format(LocalizationService.Localize("Lobby.Section.Progress", "{0}% · {1} lessons"),
                    Mathf.RoundToInt(progress * 100f), lessonsCount);
            }

            if (progressBar != null)
            {
                progressBar.SetProgress(progress);
            }
        }

        public void SetInteractableRepeatButton(bool isOn)
        {
            repeatSectionButton.interactable = isOn;
        }
    }
}
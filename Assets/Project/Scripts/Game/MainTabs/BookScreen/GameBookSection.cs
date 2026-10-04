using System;
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
        [Tooltip("SectionHeader variants: Expanded, Collapsed")]
        [SerializeField] private DesignStates states;
        [SerializeField] private Button collapseButton;

        private bool _isCollapsed;

        /// <summary>Raised with the new collapsed value when the header chevron is pressed.</summary>
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
            if (states != null)
            {
                states.Apply(collapsed ? "Collapsed" : "Expanded");
                ReapplyBaseColor();
            }

            CollapseToggled?.Invoke(collapsed);
        }
        
        public void Init(string key, Action onSectionSortClick, Action onSectionRepetitionClick)
        {
            label.text = key;
            
            sortSectionToggle.onValueChanged.RemoveAllListeners();
            sortSectionToggle.onValueChanged.AddListener(_ => onSectionSortClick.Invoke());
            
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
            
            // Debug.Log($"SetSortToggle, isOn: {isOn}, interactable: {isInteractable}");
        }
        
        public void SetInteractableRepeatButton(bool isOn)
        {
            repeatSectionButton.interactable = isOn;
        }
    }
}
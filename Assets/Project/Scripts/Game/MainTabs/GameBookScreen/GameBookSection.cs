using System;
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
        }
        
        public void SetSortToggle(bool isOn, bool isInteractable)
        {
            sortSectionToggle.SetIsOnWithoutNotify(isOn);
            sortSectionToggle.interactable = isInteractable;
            
            Debug.Log($"SetSortToggle, isOn: {isOn}, interactable: {isInteractable}");
        }
        
        public void SetInteractableRepeatButton(bool isOn)
        {
            repeatSectionButton.interactable = isOn;
        }
    }
}
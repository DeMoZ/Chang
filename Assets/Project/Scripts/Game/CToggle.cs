using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class CToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _inactive;
        [SerializeField] private TMP_Text _word;
        [SerializeField] private TMP_Text _phonetic;

        private Action<bool> _onValueChanged;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            _inactive.gameObject.SetActive(false);
        }

        public void Set(string word, string phonetic, ToggleGroup toggleGroup, Action<bool> onValueChanged)
        {
            _word.text = word;
            _phonetic.text = phonetic;
            _onValueChanged = onValueChanged;
            _toggle.group = toggleGroup;
        }
        
        public void Set(string word, string phonetic, Action<bool> onValueChanged)
        {
            _word.text = word;
            _phonetic.text = phonetic;
            _onValueChanged = onValueChanged;
        }

        public void SetGroup(ToggleGroup toggleGroup)
        {
            _toggle.group = toggleGroup;
        }
        
        public void EnablePhonetic(bool enable)
        {
            _phonetic.gameObject.SetActive(enable);
        }

        public void SetInteractable(bool interactable)
        {
            _toggle.interactable = interactable;
        }
        
        public void SetActive(bool active)
        {
            _inactive.gameObject.SetActive(!active);
        }
        
        private void OnToggleValueChanged(bool isOn)
        {
            _onValueChanged?.Invoke(isOn);
        }
        
        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
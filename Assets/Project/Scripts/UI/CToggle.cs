using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class CToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _incorrect;
        [SerializeField] private Image _correct;
        [SerializeField] private Image _inactive;
        [SerializeField] private TMP_Text _word;
        [SerializeField] private TMP_Text _phonetics;

        private Action<bool> _onValueChanged;
        
        public bool IsOn
        {
            get { return _toggle.isOn; }
            set { _toggle.isOn = value; }
        }

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            _inactive.gameObject.SetActive(false);
            _correct.gameObject.SetActive(false);
            _incorrect.gameObject.SetActive(false);
        }

        public void Set(string word, string phonetic, ToggleGroup toggleGroup, Action<bool> onValueChanged)
        {
            _word.text = word;
            _phonetics.text = phonetic;
            _onValueChanged = onValueChanged;
            _toggle.group = toggleGroup;
        }
        
        public void Set(string word, string phonetic, Action<bool> onValueChanged)
        {
            _word.text = word;
            _phonetics.text = phonetic;
            _onValueChanged = onValueChanged;
        }

        public void SetGroup(ToggleGroup toggleGroup)
        {
            _toggle.group = toggleGroup;
        }
        
        public void EnablePhonetics(bool enable)
        {
            _phonetics.gameObject.SetActive(enable);
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

        public void SetCorrect(bool isCorrect)
        {
            _correct.gameObject.SetActive(isCorrect);
            _incorrect.gameObject.SetActive(!isCorrect);
        }
        
        public void SetNormal()
        {
            _correct.gameObject.SetActive(false);
            _incorrect.gameObject.SetActive(false);
        }
    }
}
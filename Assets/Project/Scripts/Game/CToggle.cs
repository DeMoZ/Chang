using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class CToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _word;
        [SerializeField] private TMP_Text _phonetic;

        private Action<bool> _onValueChanged;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void Set(string word, string phonetic, Action<bool> onValueChanged)
        {
            _word.text = word;
            _phonetic.text = phonetic;
            _onValueChanged = onValueChanged;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            _onValueChanged?.Invoke(isOn);
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }

        public void EnablePhonetic(bool enable)
        {
            _phonetic.gameObject.SetActive(enable);
        }
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class TabToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _word;

        public void Set(ToggleGroup toggleGroup, bool isOn)
        {
            _toggle.group = toggleGroup;
            _toggle.isOn = isOn;
        }

        public void AddListener(Action<bool> onValueChanged)
        {
            _toggle.onValueChanged.AddListener(isOn => onValueChanged?.Invoke(isOn));
        }

        public void RemoveAllListeners()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
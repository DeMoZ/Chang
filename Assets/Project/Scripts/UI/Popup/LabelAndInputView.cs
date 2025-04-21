using System;
using TMPro;
using UnityEngine;

namespace Popup
{
    public class LabelAndInputView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_InputField inputField;

        public Action<string> OnInputTextChanged { get; set; }
        public Action<Color> OnInputTextColor { get; set; }

        public string LabelText
        {
            set => labelText.text = value;
        }

        public string InputText
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(InputTextChanged);
            OnInputTextColor += SetInputTextColor;
        }

        private void SetInputTextColor(Color color)
        {
            inputField.textComponent.color = color;
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(InputTextChanged);
            OnInputTextColor -= SetInputTextColor;
        }

        private void InputTextChanged(string text)
        {
            OnInputTextChanged?.Invoke(text);
        }
    }
}
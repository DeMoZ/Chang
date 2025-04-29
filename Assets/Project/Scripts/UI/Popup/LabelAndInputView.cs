using System;
using DMZ.Events;
using TMPro;
using UnityEngine;

namespace Popup
{
    public class LabelAndInputView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_InputField inputField;

        public Action<string> OnInputTextChanged { get; set; }
        public DMZState<string> LabelText { get; set; }

        public string InputText
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        public void Init()
        {
            inputField.onValueChanged.AddListener(InputTextChanged);
            LabelText.Subscribe(SetLabelText);
        }

        private void OnDestroy()
        {
            inputField.onValueChanged.RemoveListener(InputTextChanged);
            LabelText.Unsubscribe(SetLabelText);
        }

        private void SetLabelText(string text)
        {
            Debug.Log($"Set label text: {text}");
            labelText.text = text;
        }

        private void InputTextChanged(string text)
        {
            OnInputTextChanged?.Invoke(text);
        }
    }
}
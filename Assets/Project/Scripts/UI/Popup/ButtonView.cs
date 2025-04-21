using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popup
{
    public class ButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;

        public string Text
        {
            set => text.text = value;
        }

        public Action OnClick { get; set; }
        public Action<bool> OnSetInteractable { get; set; }

        private void OnEnable()
        {
            button.onClick.AddListener(OnClicked);
            OnSetInteractable += SetInteractable;
        }
        
        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClicked);
            OnSetInteractable -= SetInteractable;
        }
        
        private void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }
        
        private void OnClicked()
        {
            OnClick?.Invoke();
        }
    }
}
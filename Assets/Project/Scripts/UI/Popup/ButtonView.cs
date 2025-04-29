using System;
using DMZ.Events;
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
        public DMZState<bool> OnSetInteractable { get; set; }

        public void Init()
        {
            button.onClick.AddListener(OnClicked);
            OnSetInteractable.Subscribe(SetInteractable);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClicked);
            OnSetInteractable.Unsubscribe(SetInteractable);
        }

        private void SetInteractable(bool interactable)
        {
            Debug.LogWarning("SetInteractable: " + interactable);
            button.interactable = interactable;
        }

        private void OnClicked()
        {
            OnClick?.Invoke();
        }
    }
}
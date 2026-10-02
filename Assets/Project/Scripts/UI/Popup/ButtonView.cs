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
        [SerializeField] private Color selectedColor = new(0.93f, 0.78f, 0.45f, 1f);

        private Color _normalColor = Color.white;
        private bool _normalColorCached;

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

        public void SetSelected(bool selected)
        {
            Graphic graphic = button.targetGraphic;
            if (graphic == null)
            {
                return;
            }

            if (!_normalColorCached)
            {
                _normalColor = graphic.color;
                _normalColorCached = true;
            }

            graphic.color = selected ? selectedColor : _normalColor;
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
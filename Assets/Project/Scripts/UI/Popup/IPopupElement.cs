using System;
using DMZ.Events;
using UnityEngine;

namespace Popup
{
    public interface IPopupElement
    {
    }

    public class PopupHeader : IPopupElement
    {
        public string Text { get; set; }

        public PopupHeader(string text)
        {
            Text = text;
        }
    }

    public class PopupLabel : IPopupElement
    {
        public DMZState<string> Text { get; set; }

        public PopupLabel(DMZState<string> text)
        {
            Text = text;
        }
    }

    public class PopupButton : IPopupElement
    {
        public string Text { get; set; }
        public Action OnClick { get; set; }
        public DMZState<bool> OnSetInteractable { get; set; }

        public PopupButton(string text, Action onClick, DMZState<bool> onSetInteractable)
        {
            Text = text;
            OnClick = onClick;
            OnSetInteractable = onSetInteractable;
        }
    }

    public class PopupLabelAndInput : IPopupElement
    {
        public DMZState<string> LabelText { get; set; }
        public string InputText { get; set; }
        public Action<string> OnInputTextChanged { get; set; }
        public Action<Color> OnSetInputColor { get; set; }

        public PopupLabelAndInput(DMZState<string> labelText,
            string inputText,
            Action<string> onInputTextChanged,
            Action<Color> onSetInputColor)
        {
            LabelText = labelText;
            InputText = inputText;
            OnInputTextChanged = onInputTextChanged;
            OnSetInputColor = onSetInputColor;
        }
    }
}
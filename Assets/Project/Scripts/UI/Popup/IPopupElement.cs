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

    public class PopupSelector : IPopupElement
    {
        public string[] Options { get; set; }
        public int SelectedIndex { get; set; }
        public Action<int> OnSelect { get; set; }

        /// <summary>
        /// Options per row, all options are in one row if 0
        /// </summary>
        public int Columns { get; set; }

        public PopupSelector(string[] options, int selectedIndex, Action<int> onSelect, int columns = 0)
        {
            Options = options;
            SelectedIndex = selectedIndex;
            OnSelect = onSelect;
            Columns = columns;
        }

        /// <summary>
        /// One option per enum value, the selection is written to the state
        /// </summary>
        public static PopupSelector ForEnum<TEnum>(DMZState<TEnum> state) where TEnum : struct, Enum
        {
            TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));

            return new PopupSelector(
                Array.ConvertAll(values, value => value.ToString()),
                Array.IndexOf(values, state.Value),
                index => state.Value = values[index]);
        }
    }
}

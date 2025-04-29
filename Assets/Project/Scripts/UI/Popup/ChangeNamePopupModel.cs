using System;
using System.Text;
using DMZ.Events;
using DMZ.Legacy.LoginScreen;
using UnityEngine;

namespace Popup
{
    public class ChangeNamePopupModel : IDisposable
    {
        private static Color _validColor = Color.gray;
        private static Color _invalidColor = Color.white;

        public DMZState<string> LabelText = new();
        public DMZState<bool> OnSetSubmitInteractable = new DMZState<bool>(true);
        public Action<string> OnNameInput;
        public Action OnChangeNameCancel;
        public Action OnChangeNameSubmit;
        public Action<NameValidationType> OnNameValidation;

        private StringBuilder _hintBuilder;
        private Color _color;

        public ChangeNamePopupModel()
        {
            _hintBuilder = new StringBuilder();

            OnNameValidation += validationType =>
            {
                _hintBuilder.Clear();

                _color = validationType.HasFlag(NameValidationType.MinLength) ? _invalidColor : _validColor;
                _hintBuilder.Append(AddColor("Min", _color));

                _color = validationType.HasFlag(NameValidationType.InvalidCharacters) ? _invalidColor : _validColor;
                _hintBuilder.Append(AddColor("3@#$", _color));

                _color = validationType.HasFlag(NameValidationType.MaxLength) ? _invalidColor : _validColor;
                _hintBuilder.Append(AddColor("Max", _color));

                LabelText.Value = _hintBuilder.ToString();
                OnSetSubmitInteractable.Value = validationType == 0;
            };
        }

        public void Dispose()
        {
            LabelText.Dispose();
            OnSetSubmitInteractable.Dispose();
            
            OnNameInput = null;
            OnChangeNameCancel = null;
            OnChangeNameSubmit = null;
            OnNameValidation = null;

            _hintBuilder.Clear();
            _hintBuilder = null;
        }

        private string AddColor(string value, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{value}, </color>";
        }
    }
}
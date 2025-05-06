using System;
using System.Text;
using System.Text.RegularExpressions;
using DMZ.Events;
using DMZ.Legacy.LoginScreen;
using UnityEngine;

namespace Popup
{
    public class ChangeNamePopupModel : IDisposable
    {
        private static readonly Color ValidColor = Color.gray;
        private static readonly Color InvalidColor = Color.white;

        private const int MinName = 1;
        private const int MaxName = 20;

        private readonly Regex _nameRegex = new(@"^(?!.*[-\s]{2,})[\p{L}]+(?:[-\s][\p{L}]+)*$", RegexOptions.Compiled);

        private Action<NameValidationType> _onNameValidation;
        private StringBuilder _hintBuilder;

        public readonly DMZState<string> NameInput = new();
        public readonly DMZState<string> LabelText = new();
        public readonly DMZState<bool> OnSetSubmitInteractable = new(true);

        public Action OnChangeNameCancel;
        public Action OnChangeNameSubmit;

        public ChangeNamePopupModel()
        {
            _hintBuilder = new StringBuilder();

            _onNameValidation += validationType =>
            {
                _hintBuilder.Clear();

                var color = validationType.HasFlag(NameValidationType.MinLength) ? InvalidColor : ValidColor;
                _hintBuilder.Append(AddColor("Min", color));

                color = validationType.HasFlag(NameValidationType.InvalidCharacters) ? InvalidColor : ValidColor;
                _hintBuilder.Append(AddColor("3@#$", color));

                color = validationType.HasFlag(NameValidationType.MaxLength) ? InvalidColor : ValidColor;
                _hintBuilder.Append(AddColor("Max", color));

                LabelText.Value = _hintBuilder.ToString();
                OnSetSubmitInteractable.Value = validationType == 0;
            };

            NameInput.Subscribe(text =>
            {
                NameValidationType validation = ValidateInputName(text);
                _onNameValidation?.Invoke(validation);
            });
        }

        public void Dispose()
        {
            LabelText.Dispose();
            OnSetSubmitInteractable.Dispose();
            NameInput.Dispose();

            OnChangeNameCancel = null;
            OnChangeNameSubmit = null;
            _onNameValidation = null;

            _hintBuilder.Clear();
            _hintBuilder = null;
        }

        private string AddColor(string value, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{value}, </color>";
        }

        private NameValidationType ValidateInputName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return NameValidationType.MinLength;
            }

            NameValidationType validationType = 0;

            if (value.Length < MinName)
                validationType |= NameValidationType.MinLength;
            if (value.Length > MaxName)
                validationType |= NameValidationType.MaxLength;
            if (!_nameRegex.IsMatch(value))
                validationType |= NameValidationType.InvalidCharacters;

            return validationType;
        }
    }
}
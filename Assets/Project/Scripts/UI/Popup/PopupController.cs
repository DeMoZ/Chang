using System;
using Chang;

namespace Popup
{
    public class PopupController : IViewController
    {
        private PopupView _view;

        public void Init(PopupView view)
        {
            _view = view;
        }

        public void CreatePopup(params IPopupElement[] elements)
        {
            _view.Clear();

            foreach (IPopupElement element in elements)
            {
                if (element is PopupHeader header)
                {
                    _view.CreateHeader(header.Text);
                }
                else if (element is PopupLabel label)
                {
                    _view.CreateLabel(label.Text);
                }
                else if (element is PopupLabelAndInput labelAndInput)
                {
                    _view.CreateLabelAndInput(labelAndInput.LabelText,
                        labelAndInput.InputText,
                        labelAndInput.OnInputTextChanged, 
                        labelAndInput.OnSetInputColor);
                }
                else if (element is PopupButton button)
                {
                    _view.CreateButton(button.Text,
                        button.OnClick,
                        button.OnSetInteractable);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(element), element, null);
                }
            }
        }

        public void SetViewActive(bool active)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
using System;
using Chang;

namespace Popup
{
    public class PopupController<TModel> : IViewController where TModel : IDisposable
    {
        private PopupView _view;
        private TModel _model;

        public void Init(PopupView view, TModel model)
        {
            _view = view;
            _model = model;
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
                    _view.CreateLabelAndInput(
                        labelAndInput.LabelText,
                        labelAndInput.InputText,
                        labelAndInput.OnInputTextChanged);
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
            _view.gameObject.SetActive(active);
        }

        public void Dispose()
        {
            _model.Dispose();
            UnityEngine.Object.Destroy(_view.gameObject);
        }
    }
}
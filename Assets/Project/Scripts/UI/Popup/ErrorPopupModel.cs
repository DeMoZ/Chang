using System;
using DMZ.Events;

namespace Popup
{
    public class ErrorPopupModel : IDisposable
    {
        public DMZState<string> LabelText = new();
        public Action OnOkClicked;

        public void Dispose()
        {
            LabelText?.Dispose();

            LabelText = null;
            OnOkClicked = null;
        }
    }
}
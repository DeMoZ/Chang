using System;
using DMZ.Events;

namespace Popup
{
    public class RetryPopupModel : IDisposable
    {
        public DMZState<string> LabelText = new();
        public Action OnRetryClicked;

        public void Dispose()
        {
            LabelText.Dispose();

            LabelText = null;
            OnRetryClicked = null;
        }
    }
}

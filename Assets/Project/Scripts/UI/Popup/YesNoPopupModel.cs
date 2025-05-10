using System;
using DMZ.Events;

namespace Popup
{
    public class YesNoPopupModel : IDisposable
    {
        public DMZState<string> HeaderText = new();
        public DMZState<string> LabelText = new();
        public Action OnOkClicked;
        public Action OnCancelClicked;
        
        public void Dispose()
        {
            HeaderText.Dispose();
            LabelText.Dispose();
            
            HeaderText = null;
            LabelText = null;
            OnOkClicked = null;
            OnCancelClicked = null;
        }
    }
}
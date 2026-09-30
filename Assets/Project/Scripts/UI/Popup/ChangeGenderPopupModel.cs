using System;
using Chang;
using DMZ.Events;

namespace Popup
{
    public class ChangeGenderPopupModel : IDisposable
    {
        public readonly DMZState<GenderType> Gender = new(GenderType.No);

        public Action OnChangeGenderCancel;
        public Action OnChangeGenderSubmit;

        public void Dispose()
        {
            Gender.Dispose();

            OnChangeGenderCancel = null;
            OnChangeGenderSubmit = null;
        }
    }
}

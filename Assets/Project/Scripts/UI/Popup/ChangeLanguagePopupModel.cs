using System;
using Chang;
using DMZ.Events;

namespace Popup
{
    public class ChangeLanguagePopupModel : IDisposable
    {
        public readonly DMZState<Languages> Language = new(Languages.English);

        /// <summary>
        /// Selectable languages and their names shown in the selector
        /// </summary>
        public Languages[] Options = Array.Empty<Languages>();
        public string[] OptionNames = Array.Empty<string>();

        public Action OnChangeLanguageCancel;
        public Action OnChangeLanguageSubmit;

        public void Dispose()
        {
            Language.Dispose();

            OnChangeLanguageCancel = null;
            OnChangeLanguageSubmit = null;
        }
    }
}

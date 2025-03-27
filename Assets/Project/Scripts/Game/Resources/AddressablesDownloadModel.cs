using System;
using DMZ.Events;

namespace Chang.Resources
{
    public class AddressablesDownloadModel : IDisposable
    {
        public DMZState<bool> ShowUi = new(false);
        public DMZState<float> Progress = new(0);
        
        public void Dispose()
        {
            ShowUi.Dispose();
            Progress.Dispose();
        }
    }
}
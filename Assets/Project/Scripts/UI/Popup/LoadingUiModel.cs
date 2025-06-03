using System;

namespace Popup
{
    [Flags]
    public enum LoadingElements
    {
        None = 0,
        Background = 1 << 0,
        Bar = 1 << 1,
        Percent = 1 << 2,
        Animation = 1 << 3,
        Bytes = 1 << 4,
    }
    
    public class LoadingUiModel : IDisposable
    {
        public LoadingElements Elements { get; }

        public LoadingUiModel(LoadingElements elements)
        {
            Elements = elements;
        }
        
        public void Dispose()
        {
            
        }
    }
}
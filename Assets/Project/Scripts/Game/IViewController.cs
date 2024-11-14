using System;

namespace Chang
{
    public interface IViewController : IDisposable
    {
        void SetViewActive(bool active);
    }
}
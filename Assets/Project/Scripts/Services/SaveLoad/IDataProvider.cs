using System;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.DataProvider
{
    public interface IDataProvider : IDisposable
    {
        UniTask<ProfileData> LoadProfileDataAsync();
        UniTask SaveProfileDataAsync(ProfileData data);

        UniTask<ProgressData> LoadProgressDataAsync();
        UniTask SaveProgressDataAsync(ProgressData data);
    }
}
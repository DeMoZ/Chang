using System;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.DataProvider
{
    public interface IDataProvider : IDisposable
    {
        UniTask InitAsync(CancellationToken ct);
        
        UniTask<ProfileData> LoadProfileDataAsync();
        UniTask SaveProfileDataAsync(ProfileData data);

        UniTask<ProgressData> LoadProgressDataAsync();
        UniTask SaveProgressDataAsync(ProgressData data);
    }
}
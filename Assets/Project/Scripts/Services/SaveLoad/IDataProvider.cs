using System;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.DataProvider
{
    public interface IDataProvider : IDisposable
    {
        UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct);
        UniTask SaveProfileDataAsync(ProfileData data);
        UniTask<ProgressData> LoadProgressDataAsync(CancellationToken ct);
        UniTask SaveProgressDataAsync(ProgressData data);
        string PlayerId { get; }
    }
}
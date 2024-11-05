using System;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.SaveLoad
{
    public interface ISaveLoad : IDisposable
    {
        UniTask<ProfileData> LoadProfileDataAsync();
        UniTask SaveProfileDataAsync(ProfileData data);

        UniTask<ProgressData> LoadProgressDataAsync();
        UniTask SaveProgressDataAsync(ProgressData data);
    }

    public interface ISaveLoadWithInit : ISaveLoad
    {
        UniTask InitAsync(CancellationToken token);
    }
}
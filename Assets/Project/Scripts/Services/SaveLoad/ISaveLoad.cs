using System;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.SaveLoad
{
    public interface ISaveLoad : IDisposable
    {
        UniTask<PlayerData> LoadData();
        UniTask SaveData(PlayerData data);
    }

    // todo roman implement unity cloud save load
}
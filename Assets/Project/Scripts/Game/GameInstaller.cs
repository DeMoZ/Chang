using Chang.Resources;
using Chang.Profile;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SimpleResourceManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameBus>().AsSingle();
            Container.BindInterfacesAndSelfTo<Game>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerProfile>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileService>().AsSingle();
            
        }
    }

    public class UpdateLoopObject : MonoBehaviour
    {

    }
}
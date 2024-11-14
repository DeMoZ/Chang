using Chang.Resources;
using Chang.Profile;
using Chang.Services;
using UnityEngine;
using Zenject;
using Chang.FSM;

namespace Chang
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SimpleResourceManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameBus>().AsSingle();
            Container.BindInterfacesAndSelfTo<Game>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameFSM>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerProfile>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileService>().AsSingle();
        }
    }

    public class UpdateLoopObject : MonoBehaviour
    {

    }
}
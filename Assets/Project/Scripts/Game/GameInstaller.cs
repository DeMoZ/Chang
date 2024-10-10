using Chang.Resources;
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
        }
    }

    public class UpdateLoopObject : MonoBehaviour
    {

    }
}
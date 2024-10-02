using Chang.Resources;
using Zenject;

namespace Chang
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SimpleResourceManager>()
               .AsSingle();
               
            Container.BindInterfacesAndSelfTo<Game>()
               .AsSingle();
        }
    }
}
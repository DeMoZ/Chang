using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class BootstrapInstaller : MonoInstaller<BootstrapInstaller>
    {
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");
            
            Container.BindInterfacesAndSelfTo<Bootstrap>().AsSingle();
        }

        // public override void Start()
        // {
        //     Debug.Log($"{nameof(Start)}");
        // }
    }
}
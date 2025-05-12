using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class RebootInstaller : MonoInstaller<RebootInstaller>
    {
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");
            
            Container.BindInterfacesAndSelfTo<Reboot>().AsSingle();
        }

        public override void Start()
        {
            Debug.Log($"{nameof(Start)}");
        }
    }
}
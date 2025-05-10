using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class RebootInstaller : MonoInstaller<RebootInstaller>
    {
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");
        }

        public override void Start()
        {
            Debug.Log($"{nameof(Start)}");
            Bootstrap bootstrap = Container.Resolve<Bootstrap>();
            bootstrap.LoadingSequenceAsync().Forget();
        }
    }
}
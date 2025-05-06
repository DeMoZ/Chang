using DMZ.Legacy.LoginScreen;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class BootstrapInstaller : MonoInstaller<BootstrapInstaller>
    {
        [SerializeField] private LoadingUiView loadingScreen;
        [SerializeField] private LogInView logInScreen;
        
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");

            #region Views
            
            Container.BindInstance(logInScreen).AsSingle();
            Container.BindInstance(loadingScreen).AsSingle();
            
            #endregion
            
            #region Controllers

            Container.BindInterfacesAndSelfTo<Bootstrap>().AsSingle();

            #endregion
        }

        public override void Start()
        {
            Debug.Log($"{nameof(Start)}");
            var loginModel = Container.Resolve<LogInModel>();
            logInScreen.Init(loginModel);
        }
    }
}
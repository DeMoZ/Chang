using DMZ.Legacy.LoginScreen;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class BootstrapSceneInstaller : MonoInstaller<BootstrapSceneInstaller>
    {
        [SerializeField] private LoadingView loadingScreen;
        [SerializeField] private LogInView logInScreen;

        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");

            #region Views
            
            Container.BindInstance(logInScreen).AsSingle();
            Container.BindInstance(loadingScreen).AsSingle();
            //Container.Inject(loadingScreen);

            #endregion
            
            #region Controllers

            Container.BindInterfacesTo<LoadingController>().AsSingle();

            #endregion
        }

        public void Start()
        {
            Debug.Log($"{nameof(Start)}");
            var loginModel = Container.Resolve<LogInModel>();
            logInScreen.Init(loginModel);
        }
    }
}
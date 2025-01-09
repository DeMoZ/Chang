using UnityEngine;
using Zenject;
using Chang.UI;
using DMZ.Legacy.LoginScreen;

namespace Chang
{
    public class ScreenInstaller : MonoInstaller
    {
        [SerializeField] private MainUiView mainUiScreen;
        [SerializeField] private RepetitionView repetitionScreen;
        [SerializeField] private ProfileView profileScreen;
        [SerializeField] private GameBookView gameBookScreen;
        [SerializeField] private GameOverlayView gameOverlayScreen;
        [SerializeField] private SystemUiScreen systemUiScreen;

        [Space] 
        [SerializeField] private GameObject pagesContainer;

        [Space] 
        [SerializeField] private DemonstrationWordView demonstrationScreen;
        [SerializeField] private MatchWordsView matchWordScreen;
        [SerializeField] private SelectWordView selectWordScreen;
        [SerializeField] private PreloaderView preloaderScreen;

        [Space] 
        [SerializeField] private LogInView logInScreen;

        private LogInController _loginController;


        public override void InstallBindings()
        {
            Debug.Log($"{nameof(ScreenInstaller)} InstallBindings");

            Container.BindInterfacesAndSelfTo<MainScreenBus>().AsSingle();

            #region Views

            Container.BindInstance(mainUiScreen).AsSingle();
            Container.BindInstance(repetitionScreen).AsSingle();
            Container.BindInstance(gameBookScreen).AsSingle();
            Container.BindInstance(gameOverlayScreen).AsSingle();
            Container.BindInstance(pagesContainer).WithId("PagesContainer").AsSingle();
            Container.BindInstance(demonstrationScreen).AsSingle();
            Container.BindInstance(matchWordScreen).AsSingle();
            Container.BindInstance(selectWordScreen).AsSingle();
            Container.BindInstance(preloaderScreen).AsSingle();
            Container.BindInstance(logInScreen).AsSingle();
            Container.BindInstance(profileScreen).AsSingle();
            Container.BindInstance(systemUiScreen).AsSingle();

            #endregion

            #region Controllers

            Container.BindInterfacesAndSelfTo<LobbyController>().AsSingle();
            Container.BindInterfacesAndSelfTo<RepetitionController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameBookController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameOverlayController>().AsSingle();
            Container.BindInterfacesAndSelfTo<DemonstrationWordController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MatchWordsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SelectWordController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PreloaderController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SystemUiController>().AsSingle();
            #endregion

            Container.BindInterfacesAndSelfTo<ScreenManager>().AsSingle();

            BindLogin();
        }

        private void BindLogin()
        {
            var loginModel = new LogInModel();
            _loginController = new LogInController(loginModel);
            Container.BindInstance(loginModel).AsSingle();
            logInScreen.Init(loginModel);
            Container.BindInstance(_loginController).AsSingle();
        }

        public void OnDestroy()
        {
            _loginController?.Dispose();
        }
    }
}
using Chang.FSM;
using UnityEngine;
using Zenject;
using Chang.UI;
using Chang.GameBook;
using Chang.Resources;
using Chang.Services;

namespace Chang
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private MainUiView mainUiScreen;
        [SerializeField] private RepetitionView repetitionScreen;
        [SerializeField] private ProfileView profileScreen;
        [SerializeField] private GameBookView gameBookScreen;
        [SerializeField] private GameOverlayView gameOverlayScreen;

        [Space, SerializeField] private PagesContainer pagesContainer;

        [Space, SerializeField] private PlayResultView playResultScreen;

        [SerializeField] private DemonstrationWordView demonstrationScreen;
        [SerializeField] private MatchWordsView matchWordScreen;
        [SerializeField] private SelectWordView selectWordScreen;

        [Space, SerializeField] private AudioSource pagesAudioSource;

        public override void InstallBindings()
        {
            Debug.Log($"{nameof(GameInstaller)} InstallBindings");

            Container.BindInterfacesAndSelfTo<Game>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameFSM>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameBus>().AsSingle();
            Container.BindInterfacesAndSelfTo<RepetitionService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WordPathHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScreenManager>().AsSingle();

            Container.BindInstance(pagesAudioSource).AsSingle();

            #region Views

            Container.BindInstance(mainUiScreen).AsSingle();
            Container.BindInstance(repetitionScreen).AsSingle();
            Container.BindInstance(gameBookScreen).AsSingle();
            Container.BindInstance(gameOverlayScreen).AsSingle();
            Container.BindInstance(pagesContainer).AsSingle();
            Container.BindInstance(playResultScreen).AsSingle();
            Container.BindInstance(demonstrationScreen).AsSingle();
            Container.BindInstance(matchWordScreen).AsSingle();
            Container.BindInstance(selectWordScreen).AsSingle();
            Container.BindInstance(profileScreen).AsSingle();

            #endregion

            #region Controllers

            Container.BindInterfacesAndSelfTo<LobbyController>().AsSingle();
            Container.BindInterfacesAndSelfTo<RepetitionController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameBookController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameOverlayController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayResultController>().AsSingle();
            Container.BindInterfacesAndSelfTo<DemonstrationWordController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MatchWordsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SelectWordController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PagesSoundController>().AsSingle();

            #endregion
        }
    }
}
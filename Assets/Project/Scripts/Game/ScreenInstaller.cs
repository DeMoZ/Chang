using UnityEngine;
using Zenject;

namespace Chang
{
    public class ScreenInstaller : MonoInstaller
    {
        [SerializeField] private GameBookView gameBookScreen;

        [Space]
        [SerializeField] private GameObject pagesContainer;

        [Space]
        [SerializeField] private DemonstrationWordView demonstationScreen;
        [SerializeField] private MatchWordsView matchWordScreen;
        [SerializeField] private SelectWordView selectWordScreen;
        [SerializeField] private PreloaderView preloaderScreen;

        public override void InstallBindings()
        {
            Debug.Log($"{nameof(ScreenInstaller)} InstallBindings");
            #region Views
            Container.BindInstance(gameBookScreen).AsSingle();

            Container.BindInstance(pagesContainer).WithId("PagesContainer").AsSingle();

            Container.BindInstance(demonstationScreen).AsSingle();
            Container.BindInstance(matchWordScreen).AsSingle();
            Container.BindInstance(selectWordScreen).AsSingle();

            Container.BindInstance(preloaderScreen).AsSingle();

            #endregion

            #region Controllers
            Container.BindInterfacesAndSelfTo<GameBookController>().AsSingle();

            Container.BindInterfacesAndSelfTo<DemonstrationWordController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MatchWordsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SelectWordController>().AsSingle();

            Container.BindInterfacesAndSelfTo<PreloaderController>().AsSingle();
            #endregion

            Container.BindInterfacesAndSelfTo<ScreenManager>().AsSingle();
        }
    }
}
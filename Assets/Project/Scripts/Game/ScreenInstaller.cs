using UnityEngine;
using Zenject;

namespace Chang
{
    public class ScreenInstaller : MonoInstaller
    {
        [SerializeField] private GameBookView gameBookScreen;

        public override void InstallBindings()
        {
            Debug.Log($"{nameof(ScreenInstaller)} InstallBindings");

            Container.BindInstance(gameBookScreen)
                .AsSingle();



            Container.BindInterfacesAndSelfTo<GameBookController>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ScreenManager>()
                .AsSingle();
        }
    }
}
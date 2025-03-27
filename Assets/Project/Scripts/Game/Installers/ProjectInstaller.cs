using Chang.FSM;
using Chang.Profile;
using Chang.Resources;
using Chang.Services;
using DMZ.Legacy.LoginScreen;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");
            Container.BindInterfacesAndSelfTo<AddressablesDownloadModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressablesAssetManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressablesDownloader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerProfile>().AsSingle();
            Container.BindInterfacesAndSelfTo<AuthorizationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameFSM>().AsSingle();
            Container.BindInterfacesAndSelfTo<Game>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameBus>().AsSingle();


            // todo Chang add required bindings
            // Container.BindInterfacesAndSelfTo<RepetitionService>().AsSingle(); probably move to SceneInstaller which is better to name after scene name - GameInstaller
            
            Container.BindInterfacesAndSelfTo<LogInModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<LogInController>().AsSingle();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"{nameof(OnApplicationPause)} = {pauseStatus}");
            // AudioListener.pause = pauseStatus;
            // UIBus.OnApplicationPause += pauseStatus;
        }

        //Unity bug that caused crash on ios: https://forum.unity.com/threads/crash-debugstringtofilepostprocessedstacktrace.1523512/#post-9513493
        private void OnApplicationFocus(bool hasFocus)
        {
            // UIBus.OnApplicationFocus += hasFocus;
        }

        private void OnApplicationQuit()
        {
            // CrashReportHandler.enableCaptureExceptions = false;
        }
    }
}
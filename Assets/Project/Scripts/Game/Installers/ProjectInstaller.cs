using Chang.Profile;
using Chang.Resources;
using Chang.Services;
using DMZ.Legacy.LoginScreen;
using Popup;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");
            Container.BindInterfacesAndSelfTo<AddressablesAssetManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressablesDownloader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerProfile>().AsSingle();
            Container.BindInterfacesAndSelfTo<AuthorizationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MainScreenBus>().AsSingle();
            Container.BindInterfacesAndSelfTo<ErrorHandler>().AsSingle();

            var loginModel = new LogInModel();
            var loginController = new LogInController(loginModel);
            Container.BindInstances(loginModel);
            Container.BindInstance(loginController);
            
            var popupManager = FindFirstObjectByType<PopupManager>();
            if (popupManager != null)
            {
                Container.BindInstance(popupManager).AsSingle();
            }
            else
            {
                Debug.LogError("PopupManager not found in the scene.");
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"{nameof(OnApplicationPause)} = {pauseStatus}");
            // todo chang when pause implemented (backlog)
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
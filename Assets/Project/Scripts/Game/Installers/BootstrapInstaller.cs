using UnityEngine.SceneManagement;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class BootstrapInstaller : MonoInstaller<BootstrapInstaller>
    {
        public override void InstallBindings()
        {
            Debug.Log($"{nameof(InstallBindings)}");
        }

        public override void Start()
        {
            Debug.Log($"{nameof(Start)}");
            LoadRebootScene();
        }
        
        private void LoadRebootScene()
        {
            Debug.Log("LoadRebootScene");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
        }
    }
}
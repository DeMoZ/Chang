using System;
using UnityEngine.SceneManagement;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class Bootstrap : IInitializable, IDisposable
    {
        [Inject]
        public Bootstrap()
        {
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
            Debug.Log("Initialize");
            LoadRebootScene();
        }
        
        private void LoadRebootScene()
        {
            Debug.Log("LoadRebootScene");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
        }
    }
}
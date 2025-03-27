using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Chang
{
    public class LoadBootstrapScene : MonoBehaviour
    {
        async void Start()
        {
            await BootstrapProcesses();

            SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
        }

        private async UniTask BootstrapProcesses()
        {
            await UniTask.Delay(1);
        }
    }
}
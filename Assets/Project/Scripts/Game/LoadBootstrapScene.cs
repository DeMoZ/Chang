using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Chang
{
    public class LoadBootstrapScene : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                await BootstrapProcesses();

                SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                throw new NotImplementedException(e.Message); // todo cnang handle exception
            }
        }

        private async UniTask BootstrapProcesses()
        {
            await UniTask.Delay(1);
        }
    }
}
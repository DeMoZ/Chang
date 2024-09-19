
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class Bootstrap : MonoBehaviour
{
    async void Start()
    {
        await BootstrapProcesses();

        var handle = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        await UniTask.WaitUntil(() => handle.isDone, PlayerLoopTiming.Update);
        
    }

    private async UniTask BootstrapProcesses()
    {
        await UniTask.Delay(1);
    }
}
#if UNITY_EDITOR
using DMZ.DebugSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Zenject;

namespace Chang
{
    /// <summary>
    /// Editor only: restarts the application by loading the reboot scene on the hotkey press.
    /// </summary>
    public class EditorRestartTrigger : ITickable
    {
        private const KeyCode RestartKey = KeyCode.Space;

        public void Tick()
        {
            if (!Input.GetKeyDown(RestartKey) || IsTyping())
            {
                return;
            }

            DMZLogger.Log($"{RestartKey} pressed, load reboot scene");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
        }

        private static bool IsTyping()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
            {
                return false;
            }

            return eventSystem.currentSelectedGameObject.TryGetComponent(out TMP_InputField inputField) && inputField.isFocused;
        }
    }
}
#endif

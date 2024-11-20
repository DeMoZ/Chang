using System;
using Chang.UI;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class MainUiView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private ToggleGroup toggleGroup;

        [Space]
        [SerializeField] private TabToggle lessonsToggle;
        [SerializeField] private TabToggle repetitionToggle;

        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private Action<bool, MainTabType> _onTabChanged;

        public void Init(Action<bool, MainTabType> onTabChanged)
        {
            _onTabChanged = onTabChanged;
        }

        public void Enter()
        {
            content.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            lessonsToggle.AddListener(isOn => OnTabChanged(isOn, MainTabType.Lessons));
            repetitionToggle.AddListener(isOn => OnTabChanged(isOn, MainTabType.Repetition));

            // settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            // exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnDisable()
        {

            lessonsToggle.RemoveAllListeners();
            repetitionToggle.RemoveAllListeners();

            // settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            // exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        public void EnableToggleType(MainTabType currentTabType)
        {
            if (currentTabType == MainTabType.Lessons)
                lessonsToggle.Activate();

            if (currentTabType == MainTabType.Repetition)
                repetitionToggle.Activate();
        }

        private void OnTabChanged(bool isOn, MainTabType tabType)
        {
            _onTabChanged?.Invoke(isOn, tabType);
        }

        private void OnStartButtonClicked()
        {
            // Handle start button click
            Debug.Log("Start button clicked");
        }

        private void OnSettingsButtonClicked()
        {
            // Handle settings button click
            Debug.Log("Settings button clicked");
        }

        private void OnExitButtonClicked()
        {
            // Handle exit button click
            Debug.Log("Exit button clicked");
            Application.Quit();
        }
    }
}
using System;
using System.Diagnostics;
using Chang.UI;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class MainUiView : MonoBehaviour
    {
        [SerializeField] private ToggleGroup toggleGroup;

        [Space]
        [SerializeField] private TabToggle lessonsToggle;
        [SerializeField] private TabToggle repetitionToggle;

        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        public Action<bool, MainTabType> OnTabChanged { get; private set; }

        private void Awake()
        {
            lessonsToggle.Set(toggleGroup, isOn: true);
            lessonsToggle.AddListener(isOn => OnTabChanged?.Invoke(isOn, MainTabType.Lessons));

            repetitionToggle.Set(toggleGroup, isOn: false);
            repetitionToggle.AddListener(isOn => OnTabChanged?.Invoke(isOn, MainTabType.Repetition));

            // settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            // exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        public void Init(Action<bool, MainTabType> onTabChanged)
        {
            OnTabChanged = onTabChanged;
        }

        private void OnDestroy()
        {
            lessonsToggle.RemoveAllListeners();
            repetitionToggle.RemoveAllListeners();

            // settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            // exitButton.onClick.RemoveListener(OnExitButtonClicked);
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
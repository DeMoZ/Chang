using System;
using Chang.UI;
using Chang.UI.DesignSystem;
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
        [SerializeField] private TabToggle vocabularyToggle;
        [SerializeField] private TabToggle sentencesToggle;
        [SerializeField] private TabToggle repetitionToggle;
        [SerializeField] private TabToggle profileToggle;

        [Tooltip("Tab bar look per tab (TabBar variants Words, Sentences, Repeat, Profile). Optional.")]
        [SerializeField] private DesignStates tabBarStates;

        // [SerializeField] private Button settingsButton;
        // [SerializeField] private Button exitButton;

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
            vocabularyToggle.AddListener(isOn => OnTabChanged(isOn, MainTabType.Vocabulary));
            sentencesToggle.AddListener(isOn => OnTabChanged(isOn, MainTabType.Sentences));
            repetitionToggle.AddListener(isOn => OnTabChanged(isOn, MainTabType.Repetition));
            profileToggle.AddListener(isOn => OnTabChanged(isOn, MainTabType.Profile));

            // settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            // exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnDisable()
        {

            vocabularyToggle.RemoveAllListeners();
            sentencesToggle.RemoveAllListeners();
            repetitionToggle.RemoveAllListeners();
            profileToggle.RemoveAllListeners();

            // settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            // exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        public void EnableToggleType(MainTabType currentTabType)
        {
            if (currentTabType == MainTabType.Vocabulary)
                vocabularyToggle.Activate();
            
            if (currentTabType == MainTabType.Sentences)
                sentencesToggle.Activate();

            if (currentTabType == MainTabType.Repetition)
                repetitionToggle.Activate();
            
            if (currentTabType == MainTabType.Profile)
                profileToggle.Activate();
        }

        /// <summary>Marks the tab as selected (toggle and tab bar look) without raising the tab changed callback.</summary>
        public void ShowTab(MainTabType tabType)
        {
            var toggle = tabType switch
            {
                MainTabType.Sentences => sentencesToggle,
                MainTabType.Repetition => repetitionToggle,
                MainTabType.Profile => profileToggle,
                _ => vocabularyToggle
            };
            toggle.SetIsOnWithoutNotify(true);
            ShowSelectedTab(tabType);
        }

        private void OnTabChanged(bool isOn, MainTabType tabType)
        {
            if (isOn)
            {
                ShowSelectedTab(tabType);
            }

            _onTabChanged?.Invoke(isOn, tabType);
        }

        private void ShowSelectedTab(MainTabType tabType)
        {
            if (tabBarStates == null)
            {
                return;
            }

            tabBarStates.Apply(tabType switch
            {
                MainTabType.Sentences => "Sentences",
                MainTabType.Repetition => "Repeat",
                MainTabType.Profile => "Profile",
                _ => "Words"
            });
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
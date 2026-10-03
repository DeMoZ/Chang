using System;
using Chang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public class ProfileView : MonoBehaviour
    {
        [SerializeField] private Button logoutBtn;
        [SerializeField] private TMP_Text userNameText;
        [SerializeField] private TMP_Text userIdText;
        [SerializeField] private Button changeNameBtn;
        [SerializeField] private TMP_Text genderText;
        [SerializeField] private Button changeGenderBtn;
        [SerializeField] private TMP_Text languageText;
        [SerializeField] private Button changeLanguageBtn;
        
        private Action _onLogOutClick;
        private Action _onChangeNameClick;
        private Action _onChangeGenderClick;
        private Action _onChangeLanguageClick;

        public void Init(Action onLogOutClick, Action onChangeNameClick, Action onChangeGenderClick, Action onChangeLanguageClick)
        {
            _onLogOutClick = onLogOutClick;
            _onChangeNameClick = onChangeNameClick;
            _onChangeGenderClick = onChangeGenderClick;
            _onChangeLanguageClick = onChangeLanguageClick;
        }
        
        public void SetUserName(string userName)
        {
            userNameText.text = userName;
        }
        
        public void SetGender(GenderType gender)
        {
            genderText.GetComponent<LocalizedTMPText>().LocalizationKey = $"Lobby.Profile.Gender.{gender}";
        }
        
        public void SetLanguage(string languageName)
        {
            languageText.text = languageName;
        }

        public void SetUserId(string userId)
        {
            userIdText.text = userId;
        }

        private void OnEnable()
        {
            logoutBtn.onClick.AddListener(OnLogOutClick);
            changeNameBtn.onClick.AddListener(OnChangeNameClick);
            changeGenderBtn.onClick.AddListener(OnChangeGenderClick);
            changeLanguageBtn.onClick.AddListener(OnChangeLanguageClick);
        }

        private void OnDisable()
        {
            logoutBtn.onClick.RemoveListener(OnLogOutClick);
            changeNameBtn.onClick.RemoveListener(OnChangeNameClick);
            changeGenderBtn.onClick.RemoveListener(OnChangeGenderClick);
            changeLanguageBtn.onClick.RemoveListener(OnChangeLanguageClick);
        }

        private void OnLogOutClick()
        {
            _onLogOutClick?.Invoke();
        }
        
        private void OnChangeNameClick()
        {
            _onChangeNameClick?.Invoke();
        }
        
        private void OnChangeGenderClick()
        {
            _onChangeGenderClick?.Invoke();
        }

        private void OnChangeLanguageClick()
        {
            _onChangeLanguageClick?.Invoke();
        }
    }
}
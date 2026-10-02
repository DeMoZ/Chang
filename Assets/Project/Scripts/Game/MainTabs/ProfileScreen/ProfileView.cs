using System;
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
        
        private Action _onLogOutClick;
        private Action _onChangeNameClick;
        private Action _onChangeGenderClick;

        public void Init(Action onLogOutClick, Action onChangeNameClick, Action onChangeGenderClick)
        {
            _onLogOutClick = onLogOutClick;
            _onChangeNameClick = onChangeNameClick;
            _onChangeGenderClick = onChangeGenderClick;
        }
        
        public void SetUserName(string userName)
        {
            userNameText.text = userName;
        }
        
        public void SetGender(GenderType gender)
        {
            genderText.text = gender.ToString();
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
        }

        private void OnDisable()
        {
            logoutBtn.onClick.RemoveListener(OnLogOutClick);
            changeNameBtn.onClick.RemoveListener(OnChangeNameClick);
            changeGenderBtn.onClick.RemoveListener(OnChangeGenderClick);
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
    }
}
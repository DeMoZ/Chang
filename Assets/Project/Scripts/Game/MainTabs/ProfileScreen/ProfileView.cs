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
        
        private Action _onLogOutClick;
        private Action _onChangeNameClick;

        public void Init(Action onLogOutClick, Action onChangeNameClick)
        {
            _onLogOutClick = onLogOutClick;
            _onChangeNameClick = onChangeNameClick;
        }
        
        public void SetUserName(string userName)
        {
            userNameText.text = userName;
        }
        
        public void SetUserId(string userId)
        {
            userIdText.text = userId;
        }

        private void OnEnable()
        {
            logoutBtn.onClick.AddListener(OnLogOutClick);
            changeNameBtn.onClick.AddListener(OnChangeNameClick);
        }

        private void OnDisable()
        {
            logoutBtn.onClick.RemoveListener(OnLogOutClick);
            changeNameBtn.onClick.RemoveListener(OnChangeNameClick);
        }

        private void OnLogOutClick()
        {
            _onLogOutClick?.Invoke();
        }
        
        private void OnChangeNameClick()
        {
            _onChangeNameClick?.Invoke();
        }
    }
}
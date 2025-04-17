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
        
        private Action _onLogOutClick;

        public void Init(Action onLogOutClick)
        {
            _onLogOutClick = onLogOutClick;
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
        }

        private void OnDisable()
        {
            logoutBtn.onClick.RemoveListener(OnLogOutClick);
        }

        private void OnLogOutClick()
        {
            _onLogOutClick?.Invoke();
        }
    }
}
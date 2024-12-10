using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public class ProfileView : MonoBehaviour
    {
        [SerializeField] private Button logoutBtn;
        private Action _onLogOutClick;

        public void Init(Action onLogOutClick)
        {
            _onLogOutClick = onLogOutClick;
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
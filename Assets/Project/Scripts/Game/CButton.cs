using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class CButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        public event Action OnClick;
        
        private void OnEnable()
        {
            button.onClick.AddListener(OnClickHandler);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClickHandler);
        }
        
        private void OnClickHandler()
        {
            OnClick?.Invoke();
        }
    }
}
using System;
using Chang.FSM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.UI
{
    public class GameOverlayView : MonoBehaviour
    {
        [SerializeField] private GameObject _blocker;
        [SerializeField] private Button _returnBtn;
        [SerializeField] private Toggle _phoneticTgl;

        [Space] [SerializeField] private Button _checkBtn;

        [SerializeField] private VocabularyContinueView _continue;

        private UnityAction _checkBtnListener;
        private UnityAction _continueBtnListener;
        private UnityAction _returnBtnListener;
        private UnityAction<bool> _phoneticTglListener;

        public void Init(Action onCheck,
            Action onContinue,
            Action onReturn,
            Action<bool> onPhonetic)
        {
            _checkBtnListener = () => onCheck?.Invoke();
            _continueBtnListener = () => onContinue?.Invoke();
            _returnBtnListener = () => onReturn?.Invoke();
            _phoneticTglListener = isOn => onPhonetic?.Invoke(isOn);

            _returnBtn.onClick.AddListener(_returnBtnListener);
            _checkBtn.onClick.AddListener(_checkBtnListener);
            _continue.ContinueBtn.onClick.AddListener(_continueBtnListener);
            _phoneticTgl.onValueChanged.AddListener(_phoneticTglListener);
        }

        public void Clean()
        {
            _returnBtn.onClick.RemoveListener(_returnBtnListener);
            _checkBtn.onClick.RemoveListener(_checkBtnListener);
            _continue.ContinueBtn.onClick.RemoveListener(_continueBtnListener);
            _phoneticTgl.onValueChanged.RemoveListener(_phoneticTglListener);
        }

        public void EnableBlocker(bool enable)
        {
            _blocker.SetActive(enable);
        }

        public void EnableReturnButton(bool enable)
        {
            _returnBtn.gameObject.SetActive(enable);
        }
        
        public void EnableCheckButton(bool enable)
        {
            _checkBtn.gameObject.SetActive(enable);
        }

        public void EnableContinueButton(bool enable)
        {
            _continue.gameObject.SetActive(enable);
        }

        public void SetContinueButtonInfo(ContinueButtonInfo info)
        {
            _continue.Set(info);
        }
    }
}
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
        [SerializeField] private Button _hintBtn;

        [Space] 
        [SerializeField] private Button _checkBtn;
        [SerializeField] private PagesContinueView _continue;

        [Tooltip("Panel holding the return and hint buttons; shown while one of them is. Optional.")]
        [SerializeField] private GameObject _topBar;

        [Tooltip("Lesson progress in the top bar. Optional.")]
        [SerializeField] private LoadingFillBar _progress;

        private UnityAction _checkBtnListener;
        private UnityAction _continueBtnListener;
        private UnityAction _returnBtnListener;
        private UnityAction _hintBtnListener;

        public void Init(Action onCheck,
            Action onContinue,
            Action onReturn,
            Action onHint)
        {
            _checkBtnListener = () => onCheck?.Invoke();
            _continueBtnListener = () => onContinue?.Invoke();
            _returnBtnListener = () => onReturn?.Invoke();
            _hintBtnListener = () => onHint?.Invoke();

            _returnBtn.onClick.AddListener(_returnBtnListener);
            _checkBtn.onClick.AddListener(_checkBtnListener);
            _hintBtn.onClick.AddListener(_hintBtnListener);
            _continue.AddContinueListener(_continueBtnListener);
        }

        public void Clean()
        {
            _returnBtn.onClick.RemoveListener(_returnBtnListener);
            _checkBtn.onClick.RemoveListener(_checkBtnListener);
            _hintBtn.onClick.RemoveListener(_hintBtnListener);
            _continue.RemoveContinueListener(_continueBtnListener);
        }

        public void EnableBlocker(bool enable)
        {
            _blocker.SetActive(enable);
        }

        public void EnableReturnButton(bool enable)
        {
            _returnBtn.gameObject.SetActive(enable);
            UpdateTopBar();
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

        /// <param name="value">0…1</param>
        public void SetProgress(float value)
        {
            if (_progress != null)
            {
                _progress.SetProgress(value);
            }
        }

        public void EnableHintButton(bool enable)
        {
            _hintBtn.gameObject.SetActive(enable);
            UpdateTopBar();
        }

        private void UpdateTopBar()
        {
            if (_topBar != null)
            {
                _topBar.SetActive(_returnBtn.gameObject.activeSelf || _hintBtn.gameObject.activeSelf);
            }
        }
    }
}
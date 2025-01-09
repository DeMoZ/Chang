using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmPopupView : MonoBehaviour
{
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private Transform _messageParent;
    [SerializeField] private TMP_Text _messagePrefab;
    [SerializeField] private Button _sureBtn;
    [SerializeField] private Button _notSureBtn;

    private UnityAction _sureBtnListener;
    private UnityAction _notSureBtnListener;

    public void Set(Action<bool> onConfirm, string headerText = null, List<string> message = null, string sureBtnText = null, string notSureBtnText = null)
    {
        _sureBtnListener = () => onConfirm?.Invoke(true);
        _notSureBtnListener = () => onConfirm?.Invoke(false);

        _sureBtn.onClick.AddListener(_sureBtnListener);
        _notSureBtn.onClick.AddListener(_notSureBtnListener);

        if (!string.IsNullOrEmpty(headerText))
        {
            _headerText.text = headerText;
        }
        
        if (message != null)
        {
            foreach (var item in message)
            {
                var messageText = Instantiate(_messagePrefab, _messageParent);
                messageText.text = item;
                messageText.gameObject.SetActive(true);
            }    
        }
        
        if (!string.IsNullOrEmpty(sureBtnText))
        {
            _sureBtn.GetComponentInChildren<TMP_Text>().text = sureBtnText;
        }
        
        if (!string.IsNullOrEmpty(notSureBtnText))
        {
            _notSureBtn.GetComponentInChildren<TMP_Text>().text = notSureBtnText;
        }
        
        _sureBtn.gameObject.SetActive(!string.IsNullOrEmpty(sureBtnText));
        _notSureBtn.gameObject.SetActive(!string.IsNullOrEmpty(notSureBtnText));
    }

    private void Clear()
    {
        _sureBtn.onClick.RemoveListener(_sureBtnListener);
        _notSureBtn.onClick.RemoveListener(_notSureBtnListener);
    }
}
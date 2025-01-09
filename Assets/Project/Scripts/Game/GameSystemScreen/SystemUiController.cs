using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class SystemUiController : IDisposable
{
    private SystemUiScreen _view;
    private Stack<MonoBehaviour> _popupStack = new Stack<MonoBehaviour>();

    [Inject]
    public SystemUiController(SystemUiScreen view)
    {
        _view = view;
    }

    public void ShowConfirmPopup(System.Action<bool> onConfirm,
        string headerText = null,
        List<string> message = default,
        string sureBtnText = null,
        string notSureBtnText = null)
    {
        var confirmPopup = _view.InstantiateConfirmPopup();
        confirmPopup.Set(onConfirm, headerText, message, sureBtnText, notSureBtnText);
        confirmPopup.gameObject.SetActive(true);
        _popupStack.Push(confirmPopup);

        if (_popupStack.Count == 1)
        {
            _view.EnableBlocker(true);
        }
    }

    public void ClosePopup()
    {
        if (_popupStack.Count > 0)
        {
            var popup = _popupStack.Pop();
            Object.Destroy(popup.gameObject);
        }

        if (_popupStack.Count == 0)
        {
            _view.EnableBlocker(false);
        }
    }

    public void Dispose()
    {
        _popupStack.Clear();
        _popupStack = null;
    }
}
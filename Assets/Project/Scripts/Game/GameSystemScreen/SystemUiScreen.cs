using UnityEngine;

public class SystemUiScreen : MonoBehaviour
{
    [SerializeField] private GameObject _blocker;
    [SerializeField] private ConfirmPopupView _confirmPopupPrefab;

    public void EnableBlocker(bool enable)
    {
        _blocker.SetActive(enable);
    }
    
    public ConfirmPopupView InstantiateConfirmPopup()
    {
        return Instantiate(_confirmPopupPrefab, transform);
    }
}

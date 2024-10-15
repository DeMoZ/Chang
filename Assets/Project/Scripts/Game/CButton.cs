using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class CButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        public event Action OnClick;
    }
}
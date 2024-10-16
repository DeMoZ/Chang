using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.UI
{
    public class GameOverlayView : MonoBehaviour
    {
        [SerializeField] private Button _checkBtn;

        public void EnableCheckButton(bool isOn)
        {
            Debug.LogError("implement enable / disable check button");
            _checkBtn.gameObject.SetActive(isOn);
        }
    }
}
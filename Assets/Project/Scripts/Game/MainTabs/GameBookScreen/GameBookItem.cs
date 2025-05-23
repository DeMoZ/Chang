using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject doneState;
        [SerializeField] private Image doneStateImage;
        [SerializeField] private GameObject nextState;
        [SerializeField] private GameObject waitState;
        [SerializeField] private Button button;

        public void Init(string labelText, int state, Action onItemClick)
        {
            label.text = labelText;

            doneState.SetActive(state == 0);
            nextState.SetActive(state == 1);
            waitState.SetActive(state == 2);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onItemClick.Invoke);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        public void SetColor(Color color)
        {
            doneStateImage.color = color;
        }
    }
}
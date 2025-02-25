using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text lable;
        [SerializeField] private GameObject doneState;
        [SerializeField] private GameObject nextState;
        [SerializeField] private GameObject wiatState;
        [SerializeField] private Button button;

        public void Init(string key, string lableText, int state, Action<string> onItemClick)
        {
            lable.text = lableText;

            doneState.SetActive(state == 0);
            nextState.SetActive(state == 1);
            wiatState.SetActive(state == 2);

            button.onClick.AddListener(() => onItemClick.Invoke(key));
        }
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookSection : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button repeatSectionButton;
        
        public string LabelText => label.text;
        
        public void Init(string key, Action<string> onSectionRepetitionClick)
        {
            label.text = key;
            
            repeatSectionButton.onClick.RemoveAllListeners();
            repeatSectionButton.onClick.AddListener(() => onSectionRepetitionClick.Invoke(key));
        }

        private void OnDestroy()
        {
            repeatSectionButton.onClick.RemoveAllListeners();
        }
    }
}
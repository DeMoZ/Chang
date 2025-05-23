using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookSection : Colorizable
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button sortSectionButton;
        [SerializeField] private Button repeatSectionButton;
        
        public void Init(string key, Action<string> onSectionSortClick, Action<string> onSectionRepetitionClick)
        {
            label.text = key;
            
            sortSectionButton.onClick.RemoveAllListeners();
            sortSectionButton.onClick.AddListener(() => onSectionSortClick.Invoke(key));
            
            repeatSectionButton.onClick.RemoveAllListeners();
            repeatSectionButton.onClick.AddListener(() => onSectionRepetitionClick.Invoke(key));
        }

        private void OnDestroy()
        {
            sortSectionButton.onClick.RemoveAllListeners();
            repeatSectionButton.onClick.RemoveAllListeners();
        }
    }
}
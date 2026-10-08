using System;
using TriInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class PlayResultView : CScreen
    {
        [SerializeField] private Transform _contentParent;
        [SerializeField] private ResultItem _itemPrefab;
        [SerializeField] private Button _continuteBtn;

        [Header("Totals (optional)")]
        [SerializeField] private TMP_Text _wordsValue;
        [SerializeField] private TMP_Text _masteryValue;
        [SerializeField] private TMP_Text _accuracyValue;

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.Result;

        public void AddItem(string word, string mark = null, bool? isUp = null)
        {
            var item = Instantiate(_itemPrefab, _contentParent);
            item.Set(word, mark, isUp);
        }

        /// <param name="masteryPercent">0…100</param>
        /// <param name="accuracyPercent">0…100</param>
        public void SetTotals(int words, int masteryPercent, int accuracyPercent)
        {
            SetValue(_wordsValue, words.ToString());
            SetValue(_masteryValue, $"{masteryPercent}%");
            SetValue(_accuracyValue, $"{accuracyPercent}%");
        }

        private static void SetValue(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        public void Init(Action onContinueClick)
        {
            Clear();
            _continuteBtn.onClick.AddListener(() => onContinueClick());
        }
        
        public void OnDisable()
        {
            Clear();
            _continuteBtn.onClick.RemoveAllListeners();
        }

        private void Clear()
        {
            foreach (Transform child in _contentParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
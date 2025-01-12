using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class PlayResultView : CScreen
    {
        [SerializeField] private Transform _contentParent;
        [SerializeField] private ResultItem _itemPrefab;
        [SerializeField] private Button _continuteBtn;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.Result;

        public void AddItem(string word, string mark = null, bool? isUp = null)
        {
            var item = Instantiate(_itemPrefab, _contentParent);
            item.Set(word, mark, isUp);
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
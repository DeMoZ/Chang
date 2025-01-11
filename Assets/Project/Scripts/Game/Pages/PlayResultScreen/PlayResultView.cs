using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang.UI
{
    public class PlayResultView : CScreen
    {
        [SerializeField] private Transform _contentParent;
        [SerializeField] private ResultItem _itemPrefab;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.Result;

        public void AddItem(string word, string mark = null, bool? isUp = null)
        {
            var item = Instantiate(_itemPrefab, _contentParent);
            item.Set(word, mark, isUp);
        }

        public void Init()
        {
        }

        public void OnDisable()
        {
            Clear();
        }

        public void Clear()
        {
            foreach (Transform child in _contentParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
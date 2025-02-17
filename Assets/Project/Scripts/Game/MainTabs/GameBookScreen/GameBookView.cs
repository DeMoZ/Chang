using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chang
{
    public class GameBookView : MonoBehaviour
    {
        [SerializeField] private GameBookItem itemPrefab;
        [SerializeField] private Transform content;

        private List<GameBookItem> _items = new();
        private Action<int> _onItemClick;

        public void Init(Action<int> onItemClick)
        {
            _onItemClick = onItemClick;
        }

        public void Set(List<string> fileNames)
        {
            foreach (Transform item in content)
            {
                Destroy(item.gameObject);
            }

            _items.Clear();

            for (var i = 0; i < fileNames.Count; i++)
            {
                var item = Instantiate(itemPrefab, content);
                var state = 0; // todo chang calculate item state from 0 to 2
                item.Init(i, fileNames[i], state, _onItemClick);
                item.gameObject.SetActive(true);
                _items.Add(item);
            }
        }
    }
}
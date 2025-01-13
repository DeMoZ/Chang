using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class MatchWordsView : CScreen
    {
        [SerializeField] private CToggle _matchWordPrefab;
        [SerializeField] private Transform _leftWordsContent;
        [SerializeField] private Transform _rightWordsContent;
        [SerializeField] private ToggleGroup _leftTogglesGroup;
        [SerializeField] private ToggleGroup _rightTogglesGroup;
        [SerializeField] private Button _continuteBtn;

        public override QuestionType ScreenType { get; } = QuestionType.MatchWords;

        private Action<bool, int, bool> _onToggleValueChanged;

        public void Init(Action<bool, int, bool> onToggleValueChanged, Action onContinueClick)
        {
            Clear();
            _onToggleValueChanged = onToggleValueChanged;
            _continuteBtn.onClick.AddListener(() => onContinueClick());
        }

        public void OnDisable()
        {
            Clear();
            _continuteBtn.onClick.RemoveAllListeners();
        }

        private void Clear()
        {
            foreach (Transform child in _leftWordsContent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in _rightWordsContent)
            {
                Destroy(child.gameObject);
            }
        }

        public CToggle AddItem(bool isLeft)
        {
            var toggle = Instantiate(_matchWordPrefab, isLeft ? _leftWordsContent : _rightWordsContent);
            toggle.SetGroup(isLeft ? _leftTogglesGroup : _rightTogglesGroup);
            return toggle;
        }
    }
}
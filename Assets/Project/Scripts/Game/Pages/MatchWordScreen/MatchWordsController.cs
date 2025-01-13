using System;
using System.Collections.Generic;
using Zenject;
using Chang.UI;

namespace Chang
{
    public class MatchWordsController : IViewController
    {
        private MatchWordsView _view;

        [Inject]
        public MatchWordsController(MatchWordsView view)
        {
            _view = view;
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(bool langColumn, 
            List<WordData> left, List<WordData> right,
            Action<bool, int, bool> onToggleValueChanged, Action onContinueClicked)
        {
            _view.Init(onToggleValueChanged, onContinueClicked);

            for (var i = 0; i < left.Count; i++)
            {
                var index = i;
                var toggle = _view.AddItem(langColumn);
                
                if (langColumn)
                    toggle.Set( left[i].Word, left[i].Phonetic, isOn => onToggleValueChanged(true, index, isOn));
                else
                    toggle.Set(left[i].GetTranslation(), string.Empty, isOn => onToggleValueChanged(true, index, isOn));
            }

            for (var i = 0; i < right.Count; i++)
            {
                var index = i;
                var toggle = _view.AddItem(!langColumn);
                
                if (!langColumn)
                    toggle.Set( right[i].Word, right[i].Phonetic, isOn => onToggleValueChanged(true, index, isOn));
                else
                    toggle.Set(right[i].GetTranslation(), string.Empty, isOn => onToggleValueChanged(true, index, isOn));
            }
        }
    }
}
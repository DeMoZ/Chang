using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zenject;
using Chang.UI;
using Cysharp.Threading.Tasks;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class MatchWordsController : IViewController
    {
        private const int TimeToSetNormal = 500; 
        
        private readonly MatchWordsView _view;

        private Action<int, int> _onToggleValueChanged;
        private List<CToggle> _leftToggles;
        private List<CToggle> _rightToggles;

        private CancellationTokenSource _cts;

        [Inject]
        public MatchWordsController(MatchWordsView view)
        {
            _view = view;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            _leftToggles?.Clear();
            _rightToggles?.Clear();
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(bool isLeftLanguage, List<WordData> left, List<WordData> right,
            Action<int, int> onToggleValueChanged, Action onContinueClicked)
        {
            _leftToggles = new List<CToggle>();
            _rightToggles = new List<CToggle>();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            
            _onToggleValueChanged = onToggleValueChanged;
            _view.Init(onContinueClicked);

            for (var i = 0; i < left.Count; i++)
            {
                var index = i;
                var toggle = _view.AddItem(true);
                _leftToggles.Add(toggle);

                if (isLeftLanguage)
                    toggle.Set(left[i].LearnWord, left[i].Phonetic, isOn => OnToggleValueChanged(true, index, isOn));
                else
                    toggle.Set(left[i].GetTranslation(), string.Empty, isOn => OnToggleValueChanged(true, index, isOn));
            }

            for (var i = 0; i < right.Count; i++)
            {
                var index = i;
                var toggle = _view.AddItem(false);
                _rightToggles.Add(toggle);

                if (isLeftLanguage)
                    toggle.Set(right[i].GetTranslation(), string.Empty, isOn => OnToggleValueChanged(false, index, isOn));
                else
                    toggle.Set(right[i].LearnWord, right[i].Phonetic, isOn => OnToggleValueChanged(false, index, isOn));
            }
        }

        public async void ShowCorrect(int leftIndex, int rightIndex, bool isCorrect)
        {
            _leftToggles[leftIndex].IsOn = false;
            _rightToggles[rightIndex].IsOn = false;
            
            _leftToggles[leftIndex].SetInteractable(false);
            _rightToggles[rightIndex].SetInteractable(false);
            
            _leftToggles[leftIndex].SetCorrect(isCorrect);
            _rightToggles[rightIndex].SetCorrect(isCorrect);

            await UniTask.Delay(TimeToSetNormal, cancellationToken: _cts.Token);
            
            _leftToggles[leftIndex].SetNormal();
            _rightToggles[rightIndex].SetNormal();

            if (!isCorrect)
            {
                _leftToggles[leftIndex].SetInteractable(true);
                _rightToggles[rightIndex].SetInteractable(true);
            }
            else
            {
                _leftToggles[leftIndex].SetActive(false);
                _rightToggles[rightIndex].SetActive(false);
            }
        }

        public void EnableContinueButton(bool active)
        {
            _view.EnableContinueButton(active);
        }
        
        private void OnToggleValueChanged(bool isLeft, int index, bool isOn)
        {
            if (!isOn)
                return;

            var column = isLeft ? "left" : "right";
            Debug.Log($"Clicked column: {column}; toggle: {index}; isOn: {isOn}");

            var left = _leftToggles.FirstOrDefault(t => t.IsOn);
            var right = _rightToggles.FirstOrDefault(t => t.IsOn);

            if (left == default || right == default)
                return;

            var leftIndex = _leftToggles.IndexOf(left);
            var rightIndex = _rightToggles.IndexOf(right);

            _onToggleValueChanged?.Invoke(leftIndex, rightIndex);
        }
    }
}
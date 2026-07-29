using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
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
        private readonly List<CToggle> _leftToggles = new();
        private readonly List<CToggle> _rightToggles = new();

        private Action<int, int> _onToggleValueChanged;
        private CancellationTokenSource _cts;
        private bool _isLeftLearnLanguage;

        [Inject]
        public MatchWordsController(MatchWordsView view)
        {
            _view = view;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            Clear();
        }

        public void Clear()
        {
            _leftToggles.Clear();
            _rightToggles.Clear();
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(bool isLeftLearnLanguage, List<Word> left, List<Word> right,
            Action<int, int> onToggleValueChanged, Action onContinueClicked, Action<string, bool> onPlaySound)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _isLeftLearnLanguage = isLeftLearnLanguage;
            _onToggleValueChanged = onToggleValueChanged;
            _view.Init(onContinueClicked);

            for (int i = 0; i < left.Count; i++)
            {
                int index = i;
                CToggle toggle = _view.AddItem(true);
                _leftToggles.Add(toggle);

                string word = _isLeftLearnLanguage ? left[i].LearnWord : left[i].Translation;
                toggle.Set(word, left[i].Phonetics, isOn =>
                {
                    OnToggleValueChanged(true, index, isOn);
                    if (isOn && _isLeftLearnLanguage)
                    {
                        onPlaySound?.Invoke(left[index].WordKey, _isLeftLearnLanguage);
                    }
                });
                toggle.EnablePhonetics(_isLeftLearnLanguage && right[i].IsShowPhonetics);
            }

            for (int i = 0; i < right.Count; i++)
            {
                int index = i;
                CToggle toggle = _view.AddItem(false);
                _rightToggles.Add(toggle);

                string word = _isLeftLearnLanguage ? right[i].Translation : right[i].LearnWord;
                toggle.Set(word, right[i].Phonetics, isOn =>
                {
                    OnToggleValueChanged(false, index, isOn);
                    if (isOn && !_isLeftLearnLanguage)
                    {
                        onPlaySound?.Invoke(right[index].WordKey, !_isLeftLearnLanguage);
                    }
                });
                toggle.EnablePhonetics(!_isLeftLearnLanguage && right[i].IsShowPhonetics);
            }
        }

        public async UniTask ShowCorrectAsync(int leftIndex, int rightIndex, bool isCorrect)
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
            {
                return;
            }

            string column = isLeft ? "left" : "right";
            Debug.Log($"Clicked column: {column}; toggle: {index}; isOn: {isOn}");

            CToggle left = _leftToggles.FirstOrDefault(toggle => toggle.IsOn);
            CToggle right = _rightToggles.FirstOrDefault(toggle => toggle.IsOn);

            if (left == null || right == null)
            {
                return;
            }

            int leftIndex = _leftToggles.IndexOf(left);
            int rightIndex = _rightToggles.IndexOf(right);

            _onToggleValueChanged?.Invoke(leftIndex, rightIndex);
        }

        public void ShowHint()
        {
            if (_isLeftLearnLanguage)
            {
                _leftToggles.ForEach(toggle => toggle.EnablePhonetics(true));
            }
            else
            {
                _rightToggles.ForEach(toggle => toggle.EnablePhonetics(true));
            }
        }
    }
}
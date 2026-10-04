using System;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    /// <summary>
    /// Word toggle of the lesson pages. The look of its states (normal, selected, correct, wrong, inactive) comes either
    /// from overlay images (old UI) or from design variants through <see cref="DesignStates"/> (new UI).
    /// </summary>
    public class CToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _checkMark;
        [SerializeField] private Image _incorrect;
        [SerializeField] private Image _correct;
        [SerializeField] private Image _inactive;
        [SerializeField] private TMP_Text _word;
        [SerializeField] private TMP_Text _phonetics;

        [Header("Design states (optional)")]
        [SerializeField] private DesignStates _states;
        [SerializeField] private string _stateNormal = "Default";
        [SerializeField] private string _stateSelected = "Selected";
        [SerializeField] private string _stateCorrect = "Correct";
        [SerializeField] private string _stateWrong = "Wrong";
        [SerializeField] private string _stateInactive = "Inactive";

        private Action<bool> _onValueChanged;
        private bool? _result;
        private bool _isActive = true;

        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void Set(string word, string phonetic, ToggleGroup toggleGroup, Action<bool> onValueChanged)
        {
            Reset();
            _word.text = word;
            SetPhoneticText(phonetic);
            _onValueChanged = onValueChanged;
            _toggle.group = toggleGroup;
        }

        public void Set(string word, string phonetic, Action<bool> onValueChanged)
        {
            _word.text = word;
            SetPhoneticText(phonetic);
            _onValueChanged = onValueChanged;
            RefreshState();
        }

        public void SetIsOnWithoutNotify(bool isOn)
        {
            _toggle.SetIsOnWithoutNotify(isOn);
            RefreshState();
        }

        public void SetGroup(ToggleGroup toggleGroup)
        {
            _toggle.group = toggleGroup;
        }

        public void EnablePhonetics(bool enable)
        {
            if (_phonetics != null)
            {
                _phonetics.gameObject.SetActive(enable);
            }
        }

        private void SetPhoneticText(string phonetic)
        {
            if (_phonetics != null)
            {
                _phonetics.text = phonetic;
            }
        }

        public void SetInteractable(bool interactable)
        {
            _toggle.interactable = interactable;
        }

        public void SetActive(bool active)
        {
            SetVisible(_inactive, !active);
            _isActive = active;
            RefreshState();
        }

        public void SetCorrect(bool isCorrect)
        {
            SetVisible(_correct, isCorrect);
            SetVisible(_incorrect, !isCorrect);
            _result = isCorrect;
            RefreshState();
        }

        public void SetNormal()
        {
            SetVisible(_correct, false);
            SetVisible(_incorrect, false);
            _result = null;
            RefreshState();
        }

        private void OnToggleValueChanged(bool isOn)
        {
            RefreshState();
            _onValueChanged?.Invoke(isOn);
        }

        private void RefreshState()
        {
            if (_states == null)
            {
                return;
            }

            var state = !_isActive ? _stateInactive
                : _result == true ? _stateCorrect
                : _result == false ? _stateWrong
                : _toggle.isOn ? _stateSelected
                : _stateNormal;

            if (_states.Current != state && _states.Has(state))
            {
                _states.Apply(state);
            }
        }

        private static void SetVisible(Image image, bool visible)
        {
            if (image != null)
            {
                image.gameObject.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }

        private void Reset()
        {
            SetVisible(_checkMark, true);
            SetVisible(_inactive, false);
            SetVisible(_correct, false);
            SetVisible(_incorrect, false);
            _result = null;
            _isActive = true;
            RefreshState();
        }
    }
}
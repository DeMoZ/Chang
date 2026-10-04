using System.Collections.Generic;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Chang.UI
{
    public class PagesContinueView : MonoBehaviour
    {
        [SerializeField] public Button _continueBtn;
        [SerializeField] private Color _correctColor = Color.green;
        [SerializeField] private Color _wrongColor = Color.red;
        [SerializeField] private TMP_Text _info;
        [SerializeField] private Image _background;

        [Header("Design (optional): FeedbackSheet variants Correct / Wrong instead of tint colors")]
        [SerializeField] private DesignStates _states;
        [Tooltip("Continue button shown in the wrong state, when the variant uses another button")]
        [SerializeField] private Button _wrongContinueBtn;
        [Tooltip("When set, the info text is split: first line here, the rest in Info")]
        [SerializeField] private TMP_Text _infoTitle;

        public Button ContinueBtn => _continueBtn;

        public void AddContinueListener(UnityAction listener)
        {
            foreach (var button in Buttons())
            {
                button.onClick.AddListener(listener);
            }
        }

        public void RemoveContinueListener(UnityAction listener)
        {
            foreach (var button in Buttons())
            {
                button.onClick.RemoveListener(listener);
            }
        }

        public void Set(ContinueButtonInfo info)
        {
            if (_states != null)
            {
                _states.Apply(info.IsCorrect ? "Correct" : "Wrong");
            }
            else
            {
                var color = info.IsCorrect ? _correctColor : _wrongColor;
                ContinueBtn.targetGraphic.color = color;
                _background.color = color;
            }

            if (_infoTitle != null)
            {
                var text = info.InfoText ?? string.Empty;
                var lineBreak = text.IndexOf('\n');
                _infoTitle.text = lineBreak < 0 ? text : text.Substring(0, lineBreak);
                _info.text = lineBreak < 0 ? string.Empty : text.Substring(lineBreak + 1).Replace('\n', ' ');
            }
            else
            {
                _info.text = info.InfoText;
            }
        }

        private IEnumerable<Button> Buttons()
        {
            yield return _continueBtn;
            if (_wrongContinueBtn != null)
            {
                yield return _wrongContinueBtn;
            }
        }
    }
}

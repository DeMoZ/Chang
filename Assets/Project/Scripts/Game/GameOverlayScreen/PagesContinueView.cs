using TMPro;
using UnityEngine;
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

        public Button ContinueBtn => _continueBtn;

        public void Set(ContinueButtonInfo info)
        {
            var color = info.IsCorrect ? _correctColor : _wrongColor;
            ContinueBtn.targetGraphic.color = color;
            _background.color = color;
            _info.text = info.InfoText;
        }
    }
}
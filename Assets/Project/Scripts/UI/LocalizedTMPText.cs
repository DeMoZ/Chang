using Assets.SimpleLocalization.Scripts;
using TMPro;
using UnityEngine;

namespace Chang.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPText : MonoBehaviour
    {
        [SerializeField] private string _localizationKey;

        private TMP_Text _text;

        // the design text is shown while the key is not in the sheet yet
        private string _fallback;

        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                Localize();
            }
        }

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _fallback = _text.text;
        }

        private void OnEnable()
        {
            Localize();
            LocalizationManager.OnLocalizationChanged += Localize;
        }

        private void OnDisable()
        {
            LocalizationManager.OnLocalizationChanged -= Localize;
        }

        private void Localize()
        {
            if (_text == null || string.IsNullOrEmpty(_localizationKey)) return;

            _text.text = Chang.Services.LocalizationService.Localize(_localizationKey, _fallback);
        }
    }
}

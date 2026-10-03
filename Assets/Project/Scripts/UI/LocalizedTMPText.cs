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

            _text.text = LocalizationManager.Localize(_localizationKey);
        }
    }
}

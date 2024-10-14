using TMPro;
using UnityEngine;

namespace Chang
{
    public class ChangText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _transcriptionText;

        public void Set(string text, string transcriptionText, Sprite sprite = null, AudioClip audioClip = null)
        {
            _text.text = text;
            _transcriptionText.text = transcriptionText;

            if (sprite != null) { }
            if (audioClip != null) { }
        }

        public void SetTranctiptionActive(bool active)
        {
            _transcriptionText.gameObject.SetActive(active);
        }
    }
}
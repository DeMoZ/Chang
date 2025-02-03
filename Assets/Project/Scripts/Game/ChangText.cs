using TMPro;
using UnityEngine;

namespace Chang
{
    public class ChangText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _word;
        [SerializeField] private TMP_Text _phonetic;

        public void Set(string word, string phonetic, Sprite sprite = null, AudioClip audioClip = null)
        {
            _word.text = word;
            _phonetic.text = phonetic;

            if (sprite != null)
            {
            }

            if (audioClip != null)
            {
            }
        }

        public void EnablePhonetic(bool enable)
        {
            _phonetic.gameObject.SetActive(enable);
        }
    }
}
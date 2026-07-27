using UnityEngine;

namespace Chang.Core
{
    [System.Serializable]
    public class Word
    {
        public Languages Language;
        public string Section;
            
        public string WordKey;      // Thai/Vocabulary/Food/Fried_rice
        public string ImageKey;     // Thai/Vocabulary/Food/Fried_rice
        public string SoundKey;     // Thai/Vocabulary/Food/Fried_rice
        public string Key;          // Fried_rice

        public string LearnWord;
        public string Phonetics;
        public string DefaultTranslation;
        public string DefaultDescription;

        // runtime properties
        public bool IsShowPhonetics { get; private set; }
        public Sprite Sprite { get; private set; }

        public void SetShowPhonetics(bool getShowPhonetics)
        {
            IsShowPhonetics = getShowPhonetics;
        }

        public void SetSprite(Sprite sprite)
        {
            Sprite = sprite;
        }

        public string GetTranslation()
        {
            // todo chang Get Translation from i2language
            return DefaultTranslation;
        }
    }
}
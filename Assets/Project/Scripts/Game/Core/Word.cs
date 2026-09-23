using UnityEngine;

namespace Chang.Core
{
    [System.Serializable]
    public class Word
    {
        public Languages Language;
        public string Section;

        public string WordKey; // Thai/Vocabulary/Food/Fried_rice
        public string ImageKey; // Thai/Vocabulary/Food/Fried_rice
        public string SoundKey; // Thai/Vocabulary/Food/Fried_rice
        public string Key; // Fried_rice

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

        public string Translation
        {
            get
            {
                // todo chang Get Translation from i2language
                return DefaultTranslation;
            }
        }

        public static Word CreateEmptyPlaceholder(string replacedWord)
        {
            // count text elements, so Thai combining vowels and tone marks don't add extra underscores
            int length = string.IsNullOrEmpty(replacedWord)
                ? 1
                : new System.Globalization.StringInfo(replacedWord).LengthInTextElements;

            return new Word
            {
                WordKey = string.Empty,
                ImageKey = string.Empty,
                SoundKey = string.Empty,
                Key = string.Empty,
                LearnWord = new string('_', length),
                Phonetics = string.Empty,
                DefaultTranslation = string.Empty,
                DefaultDescription = string.Empty,
            };
        }
    }
}
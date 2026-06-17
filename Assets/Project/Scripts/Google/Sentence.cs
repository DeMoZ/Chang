using System.Collections.Generic;

namespace Chang.GoogleSheets
{
    public class Sentence
    {
        public Languages Language;
        public string Section;

        public string Key; // Do_not_add_sugar
        public string SentenceKey; // Thai/Sentences/Market/Do_not_add_sugar      
        public string ImageKey; // Thai/Sentences/Market/Do_not_add_sugar         
        public string SoundKey; // Thai/Sentences/Market/Do_not_add_sugar         

        public string DefaultTranslation;
        public List<SentenceWord> SentenceWords;
    }

    /// <summary>
    /// the word in the sentence with additional info
    /// </summary>
    public class SentenceWord
    {
        public Modifier Modifiers;
        public string WordKey;

        public void SetModifiers(string value)
        {
            Modifiers = Modifier.None;

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (value.Contains('V'))
            {
                Modifiers |= Modifier.Variant;
            }

            if (value.Contains('D'))
            {
                Modifiers |= Modifier.Dynamic;
            }

            if (value.Contains('G'))
            {
                Modifiers |= Modifier.Gender;
            }
        }
    }

    // Key	            Do_not_add_sugar
    // SentenceKey	    Thai/Sentences/Market/Do_not_add_sugar
    // ImageKey	        Thai/Sentences/Market/Do_not_add_sugar
    // SoundKey	        Thai/Sentences/Market/Do_not_add_sugar	

    // DefaultTranslation	Do not add sugar			

    // CompareWordsTranslation		No / Not / Don't	                        Put / Add	                Sugar
    // СompareWordsKeys		        Thai/Vocabulary/Mix/No_Not_Don_t	Thai/Vocabulary/Mix/Put_Add	Thai/Vocabulary/Mix/Sugar
}
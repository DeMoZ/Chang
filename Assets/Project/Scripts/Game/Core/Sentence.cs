using System.Collections.Generic;

namespace Chang.Core
{
    public class Sentence
    {
        public Languages Language;
        public string Section;

        public string Key;          // Do_not_add_sugar
        public string SentenceKey;  // Thai/Sentences/Market/Do_not_add_sugar      
        public string ImageKey;     // Thai/Sentences/Market/Do_not_add_sugar         
        public string SoundKey;     // Thai/Sentences/Market/Do_not_add_sugar         

        public string DefaultTranslation;

        // public string CompareWordsTranslation;
        //public List<string> CompareWordsKeys;

        public List<SentenceWord> SentenceWord;
    }

    /// <summary>
    /// the word in the sentence with additional info
    /// </summary>
    public class SentenceWord
    {
        // Modifiers
        public bool Variant;
        public bool Dynamic;
        public bool Gender;

        public string WordKey;
    }

    // Key	            Do_not_add_sugar
    // SentenceKey	    Thai/Sentences/Market/Do_not_add_sugar
    // ImageKey	        Thai/Sentences/Market/Do_not_add_sugar
    // SoundKey	        Thai/Sentences/Market/Do_not_add_sugar	

    // DefaultTranslation	Do not add sugar			

    // CompareWordsTranslation		No / Not / Don't	                        Put / Add	                Sugar
    // СompareWordsKeys		        Thai/Vocabulary/Mix/No_Not_Don_t	Thai/Vocabulary/Mix/Put_Add	Thai/Vocabulary/Mix/Sugar
}
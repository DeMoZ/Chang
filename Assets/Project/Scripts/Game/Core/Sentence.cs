using System;
using System.Collections.Generic;
using System.Linq;

[Flags]
public enum Modifier // V-Variant D-Dynamic G-Gender
{
    None,
    Variant = 1 << 0, // variants words are for add into mix words. they not supposed to be chosen by the player, and if he do - that is his fault
    Dynamic = 1 << 1, // dynamic words are for different forms of the same word. for example: apple and banana are dynamic words of fruit. In the sentence it will be replaced with the same word as in the question.
    Gender = 1 << 2
}

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
        public List<SentenceWord> SentenceWords;

        public Sentence()
        {
        }

        /// <summary>
        /// Copy constructor. Deep copies SentenceWords so the original (e.g. cached in Bus.Sentences)
        /// is not affected by later mutations of the copy's words.
        /// </summary>
        public Sentence(Sentence other)
        {
            Language = other.Language;
            Section = other.Section;
            Key = other.Key;
            SentenceKey = other.SentenceKey;
            ImageKey = other.ImageKey;
            SoundKey = other.SoundKey;
            DefaultTranslation = other.DefaultTranslation;
            SentenceWords = other.SentenceWords?.Select(word => new SentenceWord(word)).ToList();
        }
    }

    /// <summary>
    /// the word in the sentence with additional info
    /// </summary>
    public class SentenceWord
    {
        public Modifier Modifiers;
        public string WordKey;
        public int DisplayIndex; // Display word if sentence mark is below this index.

        public SentenceWord()
        {
        }

        public SentenceWord(SentenceWord other)
        {
            Modifiers = other.Modifiers;
            WordKey = other.WordKey;
            DisplayIndex = other.DisplayIndex;
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
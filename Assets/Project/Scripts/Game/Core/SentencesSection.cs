using System.Collections.Generic;

namespace Chang.Core
{
    public class SentencesSection
    {
        public Languages Language;
        public string Section; // will use it as default translation
        public string SectionKey; // Thai/SentencesBook/Fruits
        public string DefaultTranslation => Section;
        public List<Lesson> SectionLessons;
        
        // Section	Fruits	
        //         Lesson1	Thai/Sentences/Market/I_want_to_buy_fruit
        //                  Thai/Sentences/Market/I_want_to_buy_fruit
        //                  Thai/Sentences/Market/I_want_to_buy_fruit
        //                  Thai/Sentences/Market/I_want_to_buy_fruit
        //         Lesson2	Thai/Sentences/Market/I_want_to_buy_fruit
        //                  Thai/Sentences/Market/I_want_to_buy_fruit
        //                  Thai/Sentences/Market/I_want_to_buy_fruit
        //                  Thai/Sentences/Market/I_want_to_buy_fruit

        // Section	Food	
        //         Lesson1	Thai/Vocabulary/Food/Fried_rice
    }
}
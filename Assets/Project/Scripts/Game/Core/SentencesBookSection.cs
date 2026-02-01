using System.Collections.Generic;

namespace Chang.Core
{
    public class SentencesBookSection
    {
        public Languages Language;
        public string Section;    // will use it as default translation
        public string TitleKey; // Thai/Fruits
        public string DefaultTranslation => Section;
        public List<SectionLesson> SectionLessons;
        
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
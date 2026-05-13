using System.Collections.Generic;

namespace Chang.Core
{
    public class VocabularyBookSection
    {
        public Languages Language;
        public string Section;    // will use it as default translation
        public string SectionKey; // Thai/VocabularyBook/Fruits
        public string DefaultTranslation => Section;
        public List<Lesson> Lessons;
        
        // Section	Fruits	
        //         Lesson1	Thai/Vocabulary/Fruits/Fruit
        //                 Thai/Vocabulary/Fruits/Watermelon
        //                 Thai/Vocabulary/Fruits/Mango
        //                 Thai/Vocabulary/Fruits/Pineapple
        //                 Thai/Vocabulary/Fruits/Papaya
        //         Lesson2	Thai/Vocabulary/Fruits/Guava
        //                 Thai/Vocabulary/Fruits/Banana
        //                 Thai/Vocabulary/Fruits/Orange
        //                 Thai/Vocabulary/Fruits/Coconut
        //                 Thai/Vocabulary/Fruits/Durian
        //
        // Section	Food	
        //         Lesson1	Thai/Vocabulary/Food/Fried_rice
    }
}
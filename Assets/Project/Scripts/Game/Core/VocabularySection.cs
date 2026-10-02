using System.Collections.Generic;
using System.Linq;

namespace Chang.Core
{
    public class VocabularySection
    {
        public Languages Language;
        public string Section; // will use it as default translation for section to English for now
        public string SectionKey; // Thai/VocabularyBook/Fruits
        public string DefaultTranslation => Section;
        public List<Lesson> Lessons;

        public void PopulateQuestions()
        {
            foreach (Lesson lesson in Lessons)
            {
                List<IQuestion> questions = new List<IQuestion>();

                foreach (var key in lesson.Keys)
                {
                    IQuestion question = new QuestSelectWord
                    {
                        Key = key,
                        WordsKeys = lesson.Keys.Where(k => !k.Equals(key)).ToHashSet(),
                        SectionKey = SectionKey,
                        // Language = lesson.Language
                    };

                    questions.Add(question);
                }

                lesson.SetQuestions(questions);
            }
        }
        
        // Section	Fruits	
        //         Lesson1 Thai/Vocabulary/Fruits/Fruit
        //                 Thai/Vocabulary/Fruits/Watermelon
        //                 Thai/Vocabulary/Fruits/Mango
        //                 Thai/Vocabulary/Fruits/Pineapple
        //                 Thai/Vocabulary/Fruits/Papaya
        //         Lesson2 Thai/Vocabulary/Fruits/Guava
        //                 Thai/Vocabulary/Fruits/Banana
        //                 Thai/Vocabulary/Fruits/Orange
        //                 Thai/Vocabulary/Fruits/Coconut
        //                 Thai/Vocabulary/Fruits/Durian
        //
        // Section	Food	
        //         Lesson1 Thai/Vocabulary/Food/Fried_rice
    }
}
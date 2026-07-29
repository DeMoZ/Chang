using System.Collections.Generic;
using System.Linq;
using Chang.Core;

namespace Chang
{
    public class GameBus
    {
        public VocabularyBook VocabularyBook { get; private set; }
        public SentencesBook SentencesBook { get; private set; }
        public Dictionary<string, Word> Words { get; private set; }
        public Dictionary<string, Sentence> Sentences { get; private set; }
        public Dictionary<string, VocabularyBookSection> VocabularySections { get; private set; }

        // public Dictionary<string, Sentences.Deprecated.LessonData> SentencesLessons { get; set; }

        public Lesson Lesson { get; private set; }

        public GameType GameType { get; set; }

        public void SetVocabularyBook(VocabularyBook book)
        {
            VocabularyBook = book;
            SetVocabularySections();
        }

        public void SetWords(Dictionary<string, Word> words)
        {
            Words = words;
        }
        
        public void SetSentencesBook(SentencesBook book)
        {
            SentencesBook = book;
        }

        public void SetSentences(List<Sentence> sentences)
        {
            Sentences = sentences.Select(sentence =>  new KeyValuePair<string, Sentence>(sentence.SentenceKey, sentence))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        
        public void SetLesson(Lesson lesson)
        {
            Lesson = lesson;
        }

        private void SetVocabularySections()
        {
            VocabularySections = VocabularyBook.Sections
                .ToDictionary(section => section.SectionKey, section => section);
        }

        //  key = $"{_profileService.ProfileData.LearnLanguage}Lesson{sectionName}_{lessonIndex}";

        // private void SetVocabularyLessons(List<VocabularyBookSection> sections)
        // {
        //     // Bus.VocabularySections = Bus.VocabularyBookData.Sections
        //     //     .ToDictionary(section => section.SectionKey, section => section);
        //     VocabularyLessons = new Dictionary<string, Lesson>();
        //     
        //     foreach (VocabularyBookSection section in sections)
        //     {
        //         foreach (Lesson lesson in section.Lessons)
        //         {
        //             string key = string.Empty; // todo chang есть где то клас который обращается к VocabularyLessons по ключу. Этот ключ надо и сдесь повторить.
        //             VocabularyLessons[key] = lesson;
        //         }
        //     }
        // }
    }
}
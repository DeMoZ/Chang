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
        public Dictionary<string, SentencesSection> SentencesSections { get; set; }

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
            SetSentencesSections();
        }

        public void SetSentences(List<Sentence> sentences)
        {
            Sentences = sentences.Select(sentence => new KeyValuePair<string, Sentence>(sentence.SentenceKey, sentence))
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

        private void SetSentencesSections()
        {
            SentencesSections = SentencesBook.Sections
                .ToDictionary(section => section.SectionKey, section => section);
        }
    }
}
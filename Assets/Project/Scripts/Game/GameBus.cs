using System.Collections.Generic;

namespace Chang
{
    public class GameBus
    {
        public Vocabulary.VocabularyBookData VocabularyBookData { get; set; }
        public Sentences.SentencesBookData SentencesBookData { get; set; }

        /// <summary>
        /// Runtime collection to get Lesson data by name.
        /// </summary>
        public Dictionary<string, Vocabulary.LessonData> VocabularyLessons { get; set; }

        public Dictionary<string, Sentences.LessonData> SentencesLessons { get; set; }
        
        public ILesson CurrentLesson { get; set; }

        public GameType GameType { get; set; }
    }
}
using System.Collections.Generic;
using Chang.Core;

namespace Chang
{
    public class GameBus
    {
        public VocabularyBookData VocabularyBookData { get; set; }
        public SentencesBookData SentencesBookData { get; set; }

        /// <summary>
        /// Runtime collection to get Lesson data by name.
        /// </summary>
        // public Dictionary<string, Vocabulary.Deprecated.LessonData> VocabularyLessons { get; set; }
        // public Dictionary<string, VocabularyBookSection> VocabularySections = new Dictionary<string, VocabularyBookSection>();

        // public Dictionary<string, Sentences.Deprecated.LessonData> SentencesLessons { get; set; }

        public ILessonProvider LessonProvider { get; set; }

        public GameType GameType { get; set; }
    }
}
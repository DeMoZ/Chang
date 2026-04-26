using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Chang.Core
{
    public interface ILessonProvider
    {
        List<IQuestion> Questions { get; }
        Queue<IQuestion> QuestionQueue { get; }
        bool IsGeneratedMathWordsQuestPlayed { get; set; }
        IQuestion CurrentQuestion { get; set; }

        void AddQuestion(IQuestion question);
        
        IQuestion PeekNextQuestion();
        void EnqueueCurrentQuestion();
        void DequeueAndSetSipmlQuestion();
    }

    /// <summary>
    /// Provides all the data related to a lesson
    /// </summary>
    public class LessonProvider : ILessonProvider
    {
        public Lesson Lesson { get; private set; }

        public bool IsGeneratedMathWordsQuestPlayed { get; set; }

        public List<IQuestion> Questions { get; private set; }
        public Queue<IQuestion> QuestionQueue { get; }
        public IQuestion CurrentQuestion { get;  set; }

        public async UniTask Initialise(Lesson lesson)
        {
            Lesson = lesson;

            // async load questions based on lesson data
            // get vocabulary or sentences list from GameBus
            Questions = new List<IQuestion>();
        }

        public void EnqueueCurrentQuestion()
        {
            QuestionQueue.Enqueue(CurrentQuestion);
        }
        
        public void DequeueAndSetSipmlQuestion()
        {
            throw new System.NotImplementedException();
        }

        public void AddQuestion(IQuestion question)
        {
            Questions.Add(question);
            QuestionQueue.Enqueue(question);
        }

        public IQuestion PeekNextQuestion()
        {
            return QuestionQueue.Peek();
        }
    }
}
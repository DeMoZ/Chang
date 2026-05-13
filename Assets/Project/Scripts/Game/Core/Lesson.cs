using System.Collections.Generic;

namespace Chang.Core
{
    /// <summary>
    /// Book contanis lessons that determined by sections in Google Sheets page.
    /// </summary>
    public class Lesson
    {
        public Languages Language;
        public string Section; 
        public List<string> Keys;
        
        /// <summary>
        /// selected name. Used to find config with the same name
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        public IQuestion CurrentQuestion { get; private set; }
        public List<IQuestion> Questions { get; private set; }
        public Queue<IQuestion> QuestionQueue { get; private set; }
        public bool IsGeneratedMathWordsQuestPlayed { get; set; }

        public void Dispose()
        {
            FileName = string.Empty;
            CurrentQuestion = null;
            QuestionQueue.Clear();
        }

        public void DequeueAndSetSipmlQuestion()
        {
            CurrentQuestion = QuestionQueue.Dequeue();
        }

        public IQuestion PeekNextQuestion()
        {
            return QuestionQueue.Peek();
        }

        /// <summary>
        /// Add quest to the beginning of the queue (example: add demonstration screen)
        /// </summary> 
        public void InsertNextQuest(IQuestion quest)
        {
            var tempList = new List<IQuestion>(QuestionQueue);
            tempList.Insert(0, quest);
            QuestionQueue = new Queue<IQuestion>(tempList);
        }

        public void SetSimpleQuestions(List<IQuestion> questions)
        {
            Questions = questions;
            QuestionQueue = new Queue<IQuestion>(questions);
        }

        public void AddSimpleQuestion(IQuestion question)
        {
            Questions.Add(question);
            QuestionQueue.Enqueue(question);
        }

        public void EnqueueCurrentQuestion()
        {
            QuestionQueue.Enqueue(CurrentQuestion);
        }
    }
}
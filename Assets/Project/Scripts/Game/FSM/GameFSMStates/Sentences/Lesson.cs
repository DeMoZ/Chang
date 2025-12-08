using System;
using System.Collections.Generic;

namespace Chang.Sentences
{
    /// <summary>
    /// Stores the data for the current selected or generated lesson 
    /// </summary>
    public class Lesson : ILesson, IDisposable
    {
        /// <summary>
        /// selected name. Used to find config with the same name
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        public IQuestion currentQuestion { get; private set; }
        public List<IQuestion> SimpleQuestions { get; private set; }
        public Queue<IQuestion> SimpleQuestionQueue { get; private set; }
        public bool IsGeneratedMathWordsQuestPlayed { get; set; }

        public void Dispose()
        {
            FileName = null;
            currentQuestion = null;
            SimpleQuestionQueue.Clear();
        }

        public void DequeueAndSetSipmlQuestion()
        {
            currentQuestion = SimpleQuestionQueue.Dequeue();
        }

        public IQuestion PeekNextQuestion()
        {
            return SimpleQuestionQueue.Peek();
        }

        /// <summary>
        /// Add quest to the beginning of the queue (example: add demonstration screen)
        /// </summary> 
        public void InsertNextQuest(IQuestion quest)
        {
            var tempList = new List<IQuestion>(SimpleQuestionQueue);
            tempList.Insert(0, quest);
            SimpleQuestionQueue = new Queue<IQuestion>(tempList);
        }

        public void SetSimpleQuestions(List<IQuestion> questions)
        {
            SimpleQuestions = questions;
            SimpleQuestionQueue = new Queue<IQuestion>(questions);
        }

        public void AddSimpleQuestion(IQuestion question)
        {
            SimpleQuestions.Add(question);
            SimpleQuestionQueue.Enqueue(question);
        }

        public void EnqueueCurrentQuestion()
        {
            SimpleQuestionQueue.Enqueue(currentQuestion);
        }
    }
}
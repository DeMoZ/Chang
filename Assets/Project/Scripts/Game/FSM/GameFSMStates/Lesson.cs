using System;
using System.Collections.Generic;

namespace Chang
{
    /// <summary>
    /// Stores the data for the current selected or generated lesson 
    /// </summary>
    public class Lesson : IDisposable
    {
        /// <summary>
        /// selected name. Used to find config with the same name
        /// </summary>
        public string FileName { get; private set; }

        public SimpleQuestionBase CurrentSimpleQuestion { get; private set; }
        public List<SimpleQuestionBase> SimpleQuestions { get; private set; }
        public Queue<SimpleQuestionBase> SimpleQuestionQueue { get; private set; }
        public QuestBase CurrentQuestion { get; private set; }

        /// <summary>
        /// todo roman if this lesson is not preconfigured, the content should be loaded in the other way ?
        /// </summary>
        public bool IsConfig => string.IsNullOrEmpty(FileName);

        public void Dispose()
        {
            FileName = null;
            CurrentSimpleQuestion = null;
            SimpleQuestionQueue.Clear();
        }

        public void SetFileName(string fileName)
        {
            FileName = fileName;
        }

        public void DequeueAndSetSipmQiestion()
        {
            CurrentSimpleQuestion = SimpleQuestionQueue.Dequeue();
        }

        public SimpleQuestionBase PeekNextQuestion()
        {
            return SimpleQuestionQueue.Peek();
        }
        
        /// <summary>
        /// Add quest to the beginning of the queue (example: add demonstration screen)
        /// </summary> 
        public void InsertNextQuest(SimpleQuestionBase quest)
        {
            var tempList = new List<SimpleQuestionBase>(SimpleQuestionQueue);
            tempList.Insert(0, quest);
            SimpleQuestionQueue = new Queue<SimpleQuestionBase>(tempList);
        }

        public void SetSimpleQuestions(List<SimpleQuestionBase> questions)
        {
            SimpleQuestions = questions;
            SimpleQuestionQueue = new Queue<SimpleQuestionBase>(questions);
        }

        public void SetCurrentQuestionConfig(QuestBase quest)
        {
            CurrentQuestion = quest;
        }

        public void EnqueueCurrentQuestion()
        {
            SimpleQuestionQueue.Enqueue(CurrentSimpleQuestion);
        }
    }
}
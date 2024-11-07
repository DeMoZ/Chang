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

        public SimpleQuestionDataBase CurrentSimpleQuestion { get; private set; }
        public List<SimpleQuestionDataBase> SimpleQuestions { get; private set; }
        public Queue<SimpleQuestionDataBase> SimpleQuestionQueue { get; private set; }
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

        public void SetCurrentSimpQiestion()
        {
            CurrentSimpleQuestion = SimpleQuestionQueue.Dequeue();
        }

        public void SetSimpQuesitons(List<SimpleQuestionDataBase> simpQuestions)
        {
            SimpleQuestions = simpQuestions;
            SimpleQuestionQueue = new Queue<SimpleQuestionDataBase>(simpQuestions);
        }

        public void SetCurrentQuestionConfig(QuestBase question)
        {
            CurrentQuestion = question;
        }

        public void EnqueueCurrentQuestion()
        {
            SimpleQuestionQueue.Enqueue(CurrentSimpleQuestion);
        }
    }
}
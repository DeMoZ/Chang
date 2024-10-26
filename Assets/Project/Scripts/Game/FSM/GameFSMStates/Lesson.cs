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

        public SimpQuestionDataBase CurrentSimpQuestion { get; private set; }
        public List<SimpQuestionDataBase> SimpQuestions { get; private set; }
        public Queue<SimpQuestionDataBase> SimpQuestionQueue { get; private set; }
        public QuestBase CurrentQuestion { get; private set; }

        /// <summary>
        /// todo roman if this lesson is not preconfigured, the content should be loaded in the other way ?
        /// </summary>
        public bool IsConfig => string.IsNullOrEmpty(FileName);

        public void Dispose()
        {
            FileName = null;
            CurrentSimpQuestion = null;
            SimpQuestionQueue.Clear();
        }

        public void SetFileName(string fileName)
        {
            FileName = fileName;
        }

        public void SetCurrentSimpQiestion()
        {
            CurrentSimpQuestion = SimpQuestionQueue.Dequeue();
        }

        public void SetSimpQuesitons(List<SimpQuestionDataBase> simpQuestions)
        {
            SimpQuestions = simpQuestions;
            SimpQuestionQueue = new Queue<SimpQuestionDataBase>(simpQuestions);
        }

        public void SetCurrentQuestionConfig(QuestBase question)
        {
            CurrentQuestion = question;
        }

        public void EnqueueCurrentQuestion()
        {
            SimpQuestionQueue.Enqueue(CurrentSimpQuestion);
        }
    }
}
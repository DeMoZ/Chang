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
        public string FileName = string.Empty;
        public bool GenerateQuestMatchWordsData;
        public bool IsGeneratedMathWordsQuestPlayed;
        
        public ISimpleQuestion CurrentSimpleQuestion { get; private set; }
        public IQuestData CurrentQuestionData { get; private set; }
        public List<ISimpleQuestion> SimpleQuestions { get; private set; }
        public Queue<ISimpleQuestion> SimpleQuestionQueue { get; private set; }
        
        public void Dispose()
        {
            FileName = null;
            CurrentSimpleQuestion = null;
            SimpleQuestionQueue.Clear();
        }

        public void DequeueAndSetSipmlQuestion()
        {
            CurrentSimpleQuestion = SimpleQuestionQueue.Dequeue();
        }

        public ISimpleQuestion PeekNextQuestion()
        {
            return SimpleQuestionQueue.Peek();
        }
        
        /// <summary>
        /// Add quest to the beginning of the queue (example: add demonstration screen)
        /// </summary> 
        public void InsertNextQuest(ISimpleQuestion quest)
        {
            var tempList = new List<ISimpleQuestion>(SimpleQuestionQueue);
            tempList.Insert(0, quest);
            SimpleQuestionQueue = new Queue<ISimpleQuestion>(tempList);
        }

        public void SetSimpleQuestions(List<ISimpleQuestion> questions)
        {
            SimpleQuestions = questions;
            SimpleQuestionQueue = new Queue<ISimpleQuestion>(questions);
        }
        
        public void AddSimpleQuestion(ISimpleQuestion question)
        {
            SimpleQuestions.Add(question);
            SimpleQuestionQueue.Enqueue(question);
        }
        
        public void SetCurrentQuestionData(IQuestData data)
        {
            CurrentQuestionData = data;
        }

        public void EnqueueCurrentQuestion()
        {
            SimpleQuestionQueue.Enqueue(CurrentSimpleQuestion);
        }
    }
}
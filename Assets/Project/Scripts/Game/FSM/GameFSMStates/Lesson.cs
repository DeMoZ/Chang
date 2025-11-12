using System;
using System.Collections.Generic;

namespace Chang.Vocabulary
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
        
        public Vocabulary.IQuestion currentQuestion { get; private set; }
        public List<Vocabulary.IQuestion> SimpleQuestions { get; private set; }
        public Queue<Vocabulary.IQuestion> SimpleQuestionQueue { get; private set; }
        
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

        public Vocabulary.IQuestion PeekNextQuestion()
        {
            return SimpleQuestionQueue.Peek();
        }
        
        /// <summary>
        /// Add quest to the beginning of the queue (example: add demonstration screen)
        /// </summary> 
        public void InsertNextQuest(Vocabulary.IQuestion quest)
        {
            var tempList = new List<Vocabulary.IQuestion>(SimpleQuestionQueue);
            tempList.Insert(0, quest);
            SimpleQuestionQueue = new Queue<Vocabulary.IQuestion>(tempList);
        }

        public void SetSimpleQuestions(List<Vocabulary.IQuestion> questions)
        {
            SimpleQuestions = questions;
            SimpleQuestionQueue = new Queue<Vocabulary.IQuestion>(questions);
        }
        
        public void AddSimpleQuestion(Vocabulary.IQuestion question)
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
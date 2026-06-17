using System;
using System.Collections.Generic;

namespace Chang.Core
{
    public class Lesson : IDisposable
    {
        public Languages Language { get; private set; }
        public string Section { get; private set; }
        public List<string> Keys { get; private set; }

        /// <summary>
        /// selected name. Used to find config with the same name
        /// </summary>
        public IQuestion CurrentQuestion { get; private set; }

        public List<IQuestion> Questions { get; private set; }
        public Queue<IQuestion> QuestionQueue { get; private set; }
        public bool IsGeneratedMathWordsQuestPlayed { get; set; }

        public Lesson(Languages language, string section, List<string> keys)
        {
            Language = language;
            Section = section;
            Keys = keys;
        }

        public void Dispose()
        {
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

        public void SetQuestions(List<IQuestion> questions)
        {
            Questions = questions;
            QuestionQueue = new Queue<IQuestion>(questions);
        }

        public void AddQuestion(IQuestion question)
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
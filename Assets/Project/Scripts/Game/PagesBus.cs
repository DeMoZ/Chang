using System;
using System.Collections.Generic;
using Chang.FSM;
using Chang.Core;
using DMZ.Events;

namespace Chang
{
    public class PagesBus : IDisposable
    {
        public Lesson Lesson { get; set; }
        public GameType GameType { get; set; }
        public Dictionary<string, Word> Words { get; set; }
        public IQuestionResult QuestionResult { get; set; }
        public List<IQuestionResult> LessonLog { get; } = new();
        public DMZState<bool> OnHintUsed { get; set; } = new();

        public void Dispose()
        {
            LessonLog.Clear();
            Lesson = null;
            Words = null;
        }
    }
}
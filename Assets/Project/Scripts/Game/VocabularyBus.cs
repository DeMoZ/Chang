using System;
using System.Collections.Generic;
using Chang.FSM;
using DMZ.Events;

namespace Chang
{
    public class VocabularyBus : IDisposable
    {
        public Lesson CurrentLesson { get; set; }
        public IQuestionResult QuestionResult { get; set; }
        public List<IQuestionResult> LessonLog { get; } = new();
        public DMZState<bool> OnHintUsed { get; set; } = new();

        public void Dispose()
        {
            LessonLog.Clear();
            CurrentLesson = null;
        }
    }
}
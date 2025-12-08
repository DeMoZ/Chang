using System.Collections.Generic;

namespace Chang
{
    public interface ILesson
    {
        string FileName { get; }
        bool IsGeneratedMathWordsQuestPlayed { get; set; }
        List<IQuestion> SimpleQuestions { get; }
        Queue<IQuestion> SimpleQuestionQueue { get; }

        IQuestion PeekNextQuestion();
        void AddSimpleQuestion(IQuestion question);
        void InsertNextQuest(IQuestion demonstration);
        void DequeueAndSetSipmlQuestion();
    }
}
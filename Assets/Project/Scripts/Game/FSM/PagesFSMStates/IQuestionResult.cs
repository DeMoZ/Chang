using System.Collections.Generic;
using Chang.Core;

namespace Chang.FSM
{
    public interface IQuestionResult
    {
        public ChangTypes Type { get; }
        public string Key { get; }
        public string Presentation { get; }
        public bool IsCorrect { get; }
        public bool IsHintUsed { get; }
    }

    public class MatchWordsResult : IQuestionResult
    {
        public ChangTypes Type => ChangTypes.MatchWords;
        public List<WordResult> WordResults { get; } = new ();

        public string Key
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public string Presentation
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public bool IsCorrect
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public bool IsHintUsed
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
using System.Collections.Generic;

namespace Chang.Deprecated
{
    public interface IQuestion
    {
        ChangTypes QuestionType { get; }
        HashSet<string> GetConfigKeys();
        HashSet<string> GetSoundKeys();
        HashSet<string> GetImageKeys();
        HashSet<string> GetNeedDemonstrationKeys();
    }
}
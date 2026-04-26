using System.Collections.Generic;

namespace Chang.Core
{
    public interface IQuestion
    {
        ChangTypes Type { get; }

        HashSet<string> GetWordsKeys { get; }
        HashSet<string> GetSoundKeys { get; }
        HashSet<string> GetImageKeys { get; }
        HashSet<string> GetNeedDemonstrationKeys { get; }
    }

    public class QuestMatchWords : IQuestion
    {
        public ChangTypes Type => ChangTypes.MatchWords;

        public HashSet<string> MatchWordsKeys;

        public HashSet<string> GetWordsKeys => new(MatchWordsKeys);
        public HashSet<string> GetSoundKeys => new(MatchWordsKeys);
        public HashSet<string> GetImageKeys => new();
        public HashSet<string> GetNeedDemonstrationKeys => new(MatchWordsKeys);
    }
}
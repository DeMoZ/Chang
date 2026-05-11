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
    
    public class QuestSelectWord : IQuestion
    {
        public ChangTypes Type => ChangTypes.MatchWords;

        public HashSet<string> MatchWordsKeys;

        public HashSet<string> GetWordsKeys => new(MatchWordsKeys);
        public HashSet<string> GetSoundKeys => new(MatchWordsKeys);
        public HashSet<string> GetImageKeys => new();
        public HashSet<string> GetNeedDemonstrationKeys => new(MatchWordsKeys);
    }

    public class SentenceSelectWords : IQuestion
    {
        public ChangTypes Type => ChangTypes.MatchWords;

        public HashSet<string> MatchWordsKeys;

        public HashSet<string> GetWordsKeys => new(MatchWordsKeys);
        public HashSet<string> GetSoundKeys => new(MatchWordsKeys);
        public HashSet<string> GetImageKeys => new();
        public HashSet<string> GetNeedDemonstrationKeys => new(MatchWordsKeys);
        public string LocalizationKey { get; set; }
        public string DefaultTranslation { get; set; }
        public string ImageFileName { get; set; }
        public List<string> CompareWordsFileNames { get; set; }
        public List<string> DisplayWordsFileNames { get; set; }
        public List<string> MixWordsFileNames { get; set; }
        public string LogKey { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Chang.Core
{
    public interface IQuestion
    {
        ChangTypes Type { get; }
        HashSet<string> GetWordsKeys { get; }
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
        public ChangTypes Type => ChangTypes.SelectWord;

        public string Key;
        public HashSet<string> WordsKeys;
        public string SectionKey;
        // public Languages Language;

        public HashSet<string> GetWordsKeys => new(WordsKeys) { Key };
        public HashSet<string> GetSoundKeys => new(WordsKeys) { Key };
        public HashSet<string> GetImageKeys => new() { Key };
        public HashSet<string> GetNeedDemonstrationKeys => new(WordsKeys) { Key };
    }

    public class SentenceSelectWords : IQuestion
    {
        public ChangTypes Type => ChangTypes.SentenceSelectWords;

        public string Key { get; set; }

        // the actual sentence words after the Replaceable and Gender words are resolved on the pages start
        public HashSet<string> GetNeedDemonstrationKeys => new(CompareWordsKeys);

        public string Translation {get; private set;}

        public Sentence Sentence { get; set; } // runtime field

        public List<string> CompareWordsKeys { get; set; }
        public List<string> DisplayWordsKeys { get; set; }
        public List<string> MixWordsKeys { get; set; }

        // sentence words the player has to pick from the mix (compare words under the display placeholders),
        // extra mix words are not included
        public IEnumerable<string> SelectWordsKeys => CompareWordsKeys
            .Where((key, i) => i < DisplayWordsKeys.Count && string.IsNullOrEmpty(DisplayWordsKeys[i]));

        private HashSet<string> _wordsKeys;

        public HashSet<string> GetWordsKeys => _wordsKeys ??= CompareWordsKeys.Concat(MixWordsKeys).ToHashSet();
        public HashSet<string> GetImageKeys => new() { Sentence.ImageKey };
        // the sentence is voiced word by word
        public HashSet<string> GetSoundKeys => new(GetWordsKeys);

        public void SetTranslation(string translation)
        {
            Translation = translation;
        }
    }
}
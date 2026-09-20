using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

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
        public string SectionKey; // todo chang do i need it?
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

        public HashSet<string> MatchWordsKeys;

        public HashSet<string> GetNeedDemonstrationKeys => new(MatchWordsKeys);

        public string Translation {get; private set;}

        public Sentence Sentence { get; set; } // runtime field

        public List<string> CompareWordsKeys { get; set; }
        public List<string> DisplayWordsKeys { get; set; }
        public List<string> MixWordsKeys { get; set; }

        private HashSet<string> _wordsKeys;
        private HashSet<string> _soundKeys;
        private HashSet<string> _imageKeys;

        public HashSet<string> GetWordsKeys => _wordsKeys ??= CompareWordsKeys.ToHashSet();
        public HashSet<string> GetImageKeys => _imageKeys ?? throw new System.Exception("_imageKeys not set");
        public HashSet<string> GetSoundKeys => _soundKeys ?? throw new System.Exception("_soundKeys not set");

        public void SetTranslation(string translation)
        {
            Translation = translation;
        }

        public void SetImageKeys(IEnumerable<string> imageKeys)
        {
            _imageKeys = new HashSet<string> { Sentence.ImageKey };
            _imageKeys.AddRange(imageKeys);
        }

        public void SetSoundKeys(IEnumerable<string> soundKeys)
        {
            _soundKeys = soundKeys.ToHashSet();
        }
    }
}
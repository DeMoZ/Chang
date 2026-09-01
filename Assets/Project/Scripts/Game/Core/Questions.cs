using System;
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

// -> old
        public HashSet<string> MatchWordsKeys;

        public HashSet<string> GetNeedDemonstrationKeys => new(MatchWordsKeys);

        // public string LocalizationKey => Sentence.Key;
        public string DefaultTranslation => Sentence.DefaultTranslation;
        // public string ImageKey => Sentence.ImageKey;
        // public string SoundKey => Sentence.SoundKey;
        //
// <- old
        public List<string> CompareWordsKeys
        {
            get
            {
                throw new NotImplementedException("сначала надо инициализировать Sentence, потом получать ключи не из того что есть в книге, а из инициализированного Sentence");
                return Sentence.SentenceWords.Select(word => word.WordKey).ToList();
            }
        }

        public List<string> DisplayWordsKeys {
            get
            {
                throw new NotImplementedException("сначала надо инициализировать Sentence, потом получать ключи не из того что есть в книге, а из инициализированного Sentence");
                return Sentence.SentenceWords.Select(word => word.WordKey).ToList();
            }
        } // todo chang Not all words from 
        public List<string> MixWordsKeys {
            get
            {
                throw new NotImplementedException("сначала надо инициализировать Sentence, потом получать ключи не из того что есть в книге, а из инициализированного Sentence");
                return Sentence.SentenceWords.Select(word => word.WordKey).ToList();
            }
        } // todo chang missed words and something else


        public Sentence Sentence { get; set; } // runtime field

        public HashSet<string> GetWordsKeys => _wordsKeys ??= new HashSet<string> { Sentence.SentenceKey };
        public HashSet<string> GetSoundKeys => _soundKeys ??= new HashSet<string> { Sentence.SoundKey }; // todo chang incorrect. WHen i use variants for word, the sound will be changed
        public HashSet<string> GetImageKeys => _imageKeys ??= new HashSet<string> { Sentence.ImageKey };


        private HashSet<string> _wordsKeys;
        private HashSet<string> _soundKeys;
        private HashSet<string> _imageKeys;
    }
}
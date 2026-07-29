using Chang.Core;

namespace Chang
{
    public class PhraseData
    {
        public readonly string Key;
        public readonly Languages Language;
        public readonly Word Word;

        public bool ShowPhonetics { get; protected set; }
        public string LogKey => string.Empty;// $"{Language}/{Word.LogKey}";

        public PhraseData(string key, Word word, Languages language)
        {
            Key = key;
            Word = word;
            Language = language;
        }

        public void SetPhonetics(bool showPhonetics)
        {
            ShowPhonetics = showPhonetics;
        }

        public override string ToString()
        {
            return $"key: {Key}; language: {Language}; word: {Word.LearnWord}; phonetic: {ShowPhonetics}; logKey: {LogKey}"; //  audioclip: {AudioClip?.name}; sprite: {Sprite?.name}
        }
    }

    public class SequencePhraseData : PhraseData
    {
        public bool IsPlaceHolder { get; private set; }
        public bool IsHighlighted { get; private set; }
        public bool IsInteractable { get; private set; }

        public SequencePhraseData(string key, Word word, Languages language) : base(key, word, language)
        {
        }

        public SequencePhraseData(PhraseData data) : base(data.Key, data.Word, data.Language)
        {
             ShowPhonetics = data.ShowPhonetics;
        }
        
        public void SetIsPlaceHolder(bool value)
        {
            IsPlaceHolder = value;
        }

        public void SetHighlighted(bool value)
        {
            IsHighlighted = value;
        }

        public void SetInteractable(bool value)
        {
            IsInteractable = value;
        }
    }
}
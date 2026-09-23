using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Popup;
using Project.Services.PagesContentProvider;
using UnityEngine;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class SentenceSelectWordStateResult : IQuestionResult
    {
        public string Key { get; }
        public string Presentation { get; }
        public ChangTypes Type => ChangTypes.SentenceSelectWords;
        public bool IsCorrect { get; }
        public bool IsHintUsed { get; }
        public object[] Info { get; }

        public SentenceSelectWordStateResult(string key, string presentation, bool isCorrect, bool isHintUsed,
            params object[] info)
        {
            Key = key;
            Presentation = presentation;
            IsCorrect = isCorrect;
            IsHintUsed = isHintUsed;
            Info = info;
        }
    }
    
    public class QuestSentenceSelectWordData
    {
        public List<SequencePhraseData> CompareSequence { get; set; }
        public List<SequencePhraseData> DisplaySequence { get; set; }
        public List<SequencePhraseData> MixWords { get; set; }
    }

    public class SentenceSelectWordState : ResultStateBase<ChangTypes, PagesBus>
    {
        [Inject] private readonly SentenceSelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly PopupManager _popupManager;

        private readonly IPagesContentProvider _pagesContentProvider;

        private CancellationTokenSource _cts;
        private CancellationTokenSource _soundCts; // play sentence sounds async
        private SentenceSelectWordStateResult _stateResult;
        private QuestSentenceSelectWordData _questionData;
        private SentenceSelectWords _sentenceQuestion;

        public override ChangTypes Type => ChangTypes.SentenceSelectWords;

        public SentenceSelectWordState(PagesBus bus, IPagesContentProvider pagesContentProvider,
            Action<ChangTypes> onStateResult) : base(bus, onStateResult)
        {
            _pagesContentProvider = pagesContentProvider;
        }

        public override void Enter()
        {
            base.Enter();

            Bus.OnHintUsed.Subscribe(OnHint);
            _cts = new CancellationTokenSource();
            StateBodyAsync(_cts.Token).Forget();
        }

        public override void Exit()
        {
            base.Exit();

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _soundCts?.Cancel();
            _soundCts?.Dispose();
            _soundCts = null;

            Bus.OnHintUsed.Unsubscribe(OnHint);
            _stateController.SetViewActive(false);
            _pagesContentProvider.ClearCache();
            _stateController.Clear();
            _stateResult = null;
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            IQuestion question = Bus.Lesson.CurrentQuestion;
            _sentenceQuestion = question as SentenceSelectWords;

            if (_sentenceQuestion == null)
            {
                throw new Exception(
                    "SentenceSelectWords is not a SentenceSelectWords"); // todo chang implement exit state
            }

            _questionData = GetQuestionData(_sentenceQuestion);
            bool isQuestInTranslation = false; // todo chang
            
            if (!TryGetLocalization(_sentenceQuestion.Key, out string translation))
            {
                translation = _sentenceQuestion.Translation;
            }

            string spritePath = _wordPathHelper.GetTexturePath(_sentenceQuestion.GetImageKeys.First());
            Sprite sprite = _pagesContentProvider.GetCachedSprite(spritePath);
            
            _stateController.Init(
                isQuestInTranslation,
                _questionData.DisplaySequence,
                _questionData.MixWords,
                translation,
                sprite,
                OnToggleValueChanged,
                () =>
                {
                    OnClickPlaySound(!isQuestInTranslation);
                });

            _stateController.SetViewActive(true);

            OnClickPlaySound(!isQuestInTranslation);
            
            await UniTask.Yield(ct);
        }

        // todo chang implement localization
        private bool TryGetLocalization(string sentenceQuestionLocalizationKey, out string translation)
        {
            // try
            // {
            //     translation = localization.Get(sentenceQuestionLocalizationKey);
            // }
            // catch
            // {
            translation = string.Empty;
            return false;
            // }
            //
            // return true;
        }

        private QuestSentenceSelectWordData GetQuestionData(SentenceSelectWords sentenceQuestion)
        {
            var data = new QuestSentenceSelectWordData
            {
                CompareSequence = GetPhrasesDataList(sentenceQuestion.CompareWordsKeys),
                DisplaySequence = GetPhrasesDataList(sentenceQuestion.DisplayWordsKeys),
                MixWords = GetPhrasesDataList(sentenceQuestion.MixWordsKeys)
            };

            data.MixWords.Shuffle();
            data.MixWords.ForEach(pData => pData.SetInteractable(true));

            return data;

            List<SequencePhraseData> GetPhrasesDataList(List<string> keys)
            {
                List<SequencePhraseData> phrasesDataList = new List<SequencePhraseData>();

                for (var i = 0; i < keys.Count; i++)
                {
                    string key = keys[i];

                    if (string.IsNullOrEmpty(key))
                    {
                        phrasesDataList.Add(CreatePlaceholder(sentenceQuestion, i));
                        continue;
                    }

                    if (Bus.Words.TryGetValue(key, out Word word))
                    {
                        phrasesDataList.Add(new SequencePhraseData(word));
                    }
                }

                return phrasesDataList;
            }
        }

        private SequencePhraseData CreatePlaceholder(SentenceSelectWords sentenceQuestion, int index)
        {
            // display keys are parallel to compare keys, so the replaced word has the same index
            string replacedWord = index < sentenceQuestion.CompareWordsKeys.Count
                                  && Bus.Words.TryGetValue(sentenceQuestion.CompareWordsKeys[index], out Word replaced)
                ? replaced.LearnWord
                : null;

            SequencePhraseData placeholderData = new SequencePhraseData(Word.CreateEmptyPlaceholder(replacedWord));
            placeholderData.SetIsPlaceHolder(true);
            placeholderData.SetInteractable(true);
            return placeholderData;
        }

        private void OnClickPlaySound(bool isLearnLanguage)
        {
            _soundCts?.Cancel();
            _soundCts?.Dispose();
            _soundCts = null;
            
            List<AudioClip> audioClips = new List<AudioClip>();
            
            _questionData.CompareSequence.ForEach(pData =>
            {
                string key = isLearnLanguage
                    ? pData.Word.WordKey
                    : _wordPathHelper.GetNativeSoundKey(pData.Word.WordKey, _profileService.ProfileData.NativeLanguage);

                string path = _wordPathHelper.GetSoundPath(key);
                AudioClip asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);

                if (asset)
                {
                    audioClips.Add(asset);
                }
            });
            
            _soundCts = new CancellationTokenSource();
            _pagesSoundController.PlaySoundsAsync(audioClips, _soundCts.Token).Forget();
        }

        private SentenceSelectWordStateResult GetResult()
        {
            string compare = string.Join("", _questionData.CompareSequence.Select(pData => pData.Word.LearnWord));
            string display = string.Join("", _questionData.DisplaySequence.Select(pData => pData.Word.LearnWord));
            bool isCorrect = string.Equals(compare, display);
            bool isHintUsed = Bus.OnHintUsed.Value;

            List<WordResult> inCorrectWords = new();

            for (int i = 0; i < _questionData.DisplaySequence.Count && i < _questionData.CompareSequence.Count; i++)
            {
                Word compareWord = _questionData.CompareSequence[i].Word;
                Word displayWord = _questionData.DisplaySequence[i].Word;

                // compare by spelling, a different word with the same spelling is not a mistake
                if (!string.Equals(compareWord.LearnWord, displayWord.LearnWord))
                {
                    inCorrectWords.Add(new WordResult(compareWord, false, isHintUsed));
                    inCorrectWords.Add(new WordResult(displayWord, false, isHintUsed));
                }
            }

            object[] info = { compare, isHintUsed, inCorrectWords };

            return new SentenceSelectWordStateResult(_sentenceQuestion.Key, display, isCorrect, isHintUsed, info);
        }

        private void OnHint(bool isHintUsed)
        {
            _stateController.ShowHint();
        }

        private void OnToggleValueChanged(int displayIndex, int mixIndex)
        {
            Debug.Log($"displayIndex: {displayIndex}; mixIndex: {mixIndex}");

           
            if (displayIndex > -1) // display word clicked
            {
                if (!_questionData.DisplaySequence[displayIndex].IsPlaceHolder)
                {
                    _questionData.MixWords.Add(_questionData.DisplaySequence[displayIndex]);
                    _questionData.DisplaySequence[displayIndex] = CreatePlaceholder(_sentenceQuestion, displayIndex);
                    _stateController.UpdateDisplaySequence(_questionData.DisplaySequence);
                    _stateController.UpdateMixSequence(_questionData.MixWords);
                }
                else
                {
                    _questionData.DisplaySequence[displayIndex].SetHighlighted(!_questionData.DisplaySequence[displayIndex].IsHighlighted);
                }
            }
            
            SequencePhraseData placeToMove;

            if (mixIndex > -1) // mix word checked
            {
                placeToMove = _questionData.DisplaySequence.FirstOrDefault(pData => pData.IsHighlighted);

                if (placeToMove == null)
                {
                    placeToMove = _questionData.DisplaySequence.FirstOrDefault(pData => pData.IsPlaceHolder);
                }

                if (placeToMove == null)
                {
                    Debug.LogWarning("No place to move the word to.");
                    return;
                }

                int index = _questionData.DisplaySequence.IndexOf(placeToMove);
                _questionData.DisplaySequence[index] = _questionData.MixWords[mixIndex];
                _questionData.MixWords.RemoveAt(mixIndex);
                _stateController.UpdateDisplaySequence(_questionData.DisplaySequence);
                _stateController.UpdateMixSequence(_questionData.MixWords);
            }
           
            placeToMove = _questionData.DisplaySequence.FirstOrDefault(pData => pData.IsPlaceHolder);

            foreach (var pData in _questionData.MixWords)
            {
                pData.SetInteractable(placeToMove != null);
            }

            _stateController.UpdateMixSequence(_questionData.MixWords);
            _gameOverlayController.EnableCheckButton(placeToMove == null);

            if (placeToMove == null) // no more placeholders
            {
                Bus.QuestionResult = GetResult();
            }
        }
    }
}
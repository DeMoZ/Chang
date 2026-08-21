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
        public Queue<SequencePhraseData> PlaceHolderPool { get; set; }
    }

    public class SentenceSelectWordState : ResultStateBase<ChangTypes, PagesBus>
    {
        [Inject] private readonly SentenceSelectWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
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

            _soundCts?.Cancel();
            _soundCts?.Dispose();
            
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
                translation = _sentenceQuestion.DefaultTranslation;
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

            data.DisplaySequence.Where(pData => pData.IsPlaceHolder).ToList()
                .ForEach(pData => pData.SetInteractable(true));
            data.MixWords.ForEach(pData => pData.SetInteractable(true));

            return data;

            List<SequencePhraseData> GetPhrasesDataList(List<string> keys)
            {
                List<SequencePhraseData> phrasesDataList = new List<SequencePhraseData>();

                foreach (var key in keys)
                {
                    if (Bus.Words.TryGetValue(key, out Word word))
                    {
                        SequencePhraseData phraseData = new SequencePhraseData(word);
                        phraseData.SetIsPlaceHolder(string.IsNullOrEmpty(key));
                        phrasesDataList.Add(phraseData);
                    }
                }

                return phrasesDataList;
            }
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
                    _questionData.DisplaySequence[displayIndex] = _questionData.PlaceHolderPool.Dequeue();
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
                _questionData.PlaceHolderPool.Enqueue(_questionData.DisplaySequence[index]);
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

            throw new NotImplementedException();
/*
            if (placeToMove == null) // no more placeholders
            {
                string compare = string.Join("", _questionData.CompareSequence.Select(pData => pData.Word.LearnWord));
                string display = string.Join("", _questionData.DisplaySequence.Select(pData => pData.Word.LearnWord));
                bool isCorrect = string.Equals(compare, display);

                List<SelectWordResult> inCorrectWords = new();

                for (int i = 0; i > _questionData.DisplaySequence.Count; i++)
                {
                    W/o/r/d/D/a/t/a compareWord = _questionData.CompareSequence[i].Word; // WordData старый формат. Теперь новый WordData заменил PhraseData и для старого нужно придумать что то еще
                    W/o/r/d/D/a/t/a displayWord = _questionData.DisplaySequence[i].Word; // WordData старый формат. Теперь новый WordData заменил PhraseData и для старого нужно придумать что то еще

                    if (!string.Equals(compareWord.LogKey, displayWord.LogKey))
                    {
                        inCorrectWords.Add(new SelectWordResult(compareWord.LogKey, compareWord.LearnWord, false));
                        inCorrectWords.Add(new SelectWordResult(displayWord.LogKey, displayWord.LearnWord, false));
                    }
                }

                object[] info = { compare, Bus.OnHintUsed.Value, inCorrectWords };

                var result = new SentenceSelectWordStateResult(
                    _sentenceQuestion.LogKey,
                    display,
                    isCorrect,
                    info);

                Bus.QuestionResult = result;
            }
            */
        }
    }
}
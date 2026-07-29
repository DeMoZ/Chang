using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Resources;
using Chang.Sentences;
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

        public SentenceSelectWordStateResult(string key, string presentation, bool isCorrect, bool isHintUsed, params object[] info)
        {
            Key = key;
            Presentation = presentation;
            IsCorrect = isCorrect;
            IsHintUsed = isHintUsed;
            Info = info;
        }
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
        private SentenceSelectWordStateResult _stateResult;
        // private QuestSentenceSelectWordData _questionData;
        private SentenceSelectWords _sentenceQuestion;

        public override ChangTypes Type => ChangTypes.SentenceSelectWords;

        public SentenceSelectWordState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<ChangTypes> onStateResult) : base(bus, onStateResult)
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

            Bus.OnHintUsed.Unsubscribe(OnHint);
            _stateController.SetViewActive(false);
            _pagesContentProvider.ClearCache();
            _stateController.Clear();
            // _questionData.Dispose();
            _stateResult = null;
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            IQuestion question = Bus.Lesson.CurrentQuestion;
            _sentenceQuestion = question as SentenceSelectWords;

            if (_sentenceQuestion == null)
            {
                throw new Exception("SentenceSelectWords is not a SentenceSelectWords"); // todo chang implement exit state
                return;
            }

            throw new NotImplementedException();
            
            /*
            await _pagesContentProvider.GetContentAsync(question, ct);
            await _pagesContentProvider.CacheContentAsync(AssetPaths.Addressables.EmptyWordPlaceHolderPath, ct);

            _questionData = GetQuestionData(_sentenceQuestion);
            bool isQuestInTranslation = false; // todo chang

            if (!TryGetLocalization(_sentenceQuestion.LocalizationKey, out string translation))
            {
                translation = _sentenceQuestion.DefaultTranslation;
            }
            
            string spritePath = _wordPathHelper.GetTexturePath(_sentenceQuestion.ImageFileName);
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
                    /*OnClickPlaySound(!isQuestInTranslation)*/
            /*    });

            _stateController.SetViewActive(true);
            */
            // OnClickPlaySound(!isQuestInTranslation);
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
/*
        private QuestSentenceSelectWordData GetQuestionData(SentenceSelectWords sentenceQuestion)
        {
            var data = new QuestSentenceSelectWordData
            {
                CompareSequence = GetPhrasesDataList(sentenceQuestion.CompareWordsFileNames),
                DisplaySequence = GetPhrasesDataList(sentenceQuestion.DisplayWordsFileNames),
                MixWords = GetPhrasesDataList(sentenceQuestion.MixWordsFileNames)
            };

            data.DisplaySequence.Where(pData => pData.IsPlaceHolder).ToList().ForEach(pData => pData.SetInteractable(true));
            data.MixWords.ForEach(pData => pData.SetInteractable(true));

            return data;

            List<SequencePhraseData> GetPhrasesDataList(List<string> fileNames)
            {
                List<SequencePhraseData> phrasesDataList = new List<SequencePhraseData>();

                foreach (var fileName in fileNames)
                {
                    string path = string.Empty;

                    // path = string.IsNullOrEmpty(fileName)
                    //     ? AssetPaths.Addressables.EmptyWordPlaceHolderPath
                    //     : _wordPathHelper.GetConfigPath(fileName);

                    PhraseConfig asset = _pagesContentProvider.GetCachedAsset<PhraseConfig>(path);

                    if (asset)
                    {
                        SequencePhraseData phraseData = new SequencePhraseData(asset.PhraseData);
                        phraseData.SetIsPlaceHolder(string.IsNullOrEmpty(fileName));
                        phrasesDataList.Add(phraseData);
                    }
                    
                }

                return phrasesDataList;
            }
        }
*/
        private void OnClickPlaySound(bool isLearnLanguage)
        {
            throw new NotImplementedException();
            // string key =  isLearnLanguage
            //     ? _correctWord.LogKey
            //     : _wordPathHelper.GetNativeSoundKey(_correctWord.LogKey, _profileService.ProfileData.NativeLanguage);
            //
            // string path = _wordPathHelper.GetSoundPath(key);
            // AudioClip asset = _pagesContentProvider.GetCachedAsset<AudioClip>(path);
            //
            // if (asset)
            // {
            //     _pagesSoundController.PlaySound(asset);
            // }
        }

        private void OnHint(bool isHintUsed)
        {
            _stateController.ShowHint();
        }

        private void OnToggleValueChanged(int displayIndex, int mixIndex)
        {
            Debug.Log($"displayIndex: {displayIndex}; mixIndex: {mixIndex}");

            throw new NotImplementedException();
            /*
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

            if (placeToMove == null) // no more placeholders
            {
                string compare = string.Join("", _questionData.CompareSequence.Select(pData => pData.Word.LearnWord));
                string display = string.Join("", _questionData.DisplaySequence.Select(pData => pData.Word.LearnWord));
                bool isCorrect = string.Equals(compare, display);
                
                List<SelectWordResult> inCorrectWords = new();

                for (int i = 0; i > _questionData.DisplaySequence.Count; i++)
                {
                    WordData compareWord = _questionData.CompareSequence[i].Word;
                    WordData displayWord = _questionData.DisplaySequence[i].Word;

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
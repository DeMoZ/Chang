using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
    public class SentenceSelectWordResult : IQuestionResult
    {
        public string Key { get; }
        public string Presentation { get; }
        public QuestionType Type => QuestionType.SentenceSelectWords;
        public bool IsCorrect { get; }
        public object[] Info { get; }

        public SentenceSelectWordResult(string key, string presentation, bool isCorrect, params object[] info)
        {
            Key = key;
            Presentation = presentation;
            IsCorrect = isCorrect;
            Info = info;
        }
    }

    public class SentenceSelectWordState : ResultStateBase<QuestionType, PagesBus>
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
        private SentenceSelectWordResult _result;
        private QuestSentenceSelectWordData _questionData;
        private SentenceSelectWords _sentenceQuestion;

        public override QuestionType Type => QuestionType.SentenceSelectWords;

        public SentenceSelectWordState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<QuestionType> onStateResult) : base(bus, onStateResult)
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
            _questionData.Dispose();
            _result = null;
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            IQuestion question = Bus.CurrentLesson.CurrentQuestion;
            _sentenceQuestion = question as SentenceSelectWords;

            if (_sentenceQuestion == null)
            {
                throw new Exception("SentenceSelectWords is not a SentenceSelectWords"); // todo chang implement exit state
                return;
            }

            await _pagesContentProvider.GetContentAsync(question, ct);
            await _pagesContentProvider.CacheContentAsync(AssetPaths.Addressables.EmptyWordPlaceHolderPath, ct);

            _questionData = GetQuestionData(_sentenceQuestion);
            bool isQuestInTranslation = false; // todo chang
            string spritePath = _wordPathHelper.GetTexturePath(_sentenceQuestion.ImageFileName);
            Sprite sprite = _pagesContentProvider.GetCachedSprite(spritePath);

            _stateController.Init(
                isQuestInTranslation,
                _questionData.DisplaySequence,
                _questionData.MixWords,
                sprite,
                OnToggleValueChanged,
                () =>
                {
                    /*OnClickPlaySound(!isQuestInTranslation)*/
                });

            _stateController.SetViewActive(true);

            // OnClickPlaySound(!isQuestInTranslation);
        }

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

                    path = string.IsNullOrEmpty(fileName)
                        ? AssetPaths.Addressables.EmptyWordPlaceHolderPath
                        : _wordPathHelper.GetConfigPath(fileName);

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
                object[] info = { display, Bus.OnHintUsed.Value };

                string questKey = _sentenceQuestion.LogKey;
                
                var result = new SentenceSelectWordResult(
                    _sentenceQuestion.LogKey,
                    display,
                    isCorrect,
                    info);

                Bus.QuestionResult = result;
            }

            // _gameOverlayController.EnableCheckButton(isOn);
            // var isCorrect = _mixWords[index].Key == _correctWord.Key;
            // object[] info = { _correctWord.Word.LearnWord, Bus.OnHintUsed.Value };
            //
            // string path = Path.Combine(
            //     _profileService.ProfileData.LearnLanguage.ToString(),
            //     AssetPaths.Addressables.Words,
            //     _correctWord.Word.Section,
            //     _correctWord.Word.Key);
            //
            // var result = new SelectWordResult(
            //     _wordPathHelper.NormalizePath(path),
            //     _correctWord.Word.LearnWord, isCorrect, info);
            // Bus.QuestionResult = result;
        }
    }
}
using System;
using System.Collections.Generic;
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

        private List<WordConfig> _correctWordsConfig; // todo chang which to use
        private List<WordData> _correctWordsData; // todo chang which to use
        private CancellationTokenSource _cts;
        private List<PhraseData> correctSequence; // compare the result with this
        private List<PhraseData> sequence; // show question with this. It has some empty items to fill in with mixed words
        private SentenceSelectWordResult _result;
        private QuestSentenceSelectWordData _questionData;

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

            await _pagesContentProvider.GetContentAsync(question, ct);
            await _pagesContentProvider.CacheContentAsync(AssetPaths.Addressables.EmptyWordPlaceHolderPath, ct);

            _questionData = GetQuestionData(question);
            bool isQuestInTranslation = false; // todo chang
            string spritePath = _wordPathHelper.GetTexturePath(((SentenceSelectWords)question).ImageFileName);
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

        private QuestSentenceSelectWordData GetQuestionData(IQuestion question)
        {
            SentenceSelectWords sentenceSelectWords = (SentenceSelectWords)question;

            if (sentenceSelectWords == null)
            {
                Debug.LogError("SentenceSelectWords is null");
                return null;
            }

            var data = new QuestSentenceSelectWordData
            {
                CompareSequence = GetPhrasesDataList(sentenceSelectWords.CompareWordsFileNames),
                DisplaySequence = GetPhrasesDataList(sentenceSelectWords.DisplayWordsFileNames),
                MixWords = GetPhrasesDataList(sentenceSelectWords.MixWordsFileNames)
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

            if (mixIndex > -1) // mix word checked
            {
                SequencePhraseData placeToMove = _questionData.DisplaySequence.FirstOrDefault(h => h.IsHighlighted);

                if (placeToMove == null)
                {
                    placeToMove = _questionData.DisplaySequence.FirstOrDefault(h => h.IsPlaceHolder);
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
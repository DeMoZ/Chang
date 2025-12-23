using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Resources;
using Chang.Sentences;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Popup;
using Project.Services.PagesContentProvider;
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
            _result = null;
        }

        private async UniTask StateBodyAsync(CancellationToken ct)
        {
            IQuestion question = Bus.CurrentLesson.CurrentQuestion;

            await _pagesContentProvider.GetContentAsync(question, ct);

            QuestSentenceSelectWordData questionData = GetQuestionData((Sentences.SentenceSelectWords)question);
            string path = string.Empty;
            

            // _stateController.Init(isQuestInTranslation, _correctWord, sprite, _mixWords, OnToggleValueChanged, () => OnClickPlaySound(!isQuestInTranslation));
            _stateController.SetViewActive(true);

            // OnClickPlaySound(!isQuestInTranslation);
        }

        private QuestSentenceSelectWordData GetQuestionData(SentenceSelectWords sentence)
        {
            // string path = _wordPathHelper.GetConfigPath(sentence.DisplayWordsFileNames);
            // var asset = _pagesContentProvider.GetCachedAsset<PhraseConfig>(path);
            //
            return new QuestSentenceSelectWordData
            {
                // CorrectSequence = _pagesContentProvider.GetPhrasesDataList(sentence.CompareWordsFileNames),
                // SequenceWithHoles = _pagesContentProvider.GetPhrasesDataList(sentence.DisplayWordsFileNames),
                // MixWords = _pagesContentProvider.GetPhrasesDataList(sentence.MixWordsFileNames)
            };
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
    }
}
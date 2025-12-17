using System;
using System.Collections.Generic;
using Chang.Resources;
using Chang.Services;
using DMZ.FSM;
using Popup;
using Project.Services.PagesContentProvider;
using Zenject;

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

        private List<PhraseData> correctSequence; // compare the result with this
        private List<PhraseData> sequence;          // show question with this. It has some empty items to fill in with mixed words
        
        public SentenceSelectWordState(PagesBus bus, IPagesContentProvider pagesContentProvider, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
            _pagesContentProvider = pagesContentProvider;
        }
    }
}
using System;
using System.IO;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using Popup;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class DemonstrationWordResult : IQuestionResult
    {
        public string Key { get; }
        public string Presentation { get; }
        public QuestionType Type => QuestionType.DemonstrationWord;
        public bool IsCorrect => true;
        public object[] Info { get; }

        public DemonstrationWordResult(string key, string presentation, params object[] info)
        {
            Key = key;
            Presentation = presentation;
            Info = info;
        }
    }

    public class DemonstrationState : ResultStateBase<QuestionType, PagesBus>
    {
        [Inject] private readonly DemonstrationWordController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;
        [Inject] private readonly ProfileService _profileService;
        [Inject] private readonly PagesSoundController _pagesSoundController;
        [Inject] private readonly WordPathHelper _wordPathHelper;
        [Inject] private readonly IResourcesManager _assetManager;
        [Inject] private readonly PopupManager _popupManager;
        
        private PhraseData _correctWord;
        private PageService _pageService;

        public override QuestionType Type => QuestionType.DemonstrationWord;

        public DemonstrationState(PagesBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _pageService = new PageService(_wordPathHelper, _assetManager, _popupManager);
            _gameOverlayController.EnableHintButton(false);
            StateBodyAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();
            
            _pageService.Dispose();
            _stateController.SetViewActive(false);
        }

        private async UniTask StateBodyAsync()
        {
            ISimpleQuestion question = Bus.CurrentLesson.CurrentSimpleQuestion;
            
            await _pageService.LoadContentAsync(question);

            QuestDemonstrateWordData questionData = 
                new QuestDemonstrateWordData(_pageService.Configs[((SimpleQuestDemonstrationWord)question).CorrectWordFileName].Item.PhraseData);
            
            _correctWord = questionData.CorrectWord;

            _stateController.Init(_correctWord, OnToggleValueChanged, OnClickPlaySound);
            _stateController.SetViewActive(true);
        
            OnClickPlaySound();
        }
        
        private void OnClickPlaySound()
        {
            _pagesSoundController.PlaySound(_pageService.Sounds[_correctWord.Key].Item);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
            Debug.Log($"toggle isOn: {isOn}");
            object[] info = { _correctWord.Word.LearnWord, false };
            string path = Path.Combine(
                _profileService.ProfileData.LearnLanguage.ToString(),
                AssetPaths.Addressables.Words,
                _correctWord.Word.Section,
                _correctWord.Word.Key); 
            var result = new DemonstrationWordResult(
                _wordPathHelper.NormalizePath(path),
                _correctWord.Word.LearnWord,
                info);
            Bus.QuestionResult = result;
        }
    }
}
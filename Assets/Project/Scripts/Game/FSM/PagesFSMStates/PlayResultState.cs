using System;
using DMZ.FSM;
using System.Collections.Generic;
using Chang.Core;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class ResultItem
    {
        public string Key { get; }
        public bool IsSentence { get; }
        public string Presentation { get; }
        public string Translation { get; }
        public int Mark { get; }
        public bool IsCorrect { get; }

        public ResultItem(string key, bool isSentence, string presentation, string translation, int mark, bool isCorrect)
        {
            Key = key;
            IsSentence = isSentence;
            Presentation = presentation;
            Translation = translation;
            Mark = mark;
            IsCorrect = isCorrect;
        }
    }

    public class PlayResultState : ResultStateBase<ChangTypes, PagesBus>
    {
        [Inject] private readonly PlayResultController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private List<Word> _mixWords;
        private Word _correctWord;

        public override ChangTypes Type => ChangTypes.Result;

        public PlayResultState(PagesBus bus, Action<ChangTypes> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            StateBody();
        }

        public override void Exit()
        {
            base.Exit();
            _stateController.SetViewActive(false);
        }

        private void StateBody()
        {
            _stateController.Init(Bus.LessonLog, () => _gameOverlayController.OnContinue?.Invoke());
            _stateController.SetViewActive(true);
            _gameOverlayController.EnableReturnButton(false);
        }
    }
}
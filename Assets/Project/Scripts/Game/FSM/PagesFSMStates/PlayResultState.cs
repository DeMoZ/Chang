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
        public string Presentation { get; }
        public bool IsCorrect { get; }

        public ResultItem(string presentation, bool isCorrect)
        {
            Presentation = presentation;
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
            List<ResultItem> log = new();
            
            foreach (var result in Bus.LessonLog)
            {
                log.Add(new ResultItem(result.Presentation, result.IsCorrect));
            }
            
            _stateController.Init(log, () => _gameOverlayController.OnContinue?.Invoke());
            _stateController.SetViewActive(true);
            _gameOverlayController.EnableReturnButton(false);
        }
    }
}
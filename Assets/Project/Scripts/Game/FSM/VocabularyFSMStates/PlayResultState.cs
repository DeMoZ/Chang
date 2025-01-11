using System;
using DMZ.FSM;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.FSM
{
    public class ResultItem
    {
        public string Word { get; }
        public bool IsCorrect { get; }

        public ResultItem(string word, bool isCorrect)
        {
            Word = word;
            IsCorrect = isCorrect;
        }
    }

    public class PlayResultState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly PlayResultController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private List<PhraseData> _mixWords;
        private PhraseData _correctWord;

        public override QuestionType Type => QuestionType.SelectWord;

        public PlayResultState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
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
            var log = Bus.LessonLog.Select(r => new ResultItem(r.Word, r.IsCorrect)).ToList();
            _stateController.Init(log);
            _stateController.SetViewActive(true);
        }
    }
}
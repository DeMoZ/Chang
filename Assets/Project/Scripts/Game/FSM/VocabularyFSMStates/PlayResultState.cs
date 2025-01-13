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
        public string Key { get; }
        public bool IsCorrect { get; }

        public ResultItem(string key, bool isCorrect)
        {
            Key = key;
            IsCorrect = isCorrect;
        }
    }

    public class PlayResultState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly PlayResultController _stateController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private List<PhraseData> _mixWords;
        private PhraseData _correctWord;

        public override QuestionType Type => QuestionType.Result;

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
            List<ResultItem> log = new();
            
            foreach (var result in Bus.LessonLog)
            {
                log.Add(new ResultItem(result.Key, result.IsCorrect));
            }
            
            _stateController.Init(log, () => _gameOverlayController.OnContinue?.Invoke());
            _stateController.SetViewActive(true);
            _gameOverlayController.EnableReturnButton(false);
        }
    }
}
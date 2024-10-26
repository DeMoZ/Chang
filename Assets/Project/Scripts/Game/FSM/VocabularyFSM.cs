using System;
using System.Collections.Generic;
using DMZ.FSM;

namespace Chang.FSM
{
    public class VocabularyFSM : FSMResultBase<QuestionType>
    {
        private readonly VocabularyBus _vocabularyBus;

        protected override QuestionType _defaultStateType => QuestionType.None;
        
        public VocabularyFSM(VocabularyBus vocabularyBus, Action<StateType> stateChangedCallback = null)
        {
            _vocabularyBus = vocabularyBus;
        }

        public new void Dispose()
        {
            _currentState?.Value.Exit();
            _currentState?.Dispose();

            base.Dispose();
        }

        public void Initialize()
        {
            Init();
        }

        protected override void Init()
        {
            _states = new Dictionary<QuestionType, IResultState<QuestionType>>
            {
                // { QuestionType.DemonstrationWord, new State( _vocabularyBus, OnStateResult) },
                 { QuestionType.SelectWord, new SelectWordState( _vocabularyBus, OnStateResult) },
                // { QuestionType.MatchWords, new State( _vocabularyBus, OnStateResult) },
                // { QuestionType.DemonstrationDialogue, new State( _vocabularyBus, OnStateResult) },
            };

            _currentState.Subscribe(s => OnStateChanged(s.Type));
        }

        public void SwitchState(QuestionType newStateType)
        {
            _currentState.Value?.Exit();
            _currentState.Value = _states[newStateType];
            _currentState.Value.Enter();
        }
    }
}
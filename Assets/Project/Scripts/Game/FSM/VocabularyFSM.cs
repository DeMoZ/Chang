using System;
using System.Collections.Generic;
using DMZ.FSM;
using Zenject;

namespace Chang.FSM
{
    public class VocabularyFSM : FSMResultBase<QuestionType>
    {
        private readonly VocabularyBus _vocabularyBus;
        private readonly DiContainer _diContainer;

        public QuestionType CurrentStateType => _currentState?.Value?.Type ?? QuestionType.None;
        
        protected override QuestionType _defaultStateType => QuestionType.None;

        public VocabularyFSM(DiContainer diContainer, VocabularyBus vocabularyBus, Action<StateType> stateChangedCallback = null)
        {
            _diContainer = diContainer;
            _vocabularyBus = vocabularyBus;
        }

        public new void Dispose()
        {
            _currentState?.Value?.Exit();
            _currentState?.Dispose();

            base.Dispose();
        }

        public void Initialize()
        {
            Init();
        }

        protected override void Init()
        {
            var playResultState = new PlayResultState(_vocabularyBus, OnStateResult);
            var demonstrationWordState = new DemonstrationState(_vocabularyBus, OnStateResult);
            var selectWordState = new SelectWordState(_vocabularyBus, OnStateResult);
            var matchWordsState = new MatchWordsState( _vocabularyBus, OnStateResult) ;

            _diContainer.Inject(playResultState);
            _diContainer.Inject(demonstrationWordState);
            _diContainer.Inject(selectWordState);
            _diContainer.Inject(matchWordsState);
            
            _states = new Dictionary<QuestionType, IResultState<QuestionType>>
            {
                { QuestionType.Result, playResultState },
                { QuestionType.DemonstrationWord, demonstrationWordState },
                { QuestionType.SelectWord, selectWordState },
                { QuestionType.MatchWords, matchWordsState },
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
using System;
using System.Collections.Generic;
using DMZ.FSM;
using Zenject;

namespace Chang.FSM
{
    public class PagesFSM : FSMResultBase<QuestionType>
    {
        private readonly PagesBus _pagesBus;
        private readonly DiContainer _diContainer;
        private readonly PagesSoundController _pagesSoundController;

        public QuestionType CurrentStateType => _currentState?.Value?.Type ?? QuestionType.None;

        protected override QuestionType _defaultStateType => QuestionType.None;

        public PagesFSM(DiContainer diContainer, PagesBus pagesBus, Action<StateType> stateChangedCallback = null)
        {
            _diContainer = diContainer;
            _pagesBus = pagesBus;
            _pagesSoundController = _diContainer.Resolve<PagesSoundController>();
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
            var playResultState = new PlayResultState(_pagesBus, OnStateResult);
            var demonstrationWordState = new DemonstrationState(_pagesBus, OnStateResult);
            var selectWordState = new SelectWordState(_pagesBus, OnStateResult);
            var matchWordsState = new MatchWordsState(_pagesBus, OnStateResult);

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
            _pagesSoundController.UnregisterListeners();
            _currentState.Value?.Exit();
            _currentState.Value = _states[newStateType];
            _currentState.Value.Enter();
        }
    }
}
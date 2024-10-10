using System;
using System.Collections.Generic;
using Chang.Resources;
using DMZ.FSM;

namespace Chang.FSM
{
    public class VocabularyFSM : FSMResultBase<VocabularySubStates>
    {
        private readonly VocabularyBus _vocabularyBus;
        private readonly IResourcesManager _resourcesManager;
        private StateType _defaultState => StateType.Preload;

        public VocabularyFSM(VocabularyBus vocabularyBus, IResourcesManager resourcesManager, Action<StateType> stateChangedCallback = null)
        {
            _vocabularyBus = vocabularyBus;
            _resourcesManager = resourcesManager;
        }

        public void Initialize()
        {
            Init();
        }

        protected override void Init()
        {
            //_vocabularyBus.PreloadFor = PreloadType.Boot;
            
            _states = new Dictionary<VocabularySubStates, IResultState<VocabularySubStates>>
            {
                //{ VocabularySubStates.PlayLogic, new PreloadState( _gameBus, OnStateResult, _resourcesManager) },
                // { VocabularySubStates.DemonstrationWord, new LobbyState( _gameBus, OnStateResult) },
                // { VocabularySubStates.SelectWord, new VocabularyState( _gameBus, OnStateResult) },
                // { VocabularySubStates.MatchWords, new VocabularyState( _gameBus, OnStateResult) },
                // { VocabularySubStates.DemonstrationDialogue, new VocabularyState( _gameBus, OnStateResult) },
            };

            _currentState.Subscribe(s => OnStateChanged(s.Type));
            //_currentState.Value = _states[_defaultState];
            _currentState.Value.Enter();
        }
    }
}
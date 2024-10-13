using System;
using System.Collections.Generic;
using Chang.Resources;
using DMZ.FSM;

namespace Chang.FSM
{
    public class VocabularyState : ResultStateBase<StateType, GameBus>, IDisposable
    {
        public override StateType Type => StateType.PlayVocabulary;

        private VocabularyBus _vocabularyBus;
        private readonly IResourcesManager _resourcesManager;
        private VocabularyFSM _vocabularyFSM;

        public VocabularyState(GameBus gameBus, Action<StateType> onStateResult, IResourcesManager resourcesManager) : base(gameBus, onStateResult)
        {
            _resourcesManager = resourcesManager;
        }

        public void Dispose()
        {
            _vocabularyFSM.Dispose();
        }

        public override void Enter()
        {
            base.Enter();

            Bus.ScreenManager.SetActivePagesContainer(true);

            _vocabularyBus = new VocabularyBus();
            _vocabularyBus.ScreenManager = Bus.ScreenManager;
            _vocabularyBus.Questions = new Queue<Question>(Bus.ClickedLessonConfig.Questions);
            _vocabularyBus.ResourcesManager = _resourcesManager;

            _vocabularyFSM = new VocabularyFSM(_vocabularyBus);
            _vocabularyFSM.Initialize();
        }

        public override void Exit()
        {
            base.Exit();
            
            Bus.ScreenManager.SetActivePagesContainer(false);
        }
    }
}
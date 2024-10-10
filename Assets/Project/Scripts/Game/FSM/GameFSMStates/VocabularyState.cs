using System;
using Cysharp.Threading.Tasks;

namespace Chang.FSM
{
    public class VocabularyState : ResultStateBase<StateType, GameBus>
    {
        public override StateType Type => StateType.PlayVocabulary;

        private bool _isLoading;

        public VocabularyState(GameBus gameBus, Action<StateType> onStateResult) : base(gameBus, onStateResult)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            // todo roman implement PlayFSC here with different vocabulary screen

            // var _vocabularyFSM = new VocabularyFSM(vocabularyBus, _screenManager, _resourcesManager);
            //_vocabularyFSM.Initialize();
            // var gameBookController = _screenManager.GetVocabularyController();
            // gameBookController.Init(GameBus.LessonNames, (index) => OnLessonClick(index).Forget());
            // gameBookController.SetViewActive(true);
        }

        private async UniTaskVoid OnSomeClick()
        {
            if (_isLoading)
                return;

            // _isLoading = true;
            // await UniTask.DelayFrame(1);
            //
            // OnStateResult.Invoke(StateType.Preload);
            _isLoading = false;
        }
    }
}
using System;
using System.Threading;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    // todo chang this class shoud be changed to move repetition logic into VocabularyController
    // espeshially for the button click handling
    public class VocabularyRepetitionController : IViewController
    {
        private const int ShowLogLimitAmount = 30;

        private readonly ProfileService _profileService;
        private readonly MainScreenBus _mainScreenBus;
        private readonly RepetitionView _view;
        private readonly RepetitionService _repetitionService;

        [Inject]
        public VocabularyRepetitionController(
            ProfileService profileService,
            MainScreenBus mainScreenBus,
            RepetitionView view,
            RepetitionService repetitionService)
        {
            _profileService = profileService;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _repetitionService = repetitionService;
        }

        public void Dispose()
        {
        }

        public void Init(Action onRepeatWordsClick, Action onRepeatSentencesClick, Action onRepeatMixedClick)
        {
            _view.Init(onRepeatWordsClick, onRepeatSentencesClick, onRepeatMixedClick);
        }

        public async UniTask SetAsync(CancellationToken ct)
        {
            await UniTask.Yield(ct);
            _view.Set(_repetitionService.GetVocabularyLogsByPriority(ShowLogLimitAmount));
            _view.SetInteractableRepeatButtons(
                _repetitionService.CanRepeatVocabulary(),
                _repetitionService.CanRepeatSentences(),
                _repetitionService.CanRepeatMixed());
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        // todo chang implement items interacitons - show popup with word in Thai, translation, mark, info from log - marks, list when played
        // private void OnItemClick(int index)
        // {
        //     Debug.Log($"Clicked on item {index}");
        //
        //     //_mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[index].FileName);
        // }
    }
}
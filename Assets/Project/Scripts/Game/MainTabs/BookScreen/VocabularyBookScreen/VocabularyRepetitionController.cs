using System;
using System.Threading;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class VocabularyRepetitionController : IViewController
    {
        private const int ShowLogLimitAmount = 30;

        private readonly ProfileService _profileService;
        private readonly MainScreenBus _mainScreenBus;
        private readonly RepetitionView _view;
        private readonly VocabularyRepetitionService _vocabularyRepetitionService;

        [Inject]
        public VocabularyRepetitionController(
            ProfileService profileService,
            MainScreenBus mainScreenBus,
            RepetitionView view,
            VocabularyRepetitionService vocabularyRepetitionService)
        {
            _profileService = profileService;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _vocabularyRepetitionService = vocabularyRepetitionService;
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _view.Init(_mainScreenBus.OnRepeatClicked);
        }

        public async UniTask SetAsync(CancellationToken ct)
        {
            var sortedList = await _vocabularyRepetitionService.GetGeneralRepetitionAsync(ShowLogLimitAmount, ct);
            _view.Set(sortedList);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        // private void OnItemClick(int index)
        // {
        //     Debug.Log($"Clicked on item {index}");
        //
        //     //_mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[index].FileName);
        // }
        //
        // public int GetLogCount()
        // {
        //     // return _repetitionService.GetProgress().Questions;
        //     throw new NotImplementedException("Not implemented count log");
        // }
    }
}
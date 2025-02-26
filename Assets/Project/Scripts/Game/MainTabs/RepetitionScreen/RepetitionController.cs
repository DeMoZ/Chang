using System;
using Chang.Services;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class RepetitionController : IViewController
    {
        private const int ShowLogLimitAmount = 30;

        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly RepetitionView _view;
        private readonly RepetitionService _repetitionService;

        [Inject]
        public RepetitionController(
            GameBus gameBus,
            MainScreenBus mainScreenBus,
            RepetitionView view,
            RepetitionService repetitionService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _repetitionService = repetitionService;
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _view.Init(_mainScreenBus.OnRepeatClicked);
        }

        public void Set()
        {
            var sortedList = _repetitionService.GetGeneralRepetition(_gameBus.CurrentLanguage, ShowLogLimitAmount);
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
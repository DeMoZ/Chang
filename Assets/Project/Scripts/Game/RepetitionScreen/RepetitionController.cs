using System;
using Chang.Services;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class RepetitionController : IViewController
    {
        private readonly GameBus _gameBus;
        private readonly MainScreenBus _mainScreenBus;
        private readonly RepetitionView _view;
        private readonly ProfileService _profileService;

        [Inject]
        public RepetitionController(GameBus gameBus, MainScreenBus mainScreenBus, RepetitionView view, ProfileService profileService)
        {
            _gameBus = gameBus;
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {

        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        internal void Set()
        {
            // todo roman
            // read player data and sort
            //var a = _gameBus.SimpleBookData.Lessons;
            // var a = _profileService.GetProfile().
        }

        private void OnItemClick(int index)
        {
            Debug.Log($"Clicked on item {index}");

            //_mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[index].FileName);
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
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
            _view.Init(_mainScreenBus.OnRepeatClicked);
        }

        public void Set()
        {
            // todo roman
            // read player data and sort
            var progressQuestions = _profileService.GetProgress().Questions;
            List<Profile.QuestLog> progressList = progressQuestions.Select(q => q.Value).Where(q => q.SuccesSequese < 10).ToList();

            // var sortedWords = words
            //     .OrderBy(word => word.Mark)
            //     .ThenBy(word => word.LastReviewed)
            //     .Take(10)
            //     .ToList();

            // todo roman what to count?
            // 1. Iteration
            // 2. Date
            // 3. Mark


            // todo roman should be sorted by the sequence and time  too
            var sortedList = progressList.OrderBy(w => w.Mark)
            //.ThenBy(w => w.UtcTime)
            //.Take(10)
            .ToList();

            _view.Set(sortedList);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        private void OnItemClick(int index)
        {
            Debug.Log($"Clicked on item {index}");

            //_mainScreenBus.OnGameBookLessonClicked?.Invoke(_lessons[index].FileName);
        }


    }
}
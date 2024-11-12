using System;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class MainUiController : IDisposable
    {
        private readonly MainUiView _view;
        private readonly GameBookController _gameBookController;
        private readonly RepetitionController _repetitionController;

        /// <summary>
        /// should return to this tab after play any other game state
        /// </summary>
        private MainTabType _currentTab = MainTabType.Lessons;

        public MainUiController(MainUiView view, GameBookController gameBookController, RepetitionController repetitionController)
        {
            _view = view;
            _gameBookController = gameBookController;
            _repetitionController = repetitionController;

            _view.Init(OnToggleSelected);
        }

        private void OnToggleSelected(bool isOn, MainTabType lessons)
        {
            if (isOn)
                Debug.Log($"Toggle selected: {lessons}");

            switch (lessons)
            {
                case MainTabType.Lessons:
                    // Handle lessons tab selected
                    break;
                case MainTabType.Repetition:
                    // Handle repetition tab selected
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lessons), lessons, null);
            }
        }

        public void Dispose()
        {

        }
    }
}
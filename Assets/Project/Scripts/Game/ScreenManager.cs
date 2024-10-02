using System;
using Zenject;

namespace Chang
{
    public class ScreenManager
    {
        public GameBookController _gameBookController;

        [Inject]
        public ScreenManager(GameBookController gameBookController)
        {
            _gameBookController = gameBookController;

            _gameBookController.SetViewActive(false);
        }

        public GameBookController GetGameBookController()
        {
            return _gameBookController;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class ScreenManager
    {
        private GameBookController _gameBookController;
        private PreloaderController _preloaderController;
        private GameOverlayController _gameOverlayController;

        private GameObject _pagesContainer;

        private List<IViewController> _vocabularyControllers = new();

        private DemonstrationWordController _demonstrationController;
        private MatchWordsController _matchTranslationController;
        private SelectWordController _selectTranslationController;


        [Inject]
        public ScreenManager(GameBookController gameBookController,
            PreloaderController preloaderController,
            GameOverlayController gameOverlayController,
            [Inject(Id = "PagesContainer")] GameObject pagesContainer,
            DemonstrationWordController demonstrationController,
            MatchWordsController matchTranslationController,
            SelectWordController selectTranslationController)
        {
            _gameBookController = gameBookController;
            _preloaderController = preloaderController;
            _gameOverlayController = gameOverlayController;
            _gameBookController.SetViewActive(false);
            _preloaderController.SetViewActive(false);
            // _gameOverlayController.SetViewActive(false); // todo roman implement logic for enabling overlay on play state

            _pagesContainer = pagesContainer;
            SetActivePagesContainer(false);

            _demonstrationController = demonstrationController;
            _matchTranslationController = matchTranslationController;
            _selectTranslationController = selectTranslationController;

            _vocabularyControllers.Add(_demonstrationController);
            _vocabularyControllers.Add(_matchTranslationController);
            _vocabularyControllers.Add(_selectTranslationController);

            foreach (var controller in _vocabularyControllers)
            {
                controller.SetViewActive(false);
            }
        }

        public GameBookController GameBookController => _gameBookController;
        public PreloaderController PreloaderController => _preloaderController;

        public DemonstrationWordController DemonstrationWordController => _demonstrationController;
        public MatchWordsController MatchWordsController => _matchTranslationController;
        public SelectWordController SelectWordController => _selectTranslationController;

        public void SetActivePagesContainer(bool active)
        {
            _pagesContainer.SetActive(active);
        }

        public void EnableCheckButton(bool isOn)
        {
            _gameOverlayController.EnableCheckButton(isOn);
        }
    }
}
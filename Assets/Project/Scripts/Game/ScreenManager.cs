using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class ScreenManager
    {
        private MainUiController _mainUiController;
        private PreloaderController _preloaderController;

        private GameObject _pagesContainer;

        private List<IViewController> _vocabularyControllers = new();

        [Inject]
        public ScreenManager(MainUiController mainUiController,
            PreloaderController preloaderController,
            [Inject(Id = "PagesContainer")] GameObject pagesContainer,
            DemonstrationWordController demonstrationController,
            MatchWordsController matchTranslationController,
            SelectWordController selectTranslationController)
        {
            _mainUiController = mainUiController;
            _preloaderController = preloaderController;
            _mainUiController.SetViewActive(false);
            _preloaderController.SetViewActive(false);

            _pagesContainer = pagesContainer;
            SetActivePagesContainer(false);

            _vocabularyControllers.Add(demonstrationController);
            _vocabularyControllers.Add(matchTranslationController);
            _vocabularyControllers.Add(selectTranslationController);

            foreach (var controller in _vocabularyControllers)
            {
                controller.SetViewActive(false);
            }
        }

        public void SetActivePagesContainer(bool active)
        {
            _pagesContainer.SetActive(active);
        }
    }
}
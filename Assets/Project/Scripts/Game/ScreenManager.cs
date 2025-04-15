using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class ScreenManager : IDisposable
    {
        private LobbyController _mainUiController;
        private GameObject _pagesContainer;
        private List<IViewController> _pageControllers = new();

        [Inject]
        public ScreenManager(LobbyController mainUiController,
            [Inject(Id = "PagesContainer")] GameObject pagesContainer,
            DemonstrationWordController demonstrationController,
            MatchWordsController matchTranslationController,
            SelectWordController selectTranslationController)
        {
            _mainUiController = mainUiController;
            _mainUiController.SetViewActive(false);

            _pagesContainer = pagesContainer;
            SetActivePagesContainer(false);

            _pageControllers.Add(demonstrationController);
            _pageControllers.Add(matchTranslationController);
            _pageControllers.Add(selectTranslationController);

            foreach (var controller in _pageControllers)
            {
                controller.SetViewActive(false);
            }
        }

        public void Dispose()
        {
            _pageControllers = null;
        }
        
        public void SetActivePagesContainer(bool active)
        {
            _pagesContainer.SetActive(active);
        }
    }
}
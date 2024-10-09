using Zenject;

namespace Chang
{
    public class ScreenManager
    {
        public GameBookController _gameBookController;
        private DemonstrationWordController _demonstationScreen;
        private MatchWordsController _matchTranslationScreen;
        private SelectWordController _selectTranslationScreen;
        private PreloaderController _preloaderController;

        [Inject]
        public ScreenManager(GameBookController gameBookController,
            DemonstrationWordController demonstationController,
            MatchWordsController matchTranslationController,
            SelectWordController selectTranslationController,
            PreloaderController preloaderController)
        {
            _gameBookController = gameBookController;
            _gameBookController.SetViewActive(false);

            _demonstationScreen = demonstationController;
            _matchTranslationScreen = matchTranslationController;
            _selectTranslationScreen = selectTranslationController;
            _preloaderController = preloaderController;

            _demonstationScreen.SetViewActive(false);
            _matchTranslationScreen.SetViewActive(false);
            _selectTranslationScreen.SetViewActive(false);
            _preloaderController.SetViewActive(false);
        }

        public GameBookController GetGameBookController()
        {
            return _gameBookController;
        }

        public PreloaderController GetPreloaderController()
        {
            return _preloaderController;
        }
    }
}
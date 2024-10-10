using Zenject;

namespace Chang
{
    public class ScreenManager
    {
        private GameBookController _gameBookController;
        private PreloaderController _preloaderController;

        private DemonstrationWordController _demonstrationController;
        private MatchWordsController _matchTranslationController;
        private SelectWordController _selectTranslationController;

        [Inject]
        public ScreenManager(GameBookController gameBookController,
            PreloaderController preloaderController,
            DemonstrationWordController demonstrationController,
            MatchWordsController matchTranslationController,
            SelectWordController selectTranslationController)
        {
            _gameBookController = gameBookController;
            _preloaderController = preloaderController;
            _gameBookController.SetViewActive(false);
            _preloaderController.SetViewActive(false);

            _demonstrationController = demonstrationController;
            _matchTranslationController = matchTranslationController;
            _selectTranslationController = selectTranslationController;
            _demonstrationController.SetViewActive(false);
            _matchTranslationController.SetViewActive(false);
            _selectTranslationController.SetViewActive(false);
        }

        public GameBookController GameBookController => _gameBookController;
        public PreloaderController PreloaderController => _preloaderController;
        public DemonstrationWordController GetDemonstrationController => _demonstrationController;
        public MatchWordsController GetMatchTranslationController => _matchTranslationController;
        public SelectWordController GetSelectTranslationController => _selectTranslationController;
    }
}
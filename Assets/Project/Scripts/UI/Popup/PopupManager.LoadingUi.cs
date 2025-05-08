using Chang;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Popup
{
    public partial class PopupManager
    {
        [SerializeField] private LoadingUiView loadingUiPrefab;

        private LoadingUiController _loadingUiController;

        public LoadingUiController ShowLoadingUi(LoadingUiModel model)
        {
            Debug.LogError($"ShowLoadingUi with model: {model.Elements}");
            if (_loadingUiController == null)
            {
                LoadingUiView view = Instantiate(loadingUiPrefab, transform);
                _loadingUiController = new LoadingUiController(view, model);
                _popupStack.Push(_loadingUiController);
            }
            else
            {
                Debug.LogError($"ShowLoadingUi exists with model: {_loadingUiController.Model.Elements}");
                _loadingUiController.Model.Dispose();
                _loadingUiController.Update(model);
            }

            return _loadingUiController;
        }
    }
}
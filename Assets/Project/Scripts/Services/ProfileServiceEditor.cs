#if UNITY_EDITOR
using Chang.Services.DataProvider;
using UnityEditor;
#endif

namespace Chang.Services
{
    public partial class ProfileService
    {
#if UNITY_EDITOR
        private const string AssetPath = "Assets/Project/EditorCheckSaveLoad.asset";

        private PrefsDataViewEditor _prefsDataView;
#endif

        /// <summary>
        /// Editor only, shows the data stored in PlayerPrefs in the asset for the visual control
        /// </summary>
        private void RefreshPrefsDataView()
        {
#if UNITY_EDITOR
            _prefsDataView ??= AssetDatabase.LoadAssetAtPath<PrefsDataViewEditor>(AssetPath);
            if (_prefsDataView == null)
            {
                return;
            }

            _prefsDataView.Refresh(LearnLanguage);

            EditorUtility.SetDirty(_prefsDataView);
            AssetDatabase.SaveAssetIfDirty(_prefsDataView);
#endif
        }
    }
}

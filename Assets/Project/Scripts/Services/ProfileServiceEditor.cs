using Chang.Services.DataProvider;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Chang.Services
{
    public partial class ProfileService
    {
        private const string AssetPath = "Assets/Project/EditorCheckSaveLoad.asset";
        
        private IDataProvider _scriptableObjectDataProvider;

        private IDataProvider ScriptableObjectDataProvider
        {
            get
            {
#if UNITY_EDITOR
                _scriptableObjectDataProvider ??= AssetDatabase.LoadAssetAtPath<ScriptableObjectDataProviderEditor>(AssetPath);
#endif
                return _scriptableObjectDataProvider;
            }
        }

        private async UniTask SaveIntoScriptableObject()
        {
            if (ScriptableObjectDataProvider == null)
                return;

            await ScriptableObjectDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData);
            await ScriptableObjectDataProvider.SaveProgressDataAsync(_playerProfile.ProgressData);
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(_scriptableObjectDataProvider as ScriptableObjectDataProviderEditor);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}
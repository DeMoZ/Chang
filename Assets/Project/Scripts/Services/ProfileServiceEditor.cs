using Chang.Services.SaveLoad;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Chang.Services
{
    public partial class ProfileService
    {
        private const string AssetPath = "Assets/Project/EditorCheckSaveLoad.asset";
        
        private ISaveLoad _scriptableObjectSaveLoad;

        private ISaveLoad ScriptableObjectSaveLoad
        {
            get
            {
#if UNITY_EDITOR
                _scriptableObjectSaveLoad ??= AssetDatabase.LoadAssetAtPath<ScriptableObjectSaveLoadEditor>(AssetPath);
#endif
                return _scriptableObjectSaveLoad;
            }
        }

        private async UniTask SaveIntoScriptableObject()
        {
            if (ScriptableObjectSaveLoad == null)
                return;

            await ScriptableObjectSaveLoad.SaveProfileDataAsync(_playerProfile.ProfileData);
            await ScriptableObjectSaveLoad.SaveProgressDataAsync(_playerProfile.ProgressData);
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(_scriptableObjectSaveLoad as ScriptableObjectSaveLoadEditor);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}
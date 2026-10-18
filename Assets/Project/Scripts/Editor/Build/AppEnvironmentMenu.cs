using Chang.Resources;
using UnityEditor;
using UnityEngine;

namespace Chang.Editor.Build
{
    /// <summary>
    /// Chang/Environment: the Unity Cloud environment (CCD content, accounts, Cloud Save) used in Play mode in the Editor.
    /// Saved per machine in EditorPrefs. See <see cref="AppEnvironment"/>.
    /// </summary>
    public static class AppEnvironmentMenu
    {
        private const string Root = "Chang/Environment/";
        private const string DevItem = Root + "Dev";
        private const string StagingItem = Root + "Staging";
        private const string ProductionItem = Root + "Production";

        [MenuItem(DevItem)]
        private static void SelectDev() => Select(AppEnvironment.Dev);

        [MenuItem(StagingItem)]
        private static void SelectStaging() => Select(AppEnvironment.Staging);

        [MenuItem(ProductionItem)]
        private static void SelectProduction() => Select(AppEnvironment.Production);

        [MenuItem(DevItem, true)]
        private static bool ValidateDev() => Validate(DevItem, AppEnvironment.Dev);

        [MenuItem(StagingItem, true)]
        private static bool ValidateStaging() => Validate(StagingItem, AppEnvironment.Staging);

        [MenuItem(ProductionItem, true)]
        private static bool ValidateProduction() => Validate(ProductionItem, AppEnvironment.Production);

        private static string Selected => EditorPrefs.GetString(AppEnvironment.EditorPrefsKey, AppEnvironment.DefaultEnvironment);

        private static void Select(string environment)
        {
            EditorPrefs.SetString(AppEnvironment.EditorPrefsKey, environment);
            Debug.Log($"Environment for Play mode: {environment}" +
                      (EditorApplication.isPlaying ? " (applies on the next Play)" : ""));
        }

        private static bool Validate(string item, string environment)
        {
            Menu.SetChecked(item, Selected == environment);
            return true;
        }
    }
}

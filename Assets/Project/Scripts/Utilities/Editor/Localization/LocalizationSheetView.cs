using UnityEditor;
using UnityEngine;

namespace Chang.Utilities.Localization
{
    /// <summary>
    /// Editor only view of a SimpleLocalization sheet csv (Resources/Localization/*.csv): keys hierarchy and translations.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationSheetView", menuName = "Chang/Utilities/Localization/Sheet View", order = 0)]
    public class LocalizationSheetView : ScriptableObject
    {
        [Tooltip("Sheet csv downloaded by SimpleLocalization")]
        public TextAsset Csv;

        [Tooltip("Splits keys into the hierarchy: Dot for UI keys (Lobby.Tab.Words), Slash for content keys (Thai/Vocabulary/Gender/Key)")]
        public KeySeparatorType KeySeparator = KeySeparatorType.Dot;

        public char KeySeparatorChar => KeySeparator == KeySeparatorType.Slash ? '/' : '.';

        [MenuItem("Chang/Utilities/Localization/Create Sheet View", false, 100)]
        private static void Create()
        {
            ProjectWindowUtil.CreateAsset(CreateInstance<LocalizationSheetView>(), "LocalizationSheetView.asset");
        }
    }

    public enum KeySeparatorType
    {
        Dot,
        Slash,
    }
}

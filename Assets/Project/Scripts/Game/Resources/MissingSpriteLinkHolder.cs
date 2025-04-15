using UnityEngine;

namespace Chang.Resources
{
    [CreateAssetMenu(fileName = AssetPaths.Resources.MissingSpriteLinkHolder,
        menuName = AssetPaths.Resources.CreateAssetMenuName + AssetPaths.Resources.MissingSpriteLinkHolder)]
    public class MissingSpriteLinkHolder : ScriptableObject
    {
        [SerializeField] private Sprite _missingSprite;

        public Sprite MissingSprite => _missingSprite;
    }
}
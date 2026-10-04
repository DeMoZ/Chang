using UnityEngine;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Links a GameObject to the Penpot shape it was generated from.
    /// The Penpot importer uses it to update prefabs in place on re-import,
    /// so hand-added components and children survive.
    /// </summary>
    [DisallowMultipleComponent]
    public class DesignNode : MonoBehaviour
    {
        [SerializeField] private string _id;

        public string Id
        {
            get => _id;
            set => _id = value;
        }
    }
}

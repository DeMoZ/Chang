using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Paints a <see cref="DesignSelection"/> after toggles that other code switches, also silently (SetIsOnWithoutNotify):
    /// the item of the toggle that is on looks selected.
    /// </summary>
    public class ToggleSelection : MonoBehaviour
    {
        [SerializeField] private DesignSelection _selection;
        [Tooltip("In the order of the selection items")]
        [SerializeField] private List<Toggle> _toggles = new();

        private void LateUpdate()
        {
            var index = _toggles.FindIndex(toggle => toggle != null && toggle.isOn);
            if (index >= 0 && index != _selection.Current)
            {
                _selection.Select(index);
            }
        }
    }
}

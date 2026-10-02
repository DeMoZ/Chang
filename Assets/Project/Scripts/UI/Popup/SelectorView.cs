using UnityEngine;

namespace Popup
{
    /// <summary>
    /// Highlights the selected option button
    /// </summary>
    public class SelectorView : MonoBehaviour
    {
        private ButtonView[] _buttons;

        public void Init(ButtonView[] buttons, int selectedIndex)
        {
            _buttons = buttons;
            Select(selectedIndex);
        }

        public void Select(int index)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].SetSelected(i == index);
            }
        }
    }
}

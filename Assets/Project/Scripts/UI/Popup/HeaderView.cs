using TMPro;
using UnityEngine;

namespace Popup
{
    public class HeaderView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public string Text
        {
            set => text.text = value;
        }
    }
}
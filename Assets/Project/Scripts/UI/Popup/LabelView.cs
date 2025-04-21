using TMPro;
using UnityEngine;

namespace Popup
{
    public class LabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public string Text
        {
            set => text.text = value;
        }
    }
}
using DMZ.Events;
using TMPro;
using UnityEngine;

namespace Popup
{
    public class LabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        
        public DMZState<string> Text { get; set; }
        
        public void Init()
        {
            Text.Subscribe(SetLabelText);
        }

        private void OnDestroy()
        {
            Text.Unsubscribe(SetLabelText);
        }

        private void SetLabelText(string txt)
        {
            Debug.Log($"Set label text: {txt}");
            text.text = txt;
        }
    }
}
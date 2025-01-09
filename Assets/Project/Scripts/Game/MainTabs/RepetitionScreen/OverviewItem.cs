using TMPro;
using UnityEngine;

namespace Chang
{
    public class OverviewItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private TMP_Text value;


        public void Set(string value)
        {
            this.value.text = value;
        }

        public void Set(string text, string value)
        {
            this.text.text = text;
            this.value.text = value;
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class GameBookSection : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button repeatSectionButton;
        
        public void Init(string labelText)
        {
            label.text = labelText;
        }
    }
}
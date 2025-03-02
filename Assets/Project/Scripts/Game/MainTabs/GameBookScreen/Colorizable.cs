using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class Colorizable : MonoBehaviour
    {
        [SerializeField] private Image[] baseColors;

        public void SetBaseColor(Color baseColor)
        {
            foreach (var image in baseColors)
            {
                image.color = baseColor;
            }
        }
    }
}
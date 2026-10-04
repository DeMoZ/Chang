using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class Colorizable : MonoBehaviour
    {
        [SerializeField] private Image[] baseColors;

        private Color? _baseColor;

        public void SetBaseColor(Color baseColor)
        {
            _baseColor = baseColor;
            foreach (var image in baseColors)
            {
                image.color = baseColor;
            }
        }

        /// <summary>Paints the base color again after something else (a design state) recolored the images.</summary>
        protected void ReapplyBaseColor()
        {
            if (_baseColor.HasValue)
            {
                SetBaseColor(_baseColor.Value);
            }
        }
    }
}

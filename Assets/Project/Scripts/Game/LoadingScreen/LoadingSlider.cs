using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public class LoadingSlider : LoadingSliderAbstract
    {
       [SerializeField] private Slider slider;
       public override void SetProgress(float value)
       {
           slider.value = value;
       }
    }
}
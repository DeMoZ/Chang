using TMPro;
using UnityEngine;

namespace Chang
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text percents;
        [SerializeField] private LoadingSliderAbstract progressSlider;

        public void SetProgress(float value)
        {
            percents.text = $"{(int)(value * 100)}%";
            progressSlider.SetProgress(value);
        }
    }
}
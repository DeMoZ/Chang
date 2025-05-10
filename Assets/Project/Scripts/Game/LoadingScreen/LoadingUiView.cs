using TMPro;
using UnityEngine;

namespace Chang
{
    public class LoadingUiView : MonoBehaviour
    {
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject blocker;
        [SerializeField] private TMP_Text percents;
        [SerializeField] private LoadingSliderAbstract progressSlider;
        [SerializeField] private GameObject loadingAnimation;

        public void SetProgress(float value)
        {
            percents.text = $"{(int)(value * 100)}%";
            progressSlider.SetProgress(value);
        }
        
        public void EnableBackground(bool enable)
        {
            background.SetActive(enable);
        }
        
        public void EnableBlocker(bool enable)
        {
            blocker.SetActive(enable);
        }
        
        public void EnablePercents(bool enable)
        {
            percents.gameObject.SetActive(enable);
        }
        
        public void EnableLoadingAnimation(bool enable)
        {
            loadingAnimation.SetActive(enable);
        }
        
        public void EnableProgressSlider(bool enable)
        {
            progressSlider.gameObject.SetActive(enable);
        }
    }
}
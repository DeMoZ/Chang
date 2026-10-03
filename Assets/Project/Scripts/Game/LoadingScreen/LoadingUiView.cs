using TMPro;
using UnityEngine;

namespace Chang
{
    public class LoadingUiView : MonoBehaviour
    {
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject blocker;
        [SerializeField] private TMP_Text percents;
        [SerializeField] private TMP_Text bytes;
        [SerializeField] private LoadingSliderAbstract progressSlider;
        [SerializeField] private GameObject loadingAnimation;

        public void SetProgress(float value)
        {
            percents.text = $"{(int)(value * 100)}%";
            progressSlider.SetProgress(value);
        }
        
        public void SetBytes(float current, float total)
        {
            bytes.text = total > 0 ? $"{ToMegabytes(current):0.0} / {ToMegabytes(total):0.0} MB" : string.Empty;
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
        
        public void EnableBytes(bool enable)
        {
            bytes.gameObject.SetActive(enable);
        }

        public void EnableLoadingAnimation(bool enable)
        {
            loadingAnimation.SetActive(enable);
        }
        
        public void EnableProgressSlider(bool enable)
        {
            progressSlider.gameObject.SetActive(enable);
        }

        private static float ToMegabytes(float bytesCount)
        {
            return bytesCount / (1024f * 1024f);
        }
    }
}
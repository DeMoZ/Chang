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
            if (percents != null) percents.text = $"{(int)(value * 100)}%";
            if (progressSlider != null) progressSlider.SetProgress(value);
        }
        
        public void SetBytes(float current, float total)
        {
            if (bytes == null)
            {
                return;
            }

            bytes.text = total > 0 ? $"{ToMegabytes(current):0.0} / {ToMegabytes(total):0.0} MB" : string.Empty;
        }

        public void EnableBackground(bool enable)
        {
            if (background != null) background.SetActive(enable);
        }
        
        public void EnableBlocker(bool enable)
        {
            if (blocker != null) blocker.SetActive(enable);
        }
        
        public void EnablePercents(bool enable)
        {
            if (percents != null) percents.gameObject.SetActive(enable);
        }
        
        public void EnableBytes(bool enable)
        {
            if (bytes != null) bytes.gameObject.SetActive(enable);
        }

        public void EnableLoadingAnimation(bool enable)
        {
            if (loadingAnimation != null) loadingAnimation.SetActive(enable);
        }
        
        public void EnableProgressSlider(bool enable)
        {
            if (progressSlider != null) progressSlider.gameObject.SetActive(enable);
        }

        private static float ToMegabytes(float bytesCount)
        {
            return bytesCount / (1024f * 1024f);
        }
    }
}
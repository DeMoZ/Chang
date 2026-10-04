using UnityEngine;

namespace Chang.UI
{
    public class PlayStopButton : CButton
    {
        [SerializeField] private GameObject playObject;
        [SerializeField] private GameObject stopObject;

        public void SetPlay(bool isPlay)
        {
            if (playObject != null)
            {
                playObject.SetActive(isPlay);
            }

            if (stopObject != null)
            {
                stopObject.SetActive(!isPlay);
            }
        }
    }
}
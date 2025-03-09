using UnityEngine;

namespace Chang.UI
{
    public class PlayStopButton : CButton
    {
        [SerializeField] private GameObject playObject;
        [SerializeField] private GameObject stopObject;

        public void SetPlay(bool isPlay)
        {
            playObject.SetActive(isPlay);
            stopObject.SetActive(!isPlay);
        }
    }
}
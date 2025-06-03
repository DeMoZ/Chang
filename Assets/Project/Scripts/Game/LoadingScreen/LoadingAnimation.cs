using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public class LoadingAnimation : MonoBehaviour
    {
        [SerializeField] private Image up;
        [SerializeField] private Image down;
        [SerializeField] private float speed = 100;

        private void OnEnable()
        {
            StartCoroutine(Rotate());
        }

        private IEnumerator Rotate()
        {
            transform.localRotation = Quaternion.identity;
            var duration = 0.01f * speed;
            
            while (true)
            {
                up.gameObject.SetActive(true);
                down.gameObject.SetActive(false);
                up.fillAmount = 0f;
                
                var upTween = DOTween.To(
                    () => up.fillAmount,
                    x => up.fillAmount = x,
                    1f, duration).SetEase(Ease.Linear);
                
                yield return upTween.WaitForCompletion();
                
                up.gameObject.SetActive(false);
                down.gameObject.SetActive(true);
                down.fillAmount = 1f;

                var downTween = DOTween.To(
                    () => down.fillAmount,
                    x => down.fillAmount = x,
                    0f, duration).SetEase(Ease.Linear);
                
                yield return downTween.WaitForCompletion();
            }
        }
    }
}
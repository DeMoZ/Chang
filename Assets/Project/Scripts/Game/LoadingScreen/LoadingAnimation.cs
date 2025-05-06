using System.Collections;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private float speed = 100;
    private void OnEnable()
    {
        StartCoroutine(Rotate());
    }
        
    private IEnumerator Rotate()
    {
        transform.localRotation = Quaternion.identity;
        while (true)
        {
            transform.Rotate(0, 0, 1 * Time.deltaTime * speed);
            yield return null;
        }
    }
}

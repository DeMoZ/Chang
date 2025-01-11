using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _mark;
    [SerializeField] private GameObject _changeUp;
    [SerializeField] private GameObject _changeDown;
    [SerializeField] private TMP_Text _word;
    [SerializeField] private Image _bg;

    public void Set(string word, string mark = null, bool? isUp = null)
    {
        _word.text = word;
        _mark.text = mark;
        _mark.gameObject.SetActive(!string.IsNullOrEmpty(mark));

        if (isUp == null)
        {
            _changeDown.SetActive(false);
            _changeUp.SetActive(false);
        }
        else
        {
            _changeDown.SetActive(!isUp.Value);
            _changeUp.SetActive(isUp.Value);
        }
        
        gameObject.SetActive(true);
    }
}
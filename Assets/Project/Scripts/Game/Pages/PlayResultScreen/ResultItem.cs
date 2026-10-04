using Chang.UI.DesignSystem;
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

    [Header("Design (optional)")]
    [Tooltip("ResultRow variants: Up, Down")]
    [SerializeField] private DesignStates _states;
    [Tooltip("Shown when the change is known (the design's Delta badge)")]
    [SerializeField] private GameObject _change;
    [Tooltip("Mastery meter segments from the lowest mark up; as many are lit as the mark")]
    [SerializeField] private Graphic[] _meterSegments = { };
    [SerializeField] private Color _meterOnColor = new(0.85f, 0.6f, 0.12f);
    [SerializeField] private Color _meterOffColor = new(0.94f, 0.9f, 0.82f);

    public void Set(string word, string mark = null, bool? isUp = null)
    {
        _word.text = word;
        if (_mark != null)
        {
            _mark.text = mark;
            _mark.gameObject.SetActive(!string.IsNullOrEmpty(mark));
        }

        if (_states != null && isUp != null)
        {
            _states.Apply(isUp.Value ? "Up" : "Down");
        }

        SetActive(_change, isUp != null);
        SetActive(_changeDown, isUp == false);
        SetActive(_changeUp, isUp == true);
        SetMeter(mark);

        gameObject.SetActive(true);
    }

    private void SetMeter(string mark)
    {
        if (_meterSegments.Length == 0)
        {
            return;
        }

        var hasMark = int.TryParse(mark, out var value);
        for (var i = 0; i < _meterSegments.Length; i++)
        {
            _meterSegments[i].color = hasMark && i < value ? _meterOnColor : _meterOffColor;
        }

        _meterSegments[0].transform.parent.gameObject.SetActive(hasMark);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }
}

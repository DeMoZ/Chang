using UnityEngine;

namespace Chang
{
    /// <summary>
    /// Progress bar drawn as a track with a fill object (the design's Bar/Value): the fill is stretched from the left edge.
    /// </summary>
    public class LoadingFillBar : LoadingSliderAbstract
    {
        [SerializeField] private RectTransform _fill;

        public override void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            _fill.anchorMin = new Vector2(0f, 0f);
            _fill.anchorMax = new Vector2(value, 1f);
            _fill.pivot = new Vector2(0f, 0.5f);
            _fill.offsetMin = Vector2.zero;
            _fill.offsetMax = Vector2.zero;
            _fill.gameObject.SetActive(value > 0f);
        }
    }
}

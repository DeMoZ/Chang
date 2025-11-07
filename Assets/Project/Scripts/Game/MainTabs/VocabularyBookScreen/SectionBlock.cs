using UnityEngine;

namespace Chang.GameBook
{
    public class SectionBlock : Colorizable
    {
        [SerializeField] private RectTransform container;
        public RectTransform Container => container;
        public GameBookSection SectionView { get; set; }
    }
}
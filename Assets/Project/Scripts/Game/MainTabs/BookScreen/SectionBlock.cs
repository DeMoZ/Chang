using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class SectionBlock : Colorizable
    {
        [SerializeField] private RectTransform container;

        private GameBookSection _sectionView;

        public RectTransform Container => container;

        public GameBookSection SectionView
        {
            get => _sectionView;
            set
            {
                if (_sectionView != null)
                {
                    _sectionView.CollapseToggled -= OnCollapseToggled;
                }

                _sectionView = value;
                if (_sectionView != null)
                {
                    _sectionView.CollapseToggled += OnCollapseToggled;
                }
            }
        }

        /// <summary>Collapsing a section hides its lesson rows and the card background, the header stays.</summary>
        private void OnCollapseToggled(bool collapsed)
        {
            var background = GetComponent<Graphic>();
            if (background != null)
            {
                background.enabled = !collapsed;
            }

            foreach (Transform child in container)
            {
                if (_sectionView == null || child != _sectionView.transform)
                {
                    child.gameObject.SetActive(!collapsed);
                }
            }
        }
    }
}

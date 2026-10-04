using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.GameBook
{
    public class BookView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private SectionBlock sectionBlockPrefab;
        [SerializeField] private RectTransform rowPrefab;
        [SerializeField] private GameBookSection sectionPrefab;
        [SerializeField] private GameBookItem upLessonPrefab;
        [SerializeField] private GameBookItem downLessonPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private Gradient sectionsColors;
        [SerializeField] private Gradient lessonMarkColors;

        [Tooltip("Horizontal offsets of consecutive lessons inside a section, repeated: draws the zig-zag lesson path. Empty = no offset.")]
        [SerializeField] private float[] lessonPathOffsets = { };

        private readonly List<GameObject> _created = new();
        
        public Color GetNextColor(int index) => sectionsColors.colorKeys[index % sectionsColors.colorKeys.Length].color;
        public Color GetLessonColor(float index) => lessonMarkColors.Evaluate(index);
        public float ScrollPosition
        {
            get => scrollRect.verticalNormalizedPosition;
            set => scrollRect.verticalNormalizedPosition = value;
        }

        public SectionBlock InstantiateSectionBlock()
        {
            var go = Instantiate(sectionBlockPrefab, content);
            go.gameObject.SetActive(true);
            go.SectionView = InstantiateSection(go.Container);
            _created.Add(go.gameObject);
            return go;
        }

        public RectTransform InstantiateRow(RectTransform sectionBlock)
        {
            var got = Instantiate(rowPrefab, sectionBlock);
            got.gameObject.SetActive(true);

            return got;
        }

        public GameBookItem InstantiateUpLesson(RectTransform row)
        {
            return InstantiateLesson(upLessonPrefab, row);
        }

        public GameBookItem InstantiateDownLesson(RectTransform row)
        {
            return InstantiateLesson(downLessonPrefab, row);
        }

        private GameBookItem InstantiateLesson(GameBookItem prefab, RectTransform row)
        {
            // Index of the lesson inside its section, counted before the new one is added.
            var section = row.parent != null ? row.parent : row;
            var index = section.GetComponentsInChildren<GameBookItem>(true).Length;

            var go = Instantiate(prefab, row);
            go.gameObject.SetActive(true);

            if (lessonPathOffsets.Length > 0)
            {
                go.SetHorizontalShift(lessonPathOffsets[index % lessonPathOffsets.Length]);
            }

            return go;
        }

        private GameBookSection InstantiateSection(Transform sectionBlock)
        {
            var go = Instantiate(sectionPrefab, sectionBlock);
            go.gameObject.SetActive(true);

            return go;
        }

        /// <summary>Destroys the sections created by this view; static children of the content (titles) stay.</summary>
        public void Clear()
        {
            foreach (var item in _created)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }

            _created.Clear();
        }
    }
}
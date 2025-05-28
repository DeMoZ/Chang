using UnityEngine;
namespace Chang.GameBook
{
    public class GameBookView : MonoBehaviour
    {
        [SerializeField] private SectionBlock sectionBlockPrefab;
        [SerializeField] private RectTransform rowPrefab;
        [SerializeField] private GameBookSection sectionPrefab;
        [SerializeField] private GameBookItem upLessonPrefab;
        [SerializeField] private GameBookItem downLessonPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private Gradient sectionsColors;
        [SerializeField] private Gradient lessonMarkColors;
        
        public Color GetNextColor(int index) => sectionsColors.colorKeys[index % sectionsColors.colorKeys.Length].color;
        public Color GetLessonColor(float index) => lessonMarkColors.Evaluate(index);
        
        public SectionBlock InstantiateSectionBlock()
        {
            var go = Instantiate(sectionBlockPrefab, content);
            go.gameObject.SetActive(true);
            go.SectionView = InstantiateSection(go.Container);
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
            var go = Instantiate(upLessonPrefab, row);
            go.gameObject.SetActive(true);

            return go;
        }

        public GameBookItem InstantiateDownLesson(RectTransform row)
        {
            var go = Instantiate(downLessonPrefab, row);
            go.gameObject.SetActive(true);

            return go;
        }

        private GameBookSection InstantiateSection(Transform sectionBlock)
        {
            var go = Instantiate(sectionPrefab, sectionBlock);
            go.gameObject.SetActive(true);

            return go;
        }

        public void Clear()
        {
            foreach (Transform item in content)
            {
                Destroy(item.gameObject);
            }
        }
    }
}
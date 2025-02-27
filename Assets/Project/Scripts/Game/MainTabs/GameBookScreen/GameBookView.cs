using UnityEngine;

namespace Chang.GameBook
{
    public class GameBookView : MonoBehaviour
    {
        [SerializeField] private RectTransform sectionBlockPrefab;
        [SerializeField] private RectTransform rowPrefab;
        [SerializeField] private GameBookItem upLessonPrefab;
        [SerializeField] private GameBookItem downLessonPrefab;

        [SerializeField] private GameBookSection sectionPrefab;
        [SerializeField] private GameBookSection topSection;
        [SerializeField] private Transform content;

        public GameBookSection TopSection => topSection;

        public RectTransform InstantiateSectionBlock(out GameBookSection section)
        {
            var got = Instantiate(sectionBlockPrefab, content);
            got.gameObject.SetActive(true);

            section = InstantiateSection(got);
            return got;
        }
        
        public RectTransform InstantiateRow(RectTransform sectionBlock)
        {
            var got = Instantiate(rowPrefab, sectionBlock);
            got.gameObject.SetActive(true);
            
            return got;
        }

        public GameBookItem InstantiateUpLesson(RectTransform row)
        {
            var got = Instantiate(upLessonPrefab, row);
            got.gameObject.SetActive(true);
            
            return got;
        }
        
        public GameBookItem InstantiateDownLesson(RectTransform row)
        {
            var got = Instantiate(downLessonPrefab, row);
            got.gameObject.SetActive(true);
            
            return got;
        }
        
        private GameBookSection InstantiateSection(Transform sectionBlock)
        {
            var section = Instantiate(sectionPrefab, sectionBlock);
            section.gameObject.SetActive(true);

            return section;
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
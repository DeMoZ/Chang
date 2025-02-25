using UnityEngine;

namespace Chang.GameBook
{
    public class GameBookView : MonoBehaviour
    {
        [SerializeField] private GameBookSection sectionPrefab;
        [SerializeField] private GameBookItem lessonPrefab;
        [SerializeField] private GameBookSection topSection;
        [SerializeField] private Transform content;
        
        public GameBookSection TopSection => topSection;

        public GameBookSection InstantiateSection()
        {
            var section = Instantiate(sectionPrefab, content);
            section.gameObject.SetActive(true);
            
            return section;
        }

        public GameBookItem InstantiateLesson()
        {
            var lesson = Instantiate(lessonPrefab, content);
            lesson.gameObject.SetActive(true);

            return lesson;
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
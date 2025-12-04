namespace Chang.Vocabulary
{
    public interface IBookController
    {
        void OnLessonClicked(string sectionName, int lessonIndex);

        void OnSectionRepeatClicked(string section);

        void OnGeneralRepeatClicked();
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang.UI
{
    public abstract class CScreen : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] public virtual QuestionType ScreenType { get; }

        public PagesSoundController PagesSoundController { get; private set; }

        public void SetPagesSoundController(PagesSoundController pagesSoundController)
        {
            PagesSoundController = pagesSoundController;
        }
    }
}
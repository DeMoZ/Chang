using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang.UI
{
    public abstract class CScreen : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] public virtual QuestionType ScreenType { get; }

        protected PagesSoundController PagesSoundController { get; private set; }

        public void SetPagesSoundController(PagesSoundController pagesSoundController)
        {
            PagesSoundController = pagesSoundController;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

namespace Chang.UI
{
    public abstract class CScreen : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] public virtual QuestionType ScreenType { get; }
        //[ShowInInspector, ReadOnly] public CScreenContentBase ScreenContent { get; }
    }
}
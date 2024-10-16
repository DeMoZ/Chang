using UnityEngine;

namespace Chang.UI
{
    public abstract class CScreen : MonoBehaviour
    {
        // [SerializeField] protected CButton exitBtn;
        // [SerializeField] protected CToggle tnanscriptionTgl;

        [field: SerializeField] public virtual QuestionType ScreenType { get; }

        [field: SerializeField] public CScreenContentBase ScreenContent { get; }

        private void Awake()
        {
            // todo subscribe to screen events
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
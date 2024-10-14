using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public abstract class CScreen : MonoBehaviour
    {
        [SerializeField] protected CButton exitBtn;
        [SerializeField] protected CToggle tnanscriptionTgl;

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

    [Serializable]
    public class CToggle
    {
        [SerializeField] private Toggle toggle;
    }


    [Serializable]
    public class CButton
    {
        [SerializeField] private Button button;

        public event Action OnClick;
    }
}
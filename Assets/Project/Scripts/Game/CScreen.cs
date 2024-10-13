using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public enum ScreenType
    {
        DemostrationWord,
        SelectWord,
        MatchWords,
    }

    public abstract class CScreen : MonoBehaviour
    {
        public virtual ScreenType ScreenType { get; }

        [SerializeField] private CButton exitBtn;
        [SerializeField] private CToggle tnanscriptionTgl;


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

    public class CToggle
    {
    }


    public class CButton
    {
        [SerializeField] private Button button;

        public event Action OnClick;
    }
}
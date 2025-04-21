using UnityEngine;

namespace Popup
{
    public class PopupTester : MonoBehaviour
    {
        [SerializeField] private PopupView popupViewPrefab;
        private PopupController popupController;

        private void Start()
        {
            popupController = new PopupController();
            
            PopupView view = Instantiate(popupViewPrefab, transform);
            popupController.Init(view);
            
           // CreateTstPopup();
           CreateInputNamePopup();
        }

        private void CreateTstPopup()
        {
            popupController.CreatePopup(
                new PopupHeader(" This Is Test Header"),
                new PopupLabelAndInput( "Test Label", "Test Input", (s) => { Debug.Log(s);}, (s) => { }),
                new PopupLabel("Test Label"),
                new PopupButton( "Test Button", () => Debug.Log("Button Clicked"), (b) => { }),
                new PopupButton( "Test Button 2", () => Debug.Log("Button 2 Clicked"), (b) => { }) ); 
        }

        private void CreateInputNamePopup()
        {
            popupController.CreatePopup(
                new PopupHeader("Create Name"),
                new PopupLabelAndInput( "Input name", "aa", (s) => { Debug.Log(s);}, (s) => { }),
                new PopupButton( "Cancel", () => Debug.Log("Cancelled"), (b) => { }),
                new PopupButton( "Submit", () => Debug.Log("Submitted"), (b) => { }) );
        }
    }
}
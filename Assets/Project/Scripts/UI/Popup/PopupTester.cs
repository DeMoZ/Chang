using System;
using UnityEngine;

namespace Popup
{
    public class PopupTester : MonoBehaviour
    {
        [SerializeField] private PopupView popupViewPrefab;
        private PopupController<ChangeNamePopupModel> popupController;

        private void Start()
        {
            PopupView view = Instantiate(popupViewPrefab, transform);
            ChangeNamePopupModel model = new();
            popupController = new PopupController<ChangeNamePopupModel>(view, model);

            // CreateTstPopup();
            CreateInputNamePopup();
        }

        private void CreateTstPopup()
        {
            // popupController.CreatePopup(
            //     new PopupHeader(" This Is Test Header"),
            //     new PopupLabelAndInput("Test Label", "Test Input", (s) => { Debug.Log(s); }, (s) => { }),
            //     new PopupLabel("Test Label"),
            //     new PopupButton("Test Button", () => Debug.Log("Button Clicked"), (b) => { }),
            //     new PopupButton("Test Button 2", () => Debug.Log("Button 2 Clicked"), (b) => { }));
        }

        private void CreateInputNamePopup()
        {
            // popupController.CreatePopup(
            //     new PopupHeader("Create Name"),
            //     new PopupLabelAndInput( "Input name", "aa", (s) => { Debug.Log(s);}, (s) => { }),
            //     new PopupButton( "Cancel", () => Debug.Log("Cancelled"), (b) => { }),
            //     new PopupButton( "Submit", () => Debug.Log("Submitted"), (b) => { }) );
            CreateInputNamePopup(txt => { Debug.Log(txt); },
                txt => { Debug.Log(txt); },
                () => { Debug.Log("Submitted"); },
                () => { Debug.Log("Cancelled"); }
            );
        }

        private void CreateInputNamePopup(Action<string> onInput, Action<string> onSetLabelText, Action onSubmit, Action onCancel)
        {
            // popupController.CreatePopup(
            //     //new PopupHeader("Create Name"),
            //     new PopupLabelAndInput("Input name", "",
            //         (text) =>
            //         {
            //             Debug.Log(text);
            //             onInput?.Invoke(text);
            //         },
            //         onSetLabelText,
            //         (s) => { }
            //     ),
            //     new PopupButton("Cancel", () => Debug.Log("Cancelled"), (b) => { }),
            //     new PopupButton("Submit", () => Debug.Log("Submitted"), (b) => { }));
        }
    }
}
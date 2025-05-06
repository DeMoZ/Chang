using System.Collections.Generic;
using Chang;
using DMZ.Events;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Popup
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private PopupView popupPrefab;

        private Stack<IViewController> _popupStack = new();

        public PopupController<ChangeNamePopupModel> ShowChangeNamePopup(ChangeNamePopupModel model)
        {
            PopupView popupView = Instantiate(popupPrefab, transform);
            PopupController<ChangeNamePopupModel> popupController = new(popupView, model);

            popupController.CreatePopup(
                new PopupHeader("Change Name"),
                new PopupLabelAndInput(model.LabelText, model.NameInput.Value,
                    text => { model.NameInput.Value = text; },
                    color => { }
                ),
                new PopupButton("Cancel", () =>
                {
                    Debug.Log($"{model.GetType()} Cancelled");
                    model.OnChangeNameCancel?.Invoke();
                }, new DMZState<bool>()),
                new PopupButton("Submit",
                    () =>
                    {
                        Debug.Log($"{model.GetType()} Submitted");
                        model.OnChangeNameSubmit?.Invoke();
                    },
                    model.OnSetSubmitInteractable));

            _popupStack.Push(popupController);
            return popupController;
        }

        public PopupController<ErrorPopupModel> ShowErrorPopup(ErrorPopupModel model)
        {
            PopupView popupView = Instantiate(popupPrefab, transform);
            PopupController<ErrorPopupModel> popupController = new(popupView, model);

            popupController.CreatePopup(
                new PopupHeader("Error"),
                new PopupLabel(model.LabelText),
                new PopupButton("Ok", () =>
                    {
                        Debug.Log($"{model.GetType()} Ok clicked");
                        model.OnOkClicked?.Invoke();
                    }
                    , new DMZState<bool>())
            );

            _popupStack.Push(popupController);
            return popupController;
        }

        public PopupController<YesNoPopupModel> ShowYesNoPopup(YesNoPopupModel model)
        {
            PopupView popupView = Instantiate(popupPrefab, transform);
            PopupController<YesNoPopupModel> popupController = new(popupView, model);

            popupController.CreatePopup(
                new PopupHeader(model.HeaderText.Value),
                new PopupLabel(model.LabelText),
                new PopupButton("Cancel", () =>
                    {
                        Debug.Log($"{model.GetType()} Cancel clicked");
                        model.OnCancelClicked?.Invoke();
                    }
                    , new DMZState<bool>()),
                new PopupButton("Ok", () =>
                    {
                        Debug.Log($"{model.GetType()} Ok clicked");
                        model.OnOkClicked?.Invoke();
                    }
                    , new DMZState<bool>())
            );

            _popupStack.Push(popupController);
            return popupController;
        }

        public void Dispose()
        {
            // TODO release managed resources here
            foreach (var popup in _popupStack)
            {
                popup.Dispose();
            }
        }

        public void DisposePopup(IViewController controller)
        {
            RemoveFromStack(ref _popupStack, controller);
            controller.SetViewActive(false);
            controller.Dispose();
        }

        private static void RemoveFromStack<T>(ref Stack<T> stack, T item)
        {
            var tempList = new List<T>(stack);
            tempList.Remove(item);
            tempList.Reverse();
            stack.Clear();
            stack = new Stack<T>(tempList);
        }
    }
}
using System;
using System.Collections.Generic;
using Chang;
using DMZ.Events;
using UnityEngine;

namespace Popup
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private PopupView popupPrefab;

        private Stack<IViewController> _popupStack = new();

        public PopupController<ChangeNamePopupModel> ShowChangeNamePopup(ChangeNamePopupModel model)
        {
            PopupView popupView = Instantiate(popupPrefab, transform);
            PopupController<ChangeNamePopupModel> popupController = new();
            popupController.Init(popupView, model);

            popupController.CreatePopup(
                new PopupHeader("Change Name"),
                new PopupLabelAndInput(model.LabelText, string.Empty,
                    text => { model.OnNameInput?.Invoke(text); },
                    color => { }
                ),
                new PopupButton("Cancel", () =>
                {
                    Debug.Log("Cancelled");
                    model.OnChangeNameCancel?.Invoke();
                }, new DMZState<bool>()),
                new PopupButton("Submit",
                    () =>
                    {
                        Debug.Log("Submitted");
                        model.OnChangeNameSubmit?.Invoke();
                    },
                    model.OnSetSubmitInteractable));
            
            _popupStack.Push(popupController);
            model.OnNameInput?.Invoke(string.Empty); // trigger initial validation
            return popupController;
        }

        public void Dispose()
        {
            // TODO release managed resources here
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
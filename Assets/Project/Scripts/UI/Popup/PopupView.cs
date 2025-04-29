using System;
using DMZ.Events;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Popup
{
    public class PopupView : MonoBehaviour
    {
        [SerializeField] private HeaderView headerPrefab;
        [SerializeField] private LabelView labelPrefab;
        [SerializeField] private LabelAndInputView labelAndInputPrefab;
        [SerializeField] private ButtonView buttonPrefab;
        [SerializeField] private Transform buttonsContainerPrefab;
        [SerializeField] private Transform content;

        private Transform _buttonsContainer;

        public void Clear()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }

        public void CreateHeader(string text)
        {
            HeaderView header = Instantiate(headerPrefab, content);
            header.Text = text;
            header.gameObject.SetActive(true);
        }

        public void CreateLabel(DMZState<string> text)
        {
            LabelView label = Instantiate(labelPrefab, content);
            label.Text = text;
            label.Init();
            label.gameObject.SetActive(true);
        }

        public void CreateLabelAndInput(
            DMZState<string> labelText,
            string inputText,
            Action<string> onInputTextChanged)
        {
            LabelAndInputView labelAndInput = Instantiate(labelAndInputPrefab, content);
            labelAndInput.LabelText = labelText;
            labelAndInput.InputText = inputText;
            labelAndInput.OnInputTextChanged = onInputTextChanged;
            labelAndInput.Init();
            labelAndInput.gameObject.SetActive(true);
        }

        public void CreateButton(string text,
            Action onClick,
            DMZState<bool> onSetInteractable)
        {
            if (_buttonsContainer == null)
            {
                _buttonsContainer = Instantiate(buttonsContainerPrefab, content).transform;
                foreach (Transform child in _buttonsContainer)
                {
                    Destroy(child.gameObject);
                }

                _buttonsContainer.gameObject.SetActive(true);
                HorizontalLayoutGroup group = _buttonsContainer.GetOrAddComponent<HorizontalLayoutGroup>();
                group.enabled = true;
            }

            ButtonView button = Instantiate(buttonPrefab, _buttonsContainer);
            button.Text = text;
            button.OnClick = onClick;
            button.OnSetInteractable = onSetInteractable;
            button.Init();
            button.gameObject.SetActive(true);
        }
    }
}
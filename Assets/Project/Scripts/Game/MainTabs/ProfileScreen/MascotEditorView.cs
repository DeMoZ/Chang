using System;
using System.Collections.Generic;
using Chang.Mascot;
using Chang.Profile;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    /// <summary>
    /// Mascot editor (profile → "My mascot"): a tab per mascot part, a grid of its options and a live preview.
    /// Built from the design screen "Profile · Mascot — Hat" by DesignViewsBuilder.
    /// </summary>
    public class MascotEditorView : MonoBehaviour
    {
        [SerializeField] private Button backBtn;
        [SerializeField] private Button randomBtn;
        [SerializeField] private Button saveBtn;
        [SerializeField] private MascotImage preview;

        [Tooltip("Part tabs in the order of MascotPart")]
        [SerializeField] private DesignSelection partSelection;
        [SerializeField] private HorizontalDragScroll partsScroll;

        [SerializeField] private TMP_Text partText;
        [SerializeField] private TMP_Text optionText;

        [Tooltip("Option tiles, MascotLook.OptionCount of them")]
        [SerializeField] private DesignSelection optionSelection;
        [SerializeField] private List<MascotImage> optionImages = new();

        private Action _onBack;
        private Action _onRandom;
        private Action _onSave;
        private Action<MascotPart> _onPart;
        private Action<int> _onOption;

        public void Init(Action onBack, Action onRandom, Action onSave, Action<MascotPart> onPart, Action<int> onOption)
        {
            _onBack = onBack;
            _onRandom = onRandom;
            _onSave = onSave;
            _onPart = onPart;
            _onOption = onOption;
        }

        /// <summary>Shows the look being edited with the options of one part.</summary>
        public void Set(MascotLook look, MascotPart part)
        {
            preview.Show(look);

            var partIndex = (int)part;
            if (partSelection.Current != partIndex)
            {
                partSelection.Select(partIndex);
                partsScroll.Reveal(partSelection.Item(partIndex));
            }

            optionSelection.Select(look[part]);

            for (var i = 0; i < optionImages.Count; i++)
            {
                if (MascotCatalog.IsColor(part))
                {
                    optionImages[i].ShowSwatch(MascotCatalog.Color(part, i));
                }
                else
                {
                    optionImages[i].Show(MascotCatalog.TileLook(look, part, i));
                }
            }

            partText.text = $"{MascotCatalog.Label(part)} · {MascotCatalog.ThaiLabel(part)}";
            optionText.text = $"{look[part] + 1} / {MascotLook.OptionCount} · {MascotCatalog.OptionName(part, look[part])}";
        }

        private void OnEnable()
        {
            backBtn.onClick.AddListener(OnBackClick);
            randomBtn.onClick.AddListener(OnRandomClick);
            saveBtn.onClick.AddListener(OnSaveClick);
            partSelection.Clicked += OnPartClick;
            optionSelection.Clicked += OnOptionClick;
        }

        private void OnDisable()
        {
            backBtn.onClick.RemoveListener(OnBackClick);
            randomBtn.onClick.RemoveListener(OnRandomClick);
            saveBtn.onClick.RemoveListener(OnSaveClick);
            partSelection.Clicked -= OnPartClick;
            optionSelection.Clicked -= OnOptionClick;
        }

        private void OnBackClick()
        {
            _onBack?.Invoke();
        }

        private void OnRandomClick()
        {
            _onRandom?.Invoke();
        }

        private void OnSaveClick()
        {
            _onSave?.Invoke();
        }

        private void OnPartClick(int index)
        {
            _onPart?.Invoke((MascotPart)index);
        }

        private void OnOptionClick(int index)
        {
            _onOption?.Invoke(index);
        }
    }
}

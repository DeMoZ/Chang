using System;
using Chang.Mascot;
using Chang.Profile;
using Chang.UI;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang
{
    public class ProfileView : MonoBehaviour
    {
        [SerializeField] private Button logoutBtn;
        [SerializeField] private TMP_Text userNameText;
        [SerializeField] private TMP_Text userIdText;
        [SerializeField] private Button changeNameBtn;
        [SerializeField] private TMP_Text genderText;
        [SerializeField] private Button changeGenderBtn;
        [SerializeField] private TMP_Text languageText;
        [SerializeField] private Button changeLanguageBtn;

        [Header("Design (optional)")]
        [Tooltip("Second place where the name is shown (the profile header)")]
        [SerializeField] private TMP_Text userNameTitle;

        [Tooltip("Politeness options in order: Male, Female")]
        [SerializeField] private DesignSelection genderSelection;

        [Tooltip("The player's mascot in the profile header")]
        [SerializeField] private MascotImage mascotImage;

        [Tooltip("Buttons that open the mascot editor (the pencil on the avatar, the \"My mascot\" row)")]
        [SerializeField] private Button[] editMascotBtns = Array.Empty<Button>();
        
        private Action _onLogOutClick;
        private Action _onChangeNameClick;
        private Action _onChangeGenderClick;
        private Action _onChangeLanguageClick;
        private Action _onEditMascotClick;

        public void Init(Action onLogOutClick, Action onChangeNameClick, Action onChangeGenderClick, Action onChangeLanguageClick,
            Action onEditMascotClick)
        {
            _onLogOutClick = onLogOutClick;
            _onChangeNameClick = onChangeNameClick;
            _onChangeGenderClick = onChangeGenderClick;
            _onChangeLanguageClick = onChangeLanguageClick;
            _onEditMascotClick = onEditMascotClick;
        }
        
        public void SetUserName(string userName)
        {
            if (userNameText != null)
            {
                userNameText.text = userName;
            }

            if (userNameTitle != null)
            {
                userNameTitle.text = userName;
            }
        }
        
        public void SetGender(GenderType gender)
        {
            if (genderText != null)
            {
                genderText.GetComponent<LocalizedTMPText>().LocalizationKey = $"Lobby.Profile.Gender.{gender}";
            }

            if (genderSelection != null)
            {
                genderSelection.Select(gender switch
                {
                    GenderType.Male => 0,
                    GenderType.Female => 1,
                    _ => -1
                });
            }
        }
        
        public void SetLanguage(string languageName)
        {
            if (languageText != null)
            {
                languageText.text = languageName;
            }
        }

        public void SetMascot(MascotLook look)
        {
            if (mascotImage != null)
            {
                mascotImage.Show(look);
            }
        }

        public void SetUserId(string userId)
        {
            if (userIdText != null)
            {
                userIdText.text = userId;
            }
        }

        private void OnEnable()
        {
            logoutBtn.onClick.AddListener(OnLogOutClick);
            changeNameBtn.onClick.AddListener(OnChangeNameClick);
            changeGenderBtn.onClick.AddListener(OnChangeGenderClick);
            changeLanguageBtn.onClick.AddListener(OnChangeLanguageClick);
            foreach (var button in editMascotBtns)
            {
                button.onClick.AddListener(OnEditMascotClick);
            }
        }

        private void OnDisable()
        {
            logoutBtn.onClick.RemoveListener(OnLogOutClick);
            changeNameBtn.onClick.RemoveListener(OnChangeNameClick);
            changeGenderBtn.onClick.RemoveListener(OnChangeGenderClick);
            changeLanguageBtn.onClick.RemoveListener(OnChangeLanguageClick);
            foreach (var button in editMascotBtns)
            {
                button.onClick.RemoveListener(OnEditMascotClick);
            }
        }

        private void OnLogOutClick()
        {
            _onLogOutClick?.Invoke();
        }
        
        private void OnChangeNameClick()
        {
            _onChangeNameClick?.Invoke();
        }
        
        private void OnChangeGenderClick()
        {
            _onChangeGenderClick?.Invoke();
        }

        private void OnChangeLanguageClick()
        {
            _onChangeLanguageClick?.Invoke();
        }

        private void OnEditMascotClick()
        {
            _onEditMascotClick?.Invoke();
        }
    }
}

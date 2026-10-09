using System.Linq;
using Chang.UI.DesignSystem;
using DMZ.Legacy.LoginScreen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.Editor.DesignSystem
{
    public static partial class DesignViewsBuilder
    {
        /// <summary>
        /// Login screen for <see cref="LogInView"/> (the DMZ.Legacy.LoginScreen submodule). The design draws only the first step
        /// (sign in with a name or as a guest); the name and password form and the signed-in panel are cards built from the
        /// design components (Dialog card, TextField, Segmented, buttons) over the same screen.
        /// </summary>
        private static void BuildLogin()
        {
            var screen = Screen("Login");
            var input = LoadPrefab($"{PopupRoot}/LabelAndInput.prefab");

            BuildPrefab($"{ViewsRoot}/LoginView.prefab", root =>
            {
                Stretch((RectTransform)root.transform);

                // Fills the screen outside the column (landscape, tablets) with the design background.
                var background = Child(root.transform, "Background", 0);
                Stretch(background);
                var bgImage = GetOrAdd<Image>(background);
                bgImage.color = Q<Graphic>(screen, PenpotUiBuilder.BackgroundName).color;
                bgImage.raycastTarget = true;

                var column = Child(root.transform, "Column", 1);
                GetOrAdd<WidthLimiter>(column);

                var login = Instance(screen, column, "Login");
                Stretch(login);

                // ---- select login type: the design screen ----
                var actions = Q(login, "Actions");
                var nameBtn = Q(actions, "Chang DS / Button / Secondary");
                var guestBtn = Q(actions, "Chang DS / Button / Primary");
                Localize(Q(nameBtn, "Label"), "Login.SignInWithName");
                Localize(Q(guestBtn, "Label"), "Login.ContinueAsGuest");
                Localize(Q(actions, "Your progress is saved t"), "Login.CloudHint");
                Localize(Q(actions, "สวัสดี! Let’s start"), "Login.Start");
                Localize(Q(login, "Brand|Learn Thai, one word at"), "Login.Tagline");

                // ---- log in / sign up ----
                var logIn = Card(column, "LogInState");
                var logInContent = Q(logIn, "Content");

                var tabs = Instance(Component("Segmented/Repeat mode"), logInContent, "Tabs");
                Hide(tabs, "Mixed");
                var tabLayout = tabs.GetComponent<HorizontalLayoutGroup>();
                if (tabLayout != null)
                {
                    tabLayout.childControlWidth = true;
                    tabLayout.childForceExpandWidth = true;
                }

                var logInTab = Q(tabs, "Words");
                var signUpTab = Q(tabs, "Sentences");
                Localize(Q(logInTab, "Words"), "Login.LogIn", "Log in");
                Localize(Q(signUpTab, "Sentences"), "Login.SignUp", "Sign up");
                var logInToggle = ToggleOn(logInTab);
                var signUpToggle = ToggleOn(signUpTab);
                var tabSelection = GetOrAdd<DesignSelection>(tabs);
                Set(tabSelection, ("_items", Objects(new[] { (RectTransform)logInTab, (RectTransform)signUpTab })),
                    ("_designSelectedIndex", 0), ("_designNormalIndex", 1));
                Set(GetOrAdd<ToggleSelection>(tabs), ("_selection", tabSelection), ("_toggles", Objects(new[] { logInToggle, signUpToggle })));

                var nameField = Field(input, logInContent, "Name", "Login.Name", "Name", false);
                var nameHint = Hint(logInContent, "NameLbl");
                var passwordField = Field(input, logInContent, "Password", "Login.Password", "Password", true);
                var passwordHint = Hint(logInContent, "PasswordLbl");
                var status = Hint(logInContent, "StatusLbl");

                var authButtons = Row(logInContent, "AuthButtons");
                var back = CardButton("Button/Ghost", authButtons, "BackBtn", "Login.Back", "Back");
                var logInBtn = CardButton("Button/Primary", authButtons, "LogInBtn", "Login.LogIn", "Log in");
                var signUpBtn = CardButton("Button/Primary", authButtons, "SignUpBtn", "Login.SignUp", "Sign up");

                // ---- signed in: the design's account cards (AccountCard / Default, AccountCard / Confirm) ----
                // the earlier build made this panel from a Dialog card in code
                var oldLogged = column.Find("LoggedState");
                if (oldLogged != null)
                {
                    Object.DestroyImmediate(oldLogged.gameObject);
                }

                var logged = Child(column, "AccountState");
                Stretch(logged);
                var account = BottomCard(logged, "AccountCard/Default", "Account");
                var confirm = BottomCard(logged, "AccountCard/Confirm", "Confirm");

                Localize(Q(account, "Header|Title"), "Login.Account", "Account");
                Localize(Q(account, "Buttons|LogOut|Label"), "Lobby.Profile.LogOut", "Log out");
                Localize(Q(account, "Buttons|Delete|Label"), "Login.Delete", "Delete account");
                var logOutBtn = ButtonOn(Q(account, "Buttons|LogOut"));
                var deleteBtn = ButtonOn(Q(account, "Buttons|Delete"));
                var closeBtn = ButtonOn(Q(account, "Header|Close"));

                Localize(Q(confirm, "Title"), "Login.DeleteTitle", "Delete account?");
                Localize(Q(confirm, "Body"), "Login.DeleteConfirm", "All your progress will be lost. This can’t be undone.");
                Localize(Q(confirm, "Buttons|Back|Label"), "Login.Back", "Back");
                Localize(Q(confirm, "Buttons|Delete|Label"), "Login.DeleteSure", "Delete");
                var notSureBtn = ButtonOn(Q(confirm, "Buttons|Back"));
                var sureBtn = ButtonOn(Q(confirm, "Buttons|Delete"));
                Hide(confirm);
                Hide(logged);

                // ---- awaiting a server response ----
                var blocker = Blocker(root.transform, "LoadingBlocker");
                blocker.color = new Color(0.114f, 0.129f, 0.251f, 0.45f);
                blocker.transform.SetAsLastSibling();
                Hide(blocker.transform);

                var view = GetOrAdd<LogInView>(root);
                Set(view,
                    ("_selectLoginTypeState", actions),
                    ("_logInState", logIn),
                    ("_loggedState", logged),
                    ("_userAndPasswordBtn", ButtonOn(nameBtn)),
                    ("_guestBtn", ButtonOn(guestBtn)),
                    ("_nameLbl", nameHint),
                    ("_loginFld", nameField),
                    ("_passwordLbl", passwordHint),
                    ("_passwordFld", passwordField),
                    ("_statusLbl", status),
                    ("_backBtn", back),
                    ("_signUpBtn", signUpBtn),
                    ("_logInBtn", logInBtn),
                    ("_switchLogInTgl", logInToggle),
                    ("_switchSignUpTgl", signUpToggle),
                    ("_logoutContent", account.gameObject),
                    ("_logOutBtn", logOutBtn),
                    ("_deleteBtn", deleteBtn),
                    ("_confirmContent", confirm.gameObject),
                    ("_notSureBtn", notSureBtn),
                    ("_sureBtn", sureBtn),
                    ("_closeBtn", closeBtn),
                    ("_validColor", PenpotDocument.ParseColor("#8a8fa8")),
                    ("_invalidColor", PenpotDocument.ParseColor("#e04f4a")),
                    ("_validInputTextColor", PenpotDocument.ParseColor("#1d2140")),
                    ("_invalidInputTextColor", PenpotDocument.ParseColor("#e04f4a")),
                    ("_loadingBlocker", blocker.gameObject));
            });
        }

        /// <summary>A design card component at the bottom of the screen, as tall as the design draws it.</summary>
        private static RectTransform BottomCard(Transform parent, string component, string name)
        {
            var card = Instance(Component(component), parent, name);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0f);
            card.pivot = new Vector2(0.5f, 0f);
            card.sizeDelta = DesignSize(Component(component));
            card.anchoredPosition = new Vector2(0f, 24f);
            return card;
        }

        /// <summary>The design's dialog card at the bottom of the screen, its sample content hidden; returns the card with a "Content" column.</summary>
        private static RectTransform Card(Transform parent, string name)
        {
            var card = Instance(Component("Dialog/Confirm"), parent, name);
            card.anchorMin = new Vector2(0.5f, 0f);
            card.anchorMax = new Vector2(0.5f, 0f);
            card.pivot = new Vector2(0.5f, 0f);
            card.anchoredPosition = new Vector2(0f, 24f);
            var fitter = GetOrAdd<ContentSizeFitter>(card);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var dialogParts = Component("Dialog/Confirm").transform.Cast<Transform>()
                .Select(t => t.name)
                .Where(n => !n.StartsWith("#"))
                .ToList();
            foreach (Transform t in card)
            {
                if (dialogParts.Contains(t.name))
                {
                    Hide(t);
                }
            }

            var content = Child(card, "Content");
            var layout = GetOrAdd<VerticalLayoutGroup>(content);
            layout.spacing = 12f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            GetOrAdd<LayoutElement>(content).flexibleWidth = 1f;
            Hide(card);
            return card;
        }

        private static TMP_InputField Field(GameObject input, Transform parent, string name, string labelKey, string label, bool password)
        {
            var field = Instance(input, parent, name);
            Localize(Q(field, "Label"), labelKey, label);
            // The placeholder repeats the label.
            Q<TMP_Text>(field.gameObject, "Field|Placeholder").text = string.Empty;
            var inputField = Q<TMP_InputField>(field.gameObject, "Field");
            inputField.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            return inputField;
        }

        /// <summary>A small text under a field: validation hints, the server answer.</summary>
        private static TMP_Text Hint(Transform parent, string name)
        {
            var rt = Child(parent, name);
            var text = CopyText(rt.gameObject, Q<TMP_Text>(Component("TextField/TextField"), "Label"));
            text.text = string.Empty;
            text.alignment = TextAlignmentOptions.Left;
            return text;
        }

        private static RectTransform Row(Transform parent, string name)
        {
            var row = Child(parent, name);
            var layout = GetOrAdd<HorizontalLayoutGroup>(row);
            layout.spacing = 12f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return row;
        }

        private static Button CardButton(string component, Transform parent, string name, string key, string label)
        {
            var button = Instance(Component(component), parent, name);
            var le = GetOrAdd<LayoutElement>(button);
            le.minHeight = le.preferredHeight = 56f;
            le.flexibleWidth = 1f;
            Localize(Q(button, "Label"), key, label);
            return ButtonOn(button);
        }
    }
}

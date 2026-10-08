using System.Linq;
using Chang.GameBook;
using Chang.Sentences;
using Chang.UI;
using Chang.UI.DesignSystem;
using Popup;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Puts the redesigned views (built by <see cref="DesignViewsBuilder"/>) into Game.unity and Bootstrap.unity in place of
    /// the old UI and rewires GameInstaller and PopupManager to them. Old instances are removed from the scenes;
    /// their prefabs stay in Assets/Project/Prefabs.
    /// </summary>
    public static class DesignViewsSceneSetup
    {
        private const string GameScene = "Assets/Project/Scenes/Game.unity";
        private const string BootstrapScene = "Assets/Project/Scenes/Bootstrap.unity";

        [MenuItem("Chang/Design System/Use Views In Scenes", false, 21)]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            SetupGame();
            SetupBootstrap();
            Debug.Log($"[{nameof(DesignViewsSceneSetup)}] [{nameof(Setup)}] Game and Bootstrap scenes use the redesigned views");
        }

        private static GameObject View(string name) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{DesignViewsBuilder.ViewsRoot}/{name}.prefab");

        private static void SetupGame()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            GameObject Root(string name) => roots.First(r => r.name == name);

            var installer = Object.FindFirstObjectByType<GameInstaller>(FindObjectsInactive.Include);
            var so = new SerializedObject(installer);

            // Main screen with tabs.
            var oldMain = roots.FirstOrDefault(r => r.GetComponent<MainUiView>() != null && PrefabUtility.GetCorrespondingObjectFromSource(r) != View("MainUI"));
            var main = roots.FirstOrDefault(r => PrefabUtility.GetCorrespondingObjectFromSource(r) == View("MainUI"));
            if (main == null)
            {
                main = (GameObject)PrefabUtility.InstantiatePrefab(View("MainUI"), scene);
                main.name = "MainUI";
                if (oldMain != null)
                {
                    main.transform.SetSiblingIndex(oldMain.transform.GetSiblingIndex());
                    main.GetComponent<Canvas>().sortingOrder = oldMain.GetComponent<Canvas>().sortingOrder;
                    main.SetActive(oldMain.activeSelf);
                }
            }

            so.FindProperty("mainUiScreen").objectReferenceValue = main.GetComponent<MainUiView>();
            so.FindProperty("bookVocabularyScreen").objectReferenceValue = main.GetComponentInChildren<BookVocabularyView>(true);
            so.FindProperty("bookSentencesScreen").objectReferenceValue = main.GetComponentInChildren<BookSentencesView>(true);
            so.FindProperty("repetitionScreen").objectReferenceValue = main.GetComponentInChildren<RepetitionView>(true);
            so.FindProperty("profileScreen").objectReferenceValue = main.GetComponentInChildren<ProfileView>(true);
            so.FindProperty("mascotEditorScreen").objectReferenceValue = main.GetComponentInChildren<MascotEditorView>(true);

            // Lesson overlay.
            var overlayCanvas = Root("OverlayUI");
            DesignViewsBuilder.ConfigureScaler(overlayCanvas.GetComponent<CanvasScaler>());
            var overlayParent = overlayCanvas.transform.Find("SafeArea");
            var overlay = Replace<GameOverlayView>(overlayParent, View("GameOverlay"));
            so.FindProperty("gameOverlayScreen").objectReferenceValue = overlay;

            // Lesson pages.
            var pagesCanvas = Root("PagesUI");
            DesignViewsBuilder.ConfigureScaler(pagesCanvas.GetComponent<CanvasScaler>());
            var pagesBackground = pagesCanvas.transform.Find("SafeArea/Background");
            if (pagesBackground != null)
            {
                pagesBackground.GetComponent<Graphic>().color = PenpotDocument.ParseColor("#fbf6ec");
            }

            var widthLimit = pagesCanvas.transform.Find("SafeArea/WidthLimit");
            var fitter = widthLimit.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                Object.DestroyImmediate(fitter);
            }

            if (widthLimit.GetComponent<WidthLimiter>() == null)
            {
                widthLimit.gameObject.AddComponent<WidthLimiter>();
            }

            so.FindProperty("playResultScreen").objectReferenceValue = Replace<PlayResultView>(widthLimit, View("PlayResultView"), false);
            so.FindProperty("demonstrationScreen").objectReferenceValue = Replace<DemonstrationWordView>(widthLimit, View("DemonstrationWordView"), false);
            so.FindProperty("selectWordScreen").objectReferenceValue = Replace<SelectWordView>(widthLimit, View("SelectWordView"), false);
            so.FindProperty("matchWordScreen").objectReferenceValue = Replace<MatchWordsView>(widthLimit, View("MatchWordsView"), false);
            so.FindProperty("sentenceSelectWordScreen").objectReferenceValue = Replace<SentenceSelectWordView>(widthLimit, View("SentenceSelectWordView"), false);

            DesignViewsBuilder.ConfigureScaler(Root("BackUI").GetComponent<CanvasScaler>());
            so.ApplyModifiedPropertiesWithoutUndo();

            if (oldMain != null)
            {
                Object.DestroyImmediate(oldMain);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>Instantiates the view prefab under <paramref name="parent"/> in place of the old view of the same type.</summary>
        private static T Replace<T>(Transform parent, GameObject prefab, bool active = true) where T : Component
        {
            var existing = parent.Cast<Transform>().FirstOrDefault(t => PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject) == prefab);
            if (existing != null)
            {
                return existing.GetComponent<T>();
            }

            var old = parent.Cast<Transform>().FirstOrDefault(t => t.GetComponent<T>() != null);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = prefab.name;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            if (old != null)
            {
                go.transform.SetSiblingIndex(old.GetSiblingIndex());
                go.SetActive(old.gameObject.activeSelf);
                Object.DestroyImmediate(old.gameObject);
            }
            else
            {
                go.SetActive(active);
            }

            return go.GetComponent<T>();
        }

        private static void SetupBootstrap()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);
            var manager = Object.FindFirstObjectByType<PopupManager>(FindObjectsInactive.Include);
            var so = new SerializedObject(manager);
            so.FindProperty("popupPrefab").objectReferenceValue = View("Popup/PopupView").GetComponent<PopupView>();
            so.FindProperty("loadingUiPrefab").objectReferenceValue = View("LoadingScreen").GetComponent<LoadingUiView>();
            so.ApplyModifiedPropertiesWithoutUndo();

            var canvas = manager.GetComponentInParent<Canvas>(true);
            DesignViewsBuilder.ConfigureScaler(canvas.GetComponent<CanvasScaler>());

            ReplaceLogin();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        [MenuItem("Chang/Design System/Use Login View In Bootstrap", false, 22)]
        public static void SetupLogin()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);
            ReplaceLogin();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[{nameof(DesignViewsSceneSetup)}] [{nameof(SetupLogin)}] Bootstrap uses the redesigned login view");
        }

        /// <summary>The login view in place of the old LoginScreen (ProjectInstaller finds the LogInView in the scene).</summary>
        private static void ReplaceLogin()
        {
            var old = Object.FindFirstObjectByType<DMZ.Legacy.LoginScreen.LogInView>(FindObjectsInactive.Include);
            if (old == null)
            {
                Debug.LogError($"[{nameof(DesignViewsSceneSetup)}] [{nameof(ReplaceLogin)}] No LogInView in the scene");
                return;
            }

            var canvas = old.GetComponentInParent<Canvas>(true);
            DesignViewsBuilder.ConfigureScaler(canvas.GetComponent<CanvasScaler>());

            var prefab = View("LoginView");
            if (PrefabUtility.GetCorrespondingObjectFromSource(old.gameObject) == prefab)
            {
                return;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, old.transform.parent);
            go.name = prefab.name;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.transform.SetSiblingIndex(old.transform.GetSiblingIndex());
            go.SetActive(old.gameObject.activeSelf);

            // the scene switches the login background with the view
            var view = new SerializedObject(go.GetComponent<DMZ.Legacy.LoginScreen.LogInView>());
            view.CopyFromSerializedProperty(new SerializedObject(old).FindProperty("_onEnable"));
            view.ApplyModifiedPropertiesWithoutUndo();

            Object.DestroyImmediate(old.gameObject);
        }
    }
}

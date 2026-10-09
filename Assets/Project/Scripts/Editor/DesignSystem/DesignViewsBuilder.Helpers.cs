using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chang.UI;
using Chang.UI.DesignSystem;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chang.Editor.DesignSystem
{
    /// <summary>Small building blocks for <see cref="DesignViewsBuilder"/>. Every helper is idempotent: running the build again updates objects instead of adding new ones.</summary>
    public static partial class DesignViewsBuilder
    {
        private const char PathSeparator = '|';

        private static GameObject LoadPrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Prefab not found: {path}. Import the design first (Chang/Design System/Import from Penpot JSON).");
            }

            return prefab;
        }

        private static GameObject Component(string name) => LoadPrefab($"{PenpotImporter.ComponentsRoot}/{name}.prefab");

        /// <summary>Size of a component as drawn in Penpot (its prefab root).</summary>
        private static Vector2 DesignSize(GameObject prefab) => ((RectTransform)prefab.transform).sizeDelta;

        private static GameObject Screen(string name) => LoadPrefab($"{PenpotImporter.ScreensRoot}/{name}.prefab");

        /// <summary>Builds a prefab variant of <paramref name="basePrefab"/> at <paramref name="path"/>, or updates it in place when it exists.</summary>
        private static GameObject BuildVariant(GameObject basePrefab, string path, Action<GameObject> build)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && PrefabUtility.GetCorrespondingObjectFromSource(existing) == basePrefab)
            {
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    build(contents);
                    return PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab, scene);
                go.name = Path.GetFileNameWithoutExtension(path);
                build(go);
                return PrefabUtility.SaveAsPrefabAsset(go, path);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        /// <summary>Builds a plain prefab (not a variant) at <paramref name="path"/>, or updates it in place when it exists.</summary>
        private static GameObject BuildPrefab(string path, Action<GameObject> build)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    build(contents);
                    return PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var go = new GameObject(Path.GetFileNameWithoutExtension(path), typeof(RectTransform)) { layer = UiLayer };
                SceneManagerMove(go, scene);
                build(go);
                return PrefabUtility.SaveAsPrefabAsset(go, path);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void SceneManagerMove(GameObject go, UnityEngine.SceneManagement.Scene scene)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
        }

        private static int UiLayer => LayerMask.NameToLayer("UI");

        private static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Child by path of exact names separated by '|' (names may contain '/').</summary>
        private static Transform Q(Transform root, string path)
        {
            var current = root;
            foreach (var name in path.Split(PathSeparator))
            {
                var next = current.Cast<Transform>().FirstOrDefault(c => c.name == name);
                if (next == null)
                {
                    var children = string.Join(", ", current.Cast<Transform>().Select(c => c.name));
                    throw new InvalidOperationException($"'{name}' not found under '{current.name}' (path '{path}'). Children: {children}");
                }

                current = next;
            }

            return current;
        }

        private static Transform Q(GameObject root, string path) => Q(root.transform, path);

        private static T Q<T>(GameObject root, string path) where T : Component => Q(root.transform, path).GetComponent<T>();

        private static IEnumerable<Transform> Children(Transform parent, string namePrefix) =>
            parent.Cast<Transform>().Where(c => c.name.StartsWith(namePrefix));

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static T GetOrAdd<T>(Transform t) where T : Component => GetOrAdd<T>(t.gameObject);

        private static void Hide(Transform t)
        {
            if (t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(false);
            }
        }

        private static void Hide(GameObject root, string path) => Hide(Q(root, path));

        /// <summary>Shows a part an earlier build hid.</summary>
        private static void Show(Transform t)
        {
            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
            }
        }

        private static void HideAll(IEnumerable<Transform> items)
        {
            foreach (var t in items.ToList())
            {
                Hide(t);
            }
        }

        /// <summary>Finds a direct child by name or creates an empty UI object.</summary>
        private static RectTransform Child(Transform parent, string name, int siblingIndex = -1)
        {
            var t = parent.Cast<Transform>().FirstOrDefault(c => c.name == name);
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
                go.transform.SetParent(parent, false);
                t = go.transform;
            }

            if (siblingIndex >= 0 && t.GetSiblingIndex() != siblingIndex)
            {
                t.SetSiblingIndex(siblingIndex);
            }

            return (RectTransform)t;
        }

        /// <summary>Finds a direct child by name or instantiates the prefab there (nested prefab).</summary>
        private static RectTransform Instance(GameObject prefab, Transform parent, string name)
        {
            var t = parent.Cast<Transform>().FirstOrDefault(c => c.name == name);
            if (t == null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                go.name = name;
                t = go.transform;
            }

            return (RectTransform)t;
        }

        private static void Stretch(RectTransform rt, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void AnchorTop(RectTransform rt, float height, float top, float width = -1f)
        {
            rt.anchorMin = new Vector2(width < 0f ? 0f : 0.5f, 1f);
            rt.anchorMax = new Vector2(width < 0f ? 1f : 0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width < 0f ? 0f : width, height);
            rt.anchoredPosition = new Vector2(0f, -top);
        }

        private static void AnchorBottom(RectTransform rt, float height, float bottom, float width = -1f)
        {
            rt.anchorMin = new Vector2(width < 0f ? 0f : 0.5f, 0f);
            rt.anchorMax = new Vector2(width < 0f ? 1f : 0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(width < 0f ? 0f : width, height);
            rt.anchoredPosition = new Vector2(0f, bottom);
        }

        /// <summary>Assigns serialized fields by name, so view classes don't need editor-only setters.</summary>
        private static void Set(Object target, params (string field, object value)[] values)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var p = so.FindProperty(field);
                if (p == null)
                {
                    throw new InvalidOperationException($"{target.GetType().Name} has no serialized field '{field}'");
                }

                switch (value)
                {
                    case null:
                        p.objectReferenceValue = null;
                        break;
                    case Object o:
                        p.objectReferenceValue = o;
                        break;
                    case string s:
                        p.stringValue = s;
                        break;
                    case bool b:
                        p.boolValue = b;
                        break;
                    case int i:
                        p.intValue = i;
                        break;
                    case float f:
                        p.floatValue = f;
                        break;
                    case Color c:
                        p.colorValue = c;
                        break;
                    case Gradient g:
                        p.gradientValue = g;
                        break;
                    case float[] floats:
                        p.arraySize = floats.Length;
                        for (var k = 0; k < floats.Length; k++)
                        {
                            p.GetArrayElementAtIndex(k).floatValue = floats[k];
                        }

                        break;
                    case IList<Object> objects:
                        p.arraySize = objects.Count;
                        for (var k = 0; k < objects.Count; k++)
                        {
                            p.GetArrayElementAtIndex(k).objectReferenceValue = objects[k];
                        }

                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported value type {value.GetType()} for '{field}'");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static List<Object> Objects<T>(IEnumerable<T> items) where T : Object => items.Cast<Object>().ToList();

        /// <summary>Sets up a <see cref="DesignStates"/> with one state per variant prefab of a component group.</summary>
        private static DesignStates States(GameObject target, string group, IEnumerable<string> variants, string initial,
            IEnumerable<TMP_Text> stateTexts = null, IEnumerable<GameObject> codeVisibility = null)
        {
            var states = GetOrAdd<DesignStates>(target);
            var so = new SerializedObject(states);
            var list = so.FindProperty("_states");
            var names = variants.ToList();
            list.arraySize = names.Count;
            for (var i = 0; i < names.Count; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Name").stringValue = names[i];
                element.FindPropertyRelative("Prefab").objectReferenceValue = Component($"{group}/{names[i]}");
            }

            so.FindProperty("_initialState").stringValue = initial;
            so.ApplyModifiedPropertiesWithoutUndo();
            Set(states, ("_stateTexts", Objects(stateTexts ?? Enumerable.Empty<TMP_Text>())),
                ("_codeVisibility", Objects(codeVisibility ?? Enumerable.Empty<GameObject>())));
            return states;
        }

        private static Button ButtonOn(Transform t)
        {
            var button = GetOrAdd<Button>(t);
            var bg = t.Find(PenpotUiBuilder.BackgroundName);
            if (bg != null)
            {
                button.targetGraphic = bg.GetComponent<Graphic>();
            }

            // without a background (or with it switched off) nothing would catch the touch
            if (t.GetComponent<Graphic>() == null)
            {
                var hitArea = GetOrAdd<Image>(t);
                hitArea.color = new Color(0f, 0f, 0f, 0f);
                hitArea.raycastTarget = true;
            }

            return button;
        }

        private static Toggle ToggleOn(Transform t)
        {
            var toggle = GetOrAdd<Toggle>(t);
            toggle.transition = Selectable.Transition.None;
            toggle.graphic = null;
            var bg = t.Find(PenpotUiBuilder.BackgroundName);
            if (bg != null)
            {
                toggle.targetGraphic = bg.GetComponent<Graphic>();
            }

            // the background can be missing or hidden by a state (TabBar: only the selected tab has one),
            // a transparent image on the toggle itself catches touches in any state
            if (t.GetComponent<Graphic>() == null)
            {
                var hitArea = GetOrAdd<Image>(t);
                hitArea.color = new Color(0f, 0f, 0f, 0f);
                hitArea.raycastTarget = true;
            }

            return toggle;
        }

        private static void Localize(Transform text, string key)
        {
            var localized = GetOrAdd<LocalizedTMPText>(text);
            Set(localized, ("_localizationKey", key));
        }

        /// <summary>Localizes a design text, <paramref name="english"/> replaces the design sample and is shown until the key is in the sheet.</summary>
        private static void Localize(Transform text, string key, string english)
        {
            text.GetComponent<TMP_Text>().text = english;
            Localize(text, key);
        }

        /// <summary>A bar (the design's Bar/Value) filled by code.</summary>
        private static LoadingFillBar FillBar(Transform bar)
        {
            var value = Q(bar, "Value");
            Show(value);
            var fillBar = GetOrAdd<LoadingFillBar>(bar);
            Set(fillBar, ("_fill", (RectTransform)value));
            return fillBar;
        }

        /// <summary>A full-size transparent Image that catches touches.</summary>
        private static Image Blocker(Transform parent, string name, int siblingIndex = -1)
        {
            var rt = Child(parent, name, siblingIndex);
            Stretch(rt);
            var image = GetOrAdd<Image>(rt);
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
            return image;
        }

        /// <summary>A sprite Image put on top of a design placeholder (the illustration) for runtime content.</summary>
        private static Image ContentImage(Transform frame, string placeholder)
        {
            if (!string.IsNullOrEmpty(placeholder))
            {
                Hide(Q(frame, placeholder));
            }

            var rt = Child(frame, "ContentImage");
            Stretch(rt, 8f, 8f, 8f, 8f);
            GetOrAdd<LayoutElement>(rt).ignoreLayout = true;
            var image = GetOrAdd<Image>(rt);
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Makes a design screen scrollable: the root is the viewport, Content grows with its children.</summary>
        private static ScrollRect Scroll(GameObject root, RectTransform content, float bottomPadding)
        {
            GetOrAdd<RectMask2D>(root);
            var scroll = GetOrAdd<ScrollRect>(root);
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = (RectTransform)root.transform;
            scroll.content = content;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            // Content anchored to the top, its height follows the layout.
            var top = TopOffset(content);
            content.anchorMin = new Vector2(content.anchorMin.x, 1f);
            content.anchorMax = new Vector2(content.anchorMax.x, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, -top);
            var fitter = GetOrAdd<ContentSizeFitter>(content);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout = content.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(layout.padding.left, layout.padding.right, layout.padding.top, Mathf.RoundToInt(bottomPadding));
            }

            return scroll;
        }

        /// <summary>The design has a 44 pt phone status bar on top; in the app the safe area takes its place.</summary>
        private const float StatusBarHeight = 44f;

        /// <summary>Distance from the top in the app, read from the generated screen (not from the variant), so the build can run again.</summary>
        private static float TopOffset(RectTransform rt)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(rt);
            if (source != null)
            {
                rt = source;
            }

            float top;
            if (Mathf.Approximately(rt.anchorMin.y, rt.anchorMax.y) && Mathf.Approximately(rt.anchorMax.y, 1f))
            {
                top = -(rt.anchoredPosition.y + rt.sizeDelta.y * (1f - rt.pivot.y));
            }
            else
            {
                top = -rt.offsetMax.y;
            }

            return Mathf.Max(8f, top - StatusBarHeight + 8f);
        }

        /// <summary>Moves a top-anchored screen part up by the design status bar height.</summary>
        private static void RemoveStatusBarOffset(RectTransform rt)
        {
            var top = TopOffset(rt);
            if (Mathf.Approximately(rt.anchorMin.y, rt.anchorMax.y) && Mathf.Approximately(rt.anchorMax.y, 1f))
            {
                rt.pivot = new Vector2(rt.pivot.x, 1f);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -top);
            }
            else
            {
                rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
            }
        }
    }
}

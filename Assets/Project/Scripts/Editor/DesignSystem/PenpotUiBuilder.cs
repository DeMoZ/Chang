using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Chang.UI;
using Chang.UI.DesignSystem;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.ProceduralImage;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Applies Penpot shapes to uGUI GameObjects.
    /// Works both on fresh objects and on existing ones (prefab contents, prefab instances, variants):
    /// children are matched by <see cref="DesignNode"/> id through the Penpot shapeRef chain,
    /// so applying the same values to a prefab instance creates no overrides and re-import keeps hand-made additions.
    /// </summary>
    public class PenpotUiBuilder
    {
        public const string ShadowName = "#shadow";
        public const string BackgroundName = "#bg";
        public const string StrokeName = "#stroke";
        public const string ImageName = "#img";
        public const string SpacerPrefix = "#spacer";

        public class Options
        {
            /// <summary>Root shape of the prefab being built.</summary>
            public JObject Root;

            /// <summary>Screens stretch to the canvas and anchor their direct children to the nearest edge.</summary>
            public bool IsScreen;

            /// <summary>Write shape ids into DesignNode ids (component prefabs and variants). Off for screens to avoid overrides.</summary>
            public bool AssignIds;

            /// <summary>Variant shape id → base component shape id, for building variants on top of their base prefab.</summary>
            public Dictionary<string, string> Alias = new();
        }

        private readonly PenpotDocument _doc;
        private readonly PenpotSvg _svg;
        private readonly DesignTheme _theme;
        private readonly Dictionary<string, string> _colorTokens;
        private readonly Dictionary<string, TMP_FontAsset> _fonts;
        private readonly Dictionary<string, Sprite> _sprites;
        private readonly Dictionary<string, PenpotSvg.Result> _vectors;
        private readonly Dictionary<string, GameObject> _componentPrefabs;
        private Options _options;

        public PenpotUiBuilder(PenpotDocument doc, PenpotSvg svg, DesignTheme theme, Dictionary<string, string> colorTokens,
            Dictionary<string, TMP_FontAsset> fonts, Dictionary<string, PenpotSvg.Result> vectors, Dictionary<string, Sprite> sprites,
            Dictionary<string, GameObject> componentPrefabs)
        {
            _doc = doc;
            _svg = svg;
            _theme = theme;
            _colorTokens = colorTokens;
            _fonts = fonts;
            _vectors = vectors;
            _sprites = sprites;
            _componentPrefabs = componentPrefabs;
        }

        /// <summary>Applies the root shape and its whole subtree to <paramref name="go"/>.</summary>
        public void BuildRoot(JObject root, GameObject go, Options options)
        {
            _options = options;
            Apply(root, go, null);
        }

        // ---- node -------------------------------------------------------------------------------------

        private GameObject Create(JObject s, Transform parent)
        {
            var compId = PenpotDocument.ComponentId(s);
            GameObject go;
            if (compId != null && !PenpotDocument.IsMainInstance(s) && _componentPrefabs.TryGetValue(compId, out var prefab) && prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            }
            else
            {
                go = new GameObject(PenpotDocument.Name(s), typeof(RectTransform));
                go.layer = LayerMask.NameToLayer("UI");
                go.transform.SetParent(parent, false);
                go.AddComponent<DesignNode>().Id = PenpotDocument.Id(s);
            }

            return go;
        }

        private void Apply(JObject s, GameObject go, JObject parent)
        {
            var name = PenpotDocument.Name(s);
            if (go.name != name)
            {
                go.name = name;
            }

            if (_options.AssignIds)
            {
                var node = GetOrAdd<DesignNode>(go);
                if (node.Id != PenpotDocument.Id(s))
                {
                    node.Id = PenpotDocument.Id(s);
                }
            }

            // StatusBar is a phone mock-up in the design, the real one belongs to the OS.
            var active = !PenpotDocument.Hidden(s) && !(parent == _options.Root && _options.IsScreen && name == "StatusBar");
            if (go.activeSelf != active)
            {
                go.SetActive(active);
            }

            var type = PenpotDocument.Type(s);
            var isVector = _svg.IsVector(s);

            ApplyRect(s, go, parent);
            ApplyLayoutElement(s, go, parent, isVector);

            if (isVector)
            {
                ApplyVector(s, go);
                return;
            }

            switch (type)
            {
                case "text":
                    ApplyText(s, go, parent);
                    return;
                case "rect":
                case "circle":
                    ApplyShapeFill(s, go);
                    ApplyStroke(s, go);
                    return;
                case "frame":
                    ApplyFrameVisuals(s, go);
                    ApplyLayoutGroup(s, go);
                    break;
            }

            ApplyOpacityGroup(s, go);
            ApplyChildren(s, go);

            if (s == _options.Root && !_options.IsScreen)
            {
                AddInteraction(s, go);
            }
        }

        private void ApplyChildren(JObject s, GameObject go)
        {
            var flex = PenpotDocument.IsFlex(s);
            var shapes = _doc.Children(s).ToList();
            if (flex)
            {
                // Penpot keeps layers bottom→top; flex lays them out top layer first.
                shapes.Reverse();
            }

            var used = new HashSet<GameObject>();
            var ordered = new List<GameObject>();
            foreach (var c in shapes)
            {
                var child = FindChild(go.transform, c, used) ?? Create(c, go.transform);
                used.Add(child);
                ordered.Add(child);
                Apply(c, child, s);
            }

            // Spacers emulate justify-content: space-between.
            var spacers = new List<GameObject>();
            if (flex && (string)s["layoutJustifyContent"] == "space-between")
            {
                var visible = shapes.Count(c => !PenpotDocument.Hidden(c));
                for (var i = 0; i < visible - 1; i++)
                {
                    var spacer = GetOrCreateChild(go, SpacerPrefix + i);
                    var le = GetOrAdd<LayoutElement>(spacer);
                    var row = (string)s["layoutFlexDir"] is "row" or "row-reverse";
                    SetIfDiff(le.flexibleWidth, row ? 1f : 0f, v => le.flexibleWidth = v);
                    SetIfDiff(le.flexibleHeight, row ? 0f : 1f, v => le.flexibleHeight = v);
                    used.Add(spacer);
                    spacers.Add(spacer);
                }
            }

            // Order: helpers, then shapes (spacers between visible shapes). Prefab instance children keep the prefab order.
            var index = 0;
            foreach (var helper in new[] { ShadowName, BackgroundName, StrokeName })
            {
                var t = go.transform.Find(helper);
                if (t != null)
                {
                    SetSibling(t, index++);
                }
            }

            var spacerIndex = 0;
            for (var i = 0; i < ordered.Count; i++)
            {
                SetSibling(ordered[i].transform, index++);
                if (spacerIndex < spacers.Count && !PenpotDocument.Hidden(shapes[i]) && shapes.Skip(i + 1).Any(c => !PenpotDocument.Hidden(c)))
                {
                    SetSibling(spacers[spacerIndex++].transform, index++);
                }
            }

            // Stale generated children (removed from the design) are deleted; hand-made children are kept.
            foreach (Transform t in go.transform.Cast<Transform>().ToList())
            {
                if (used.Contains(t.gameObject) || t.name.StartsWith("#") && t.name is ShadowName or BackgroundName or StrokeName or ImageName)
                {
                    continue;
                }

                var generated = t.GetComponent<DesignNode>() != null || t.name.StartsWith(SpacerPrefix);
                if (!generated)
                {
                    continue;
                }

                if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject) && !PrefabUtility.IsAddedGameObjectOverride(t.gameObject))
                {
                    t.gameObject.SetActive(false);
                }
                else
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private GameObject FindChild(Transform parent, JObject shape, HashSet<GameObject> used)
        {
            var chain = _doc.RefChain(shape);
            if (_options.Alias.TryGetValue(PenpotDocument.Id(shape), out var alias))
            {
                chain.Insert(1, alias);
            }

            foreach (var id in chain)
            {
                foreach (Transform t in parent)
                {
                    if (used.Contains(t.gameObject))
                    {
                        continue;
                    }

                    var node = t.GetComponent<DesignNode>();
                    if (node != null && node.Id == id)
                    {
                        return t.gameObject;
                    }
                }
            }

            return null;
        }

        private static void SetSibling(Transform t, int index)
        {
            if (t.GetSiblingIndex() == index)
            {
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject) && !PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject) &&
                !PrefabUtility.IsAddedGameObjectOverride(t.gameObject))
            {
                return;
            }

            if (t.parent != null && PrefabUtility.IsPartOfPrefabInstance(t.parent.gameObject) && PrefabUtility.IsPartOfPrefabInstance(t.gameObject) &&
                !PrefabUtility.IsAddedGameObjectOverride(t.gameObject) && !PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject))
            {
                return;
            }

            t.SetSiblingIndex(Mathf.Min(index, t.parent.childCount - 1));
        }

        // ---- rect & layout ----------------------------------------------------------------------------

        private static bool IsAbsolute(JObject s) => (bool?)s["layoutItemAbsolute"] ?? false;

        private static Vector2 Size(JObject s)
        {
            if (Mathf.Abs(PenpotDocument.Rotation(s)) > 0.01f)
            {
                return new Vector2(PenpotDocument.Num(s["width"]), PenpotDocument.Num(s["height"]));
            }

            return PenpotDocument.SelRect(s).size;
        }

        private void ApplyRect(JObject s, GameObject go, JObject parent)
        {
            var rt = (RectTransform)go.transform;
            var size = Size(s);
            var center = PenpotDocument.Center(s);
            var rot = PenpotDocument.Rotation(s);

            if (parent == null)
            {
                if (_options.IsScreen)
                {
                    SetRect(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                }
                else
                {
                    var half = new Vector2(0.5f, 0.5f);
                    SetRect(rt, half, half, half, Vector2.zero, size);
                }

                return;
            }

            var pr = PenpotDocument.SelRect(parent);
            var inFlex = PenpotDocument.IsFlex(parent) && !IsAbsolute(s);
            if (inFlex)
            {
                // Position is driven by the layout group; keep the size as a starting point.
                SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), rt.anchoredPosition, size);
            }
            else if (_options.IsScreen && parent == _options.Root)
            {
                ApplyScreenAnchors(rt, PenpotDocument.SelRect(s), pr);
            }
            else if (PenpotDocument.Type(s) == "text" && (string)s["growType"] == "auto-width")
            {
                // Auto-width text grows from its alignment side.
                var r = PenpotDocument.SelRect(s);
                var align = TextAlign(s);
                var px = align == "center" ? 0.5f : align == "right" ? 1f : 0f;
                var pos = new Vector2(Mathf.Lerp(r.xMin, r.xMax, px) - pr.x, -(r.yMin - pr.y));
                SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(px, 1f), pos, size);
            }
            else
            {
                var pos = new Vector2(center.x - pr.x, -(center.y - pr.y));
                SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), pos, size);
            }

            var euler = new Vector3(0f, 0f, -rot);
            if (rt.localEulerAngles != euler && (Mathf.Abs(rot) > 0.01f || rt.localEulerAngles != Vector3.zero))
            {
                rt.localEulerAngles = euler;
            }
        }

        /// <summary>Direct children of a screen stick to the nearest edge so the 540×1080 design adapts to other aspect ratios.</summary>
        private static void ApplyScreenAnchors(RectTransform rt, Rect r, Rect pr)
        {
            Vector2 aMin, aMax, pos, size;
            if (r.width >= pr.width * 0.9f)
            {
                var left = r.xMin - pr.xMin;
                var right = pr.xMax - r.xMax;
                aMin = new Vector2(0f, 0f);
                aMax = new Vector2(1f, 0f);
                pos = new Vector2((left - right) * 0.5f, 0f);
                size = new Vector2(-(left + right), 0f);
            }
            else
            {
                aMin = new Vector2(0.5f, 0f);
                aMax = new Vector2(0.5f, 0f);
                pos = new Vector2(r.center.x - pr.center.x, 0f);
                size = new Vector2(r.width, 0f);
            }

            var top = r.yMin - pr.yMin;
            var bottom = pr.yMax - r.yMax;
            if (r.height >= pr.height * 0.5f)
            {
                aMin.y = 0f;
                aMax.y = 1f;
                pos.y = (bottom - top) * 0.5f;
                size.y = -(top + bottom);
            }
            else if (r.yMin >= pr.yMin + pr.height * 0.6f)
            {
                aMin.y = aMax.y = 0f;
                pos.y = bottom + r.height * 0.5f;
                size.y = r.height;
            }
            else
            {
                aMin.y = aMax.y = 1f;
                pos.y = -(top + r.height * 0.5f);
                size.y = r.height;
            }

            SetRect(rt, aMin, aMax, new Vector2(0.5f, 0.5f), pos, size);
        }

        private static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            pos = Round(pos);
            size = Round(size);
            if (rt.anchorMin != aMin) rt.anchorMin = aMin;
            if (rt.anchorMax != aMax) rt.anchorMax = aMax;
            if (rt.pivot != pivot) rt.pivot = pivot;
            if (rt.sizeDelta != size) rt.sizeDelta = size;
            if (rt.anchoredPosition != pos) rt.anchoredPosition = pos;
        }

        private static Vector2 Round(Vector2 v) => new(Mathf.Round(v.x * 100f) / 100f, Mathf.Round(v.y * 100f) / 100f);

        private void ApplyLayoutElement(JObject s, GameObject go, JObject parent, bool isVector)
        {
            if (parent == null || !PenpotDocument.IsFlex(parent))
            {
                return;
            }

            var le = GetOrAdd<LayoutElement>(go);
            var abs = IsAbsolute(s);
            SetIfDiff(le.ignoreLayout, abs, v => le.ignoreLayout = v);
            if (abs)
            {
                return;
            }

            var size = Size(s);
            var hs = (string)s["layoutItemHSizing"];
            var vs = (string)s["layoutItemVSizing"];
            if (PenpotDocument.Type(s) == "text")
            {
                var grow = (string)s["growType"];
                hs ??= grow == "auto-width" ? "auto" : "fix";
                vs ??= grow is "auto-width" or "auto-height" ? "auto" : "fix";
                if (hs == "fix" && grow == "auto-width") hs = "auto";
                if (vs == "fix" && grow is "auto-width" or "auto-height") vs = "auto";

                // Single-line texts keep their width when the row overflows, like in Penpot.
                if (hs == "auto" && grow == "auto-width")
                {
                    GetOrAdd<LayoutNoShrink>(go);
                }
            }
            else if (isVector || PenpotDocument.Type(s) != "frame" || !PenpotDocument.IsFlex(s))
            {
                // Only flex frames and texts can hug their content in uGUI.
                if (hs == "auto") hs = "fix";
                if (vs == "auto") vs = "fix";
            }

            ApplySizing(hs, size.x, v => le.minWidth = v, () => le.minWidth, v => le.preferredWidth = v, () => le.preferredWidth,
                v => le.flexibleWidth = v, () => le.flexibleWidth);
            ApplySizing(vs, size.y, v => le.minHeight = v, () => le.minHeight, v => le.preferredHeight = v, () => le.preferredHeight,
                v => le.flexibleHeight = v, () => le.flexibleHeight);
        }

        private static void ApplySizing(string sizing, float size, System.Action<float> setMin, System.Func<float> getMin,
            System.Action<float> setPref, System.Func<float> getPref, System.Action<float> setFlex, System.Func<float> getFlex)
        {
            float min, pref, flex;
            switch (sizing)
            {
                case "fill":
                    min = 0f;
                    pref = 0f;
                    flex = 1f;
                    break;
                case "auto":
                    min = -1f;
                    pref = -1f;
                    flex = -1f;
                    break;
                default:
                    size = Mathf.Round(size * 100f) / 100f;
                    min = size;
                    pref = size;
                    flex = -1f;
                    break;
            }

            if (!Mathf.Approximately(getMin(), min)) setMin(min);
            if (!Mathf.Approximately(getPref(), pref)) setPref(pref);
            if (!Mathf.Approximately(getFlex(), flex)) setFlex(flex);
        }

        private void ApplyLayoutGroup(JObject s, GameObject go)
        {
            if (!PenpotDocument.IsFlex(s))
            {
                return;
            }

            var dir = (string)s["layoutFlexDir"] ?? "row";
            var row = dir is "row" or "row-reverse";
            var wrap = (string)s["layoutWrapType"] == "wrap";
            var gap = s["layoutGap"];
            var rowGap = PenpotDocument.Num(gap?["rowGap"]);
            var columnGap = PenpotDocument.Num(gap?["columnGap"]);
            var pad = s["layoutPadding"];
            var padding = new RectOffset(
                Mathf.RoundToInt(PenpotDocument.Num(pad?["p4"])),
                Mathf.RoundToInt(PenpotDocument.Num(pad?["p2"])),
                Mathf.RoundToInt(PenpotDocument.Num(pad?["p1"])),
                Mathf.RoundToInt(PenpotDocument.Num(pad?["p3"])));
            var jc = (string)s["layoutJustifyContent"] ?? "start";
            var ai = (string)s["layoutAlignItems"] ?? "start";
            var alignment = Alignment(row, jc, ai);

            var groupType = wrap ? typeof(FlowLayoutGroup) : row ? typeof(HorizontalLayoutGroup) : typeof(VerticalLayoutGroup);
            if (!RemoveOtherLayoutGroups(go, groupType))
            {
                return;
            }

            if (wrap)
            {
                var flow = GetOrAdd<FlowLayoutGroup>(go);
                if (!SamePadding(flow.padding, padding)) flow.padding = padding;
                if (flow.childAlignment != alignment) flow.childAlignment = alignment;
                SetIfDiff(flow.HorizontalSpacing, columnGap, v => flow.HorizontalSpacing = v);
                SetIfDiff(flow.VerticalSpacing, rowGap, v => flow.VerticalSpacing = v);
                return;
            }

            HorizontalOrVerticalLayoutGroup group = row ? GetOrAdd<HorizontalLayoutGroup>(go) : GetOrAdd<VerticalLayoutGroup>(go);
            if (group == null)
            {
                return;
            }

            var spacing = row ? columnGap : rowGap;
            if (jc == "space-between")
            {
                spacing *= 0.5f;
            }

            if (!SamePadding(group.padding, padding)) group.padding = padding;
            if (group.childAlignment != alignment) group.childAlignment = alignment;
            SetIfDiff(group.spacing, spacing, v => group.spacing = v);
            if (!group.childControlWidth) group.childControlWidth = true;
            if (!group.childControlHeight) group.childControlHeight = true;
            if (group.childForceExpandWidth) group.childForceExpandWidth = false;
            if (group.childForceExpandHeight) group.childForceExpandHeight = false;
            var reverse = dir.EndsWith("-reverse");
            if (group.reverseArrangement != reverse) group.reverseArrangement = reverse;
        }

        private static bool SamePadding(RectOffset a, RectOffset b) =>
            a.left == b.left && a.right == b.right && a.top == b.top && a.bottom == b.bottom;

        private static TextAnchor Alignment(bool row, string jc, string ai)
        {
            int Main(string v) => v switch { "center" => 1, "end" => 2, "space-around" => 1, "space-evenly" => 1, _ => 0 };
            int Cross(string v) => v switch { "center" => 1, "end" => 2, "stretch" => 1, _ => 0 };
            var h = row ? Main(jc) : Cross(ai);
            var v = row ? Cross(ai) : Main(jc);
            return (TextAnchor)(v * 3 + h);
        }

        // ---- visuals ----------------------------------------------------------------------------------

        private void ApplyFrameVisuals(JObject s, GameObject go)
        {
            var radius = PenpotDocument.Radius(s);
            var shadow = PenpotDocument.VisibleShadows(s).FirstOrDefault(sh => (string)sh["style"] == "drop-shadow");
            SetHelperActive(go, ShadowName, shadow != null);
            if (shadow != null)
            {
                var sg = GetOrCreateChild(go, ShadowName);
                Stretch(sg, Vector2.zero, Vector2.zero);
                var spread = PenpotDocument.Num(shadow["spread"]);
                var blur = PenpotDocument.Num(shadow["blur"]);
                var offset = new Vector2(PenpotDocument.Num(shadow["offsetX"]), -PenpotDocument.Num(shadow["offsetY"]));
                Stretch(sg, offset - Vector2.one * spread, offset + Vector2.one * spread);
                var img = SetupProcedural(sg, radius + Vector4.one * spread, 0f, Mathf.Max(1f, blur * 1.25f));
                var c = shadow["color"];
                ApplyColor(img, (string)c?["color"], PenpotDocument.Num(c?["opacity"], 1f));
                img.raycastTarget = false;
            }

            var fill = PenpotDocument.FirstVisibleFill(s);
            SetHelperActive(go, BackgroundName, fill != null);
            if (fill != null)
            {
                var bg = GetOrCreateChild(go, BackgroundName);
                Stretch(bg, Vector2.zero, Vector2.zero);
                var img = SetupProcedural(bg, radius, 0f, 1f);
                ApplyFill(img, fill, 1f);
            }

            ApplyStroke(s, go);

            var clip = ((bool?)s["showContent"] ?? true) == false;
            var mask = go.GetComponent<RectMask2D>();
            if (clip && mask == null)
            {
                go.AddComponent<RectMask2D>();
            }
        }

        private void ApplyShapeFill(JObject s, GameObject go)
        {
            var radius = PenpotDocument.Radius(s);
            if (PenpotDocument.Type(s) == "circle")
            {
                var size = Size(s);
                radius = Vector4.one * Mathf.Min(size.x, size.y) * 0.5f;
            }

            var fill = PenpotDocument.FirstVisibleFill(s);
            var img = SetupProcedural(go, radius, 0f, 1f);
            if (fill != null)
            {
                ApplyFill(img, fill, PenpotDocument.Opacity(s));
            }
            else
            {
                img.color = Color.clear;
                RemoveComponent<ThemeColor>(go);
            }

            img.raycastTarget = false;
        }

        private void ApplyStroke(JObject s, GameObject go)
        {
            var stroke = PenpotDocument.VisibleStrokes(s).FirstOrDefault();
            SetHelperActive(go, StrokeName, stroke != null);
            if (stroke == null)
            {
                return;
            }

            var width = PenpotDocument.Num(stroke["strokeWidth"], 1f);
            var alignment = (string)stroke["strokeAlignment"] ?? "inner";
            var expand = alignment switch { "outer" => width, "center" => width * 0.5f, _ => 0f };
            var radius = PenpotDocument.Radius(s);
            var size = Size(s);
            if (PenpotDocument.Type(s) == "circle")
            {
                radius = Vector4.one * Mathf.Min(size.x, size.y) * 0.5f;
            }

            var hex = (string)stroke["strokeColor"];
            var opacity = PenpotDocument.Num(stroke["strokeOpacity"], 1f);
            var sg = GetOrCreateChild(go, StrokeName);

            if ((string)stroke["strokeStyle"] is "dashed" or "dotted" && _vectors.TryGetValue(PenpotDocument.Id(s) + StrokeName, out var dashed) &&
                _sprites.TryGetValue(dashed.Hash, out var sprite))
            {
                var pad = -dashed.Bounds.x;
                Stretch(sg, -Vector2.one * pad, Vector2.one * pad);
                var image = GetOrAdd<Image>(sg);
                if (image == null)
                {
                    return;
                }

                if (image.sprite != sprite) image.sprite = sprite;
                image.raycastTarget = false;
                ApplyColor(image, hex, 1f);
                return;
            }

            Stretch(sg, -Vector2.one * expand, Vector2.one * expand);
            var img = SetupProcedural(sg, radius + Vector4.one * expand, width, 1f);
            ApplyColor(img, hex, opacity);
            img.raycastTarget = false;
        }

        private ProceduralImage SetupProcedural(GameObject go, Vector4 radius, float border, float falloff)
        {
            var existingImage = go.GetComponent<Image>();
            if (existingImage != null && existingImage is not ProceduralImage)
            {
                // A plain Image can't become procedural on a prefab instance; recreate only on own objects.
                if (PrefabUtility.IsPartOfPrefabInstance(existingImage))
                {
                    return null;
                }

                Object.DestroyImmediate(existingImage);
            }

            var modifier = go.GetComponent<ProceduralImageModifier>();
            if (modifier == null)
            {
                modifier = go.AddComponent<FreeModifier>();
            }

            var img = GetOrAdd<ProceduralImage>(go);
            radius = new Vector4(Mathf.Round(radius.x * 100f) / 100f, Mathf.Round(radius.y * 100f) / 100f,
                Mathf.Round(radius.z * 100f) / 100f, Mathf.Round(radius.w * 100f) / 100f);
            switch (modifier)
            {
                case FreeModifier free when free.Radius != radius:
                    free.Radius = radius;
                    break;
                case UniformModifier uniform when !Mathf.Approximately(uniform.Radius, radius.x):
                    uniform.Radius = radius.x;
                    break;
            }

            if (!Mathf.Approximately(img.BorderWidth, border)) img.BorderWidth = border;
            if (!Mathf.Approximately(img.FalloffDistance, falloff)) img.FalloffDistance = falloff;
            return img;
        }

        private void ApplyFill(Graphic img, JObject fill, float opacity)
        {
            if (img == null)
            {
                return;
            }

            var fillOpacity = PenpotDocument.Num(fill["fillOpacity"], 1f) * opacity;
            var gradient = fill["fillColorGradient"] as JObject;
            if (gradient == null)
            {
                RemoveComponent<Gradient2>(img.gameObject);
                ApplyColor(img, (string)fill["fillColor"], fillOpacity);
                return;
            }

            ApplyColor(img, "#ffffff", fillOpacity);
            var effect = GetOrAdd<Gradient2>(img.gameObject);
            var dx = PenpotDocument.Num(gradient["endX"], 1f) - PenpotDocument.Num(gradient["startX"]);
            var dy = PenpotDocument.Num(gradient["endY"], 1f) - PenpotDocument.Num(gradient["startY"]);
            var horizontal = Mathf.Abs(dx) >= Mathf.Abs(dy);
            // Gradient2 runs left→right and bottom→top; Penpot y grows down.
            var flip = horizontal ? dx < 0f : dy > 0f;
            var stops = (gradient["stops"] as JArray)?.OfType<JObject>().Take(8).ToList() ?? new List<JObject>();
            var colorKeys = stops.Select(st => new GradientColorKey(PenpotDocument.ParseColor((string)st["color"]),
                flip ? 1f - PenpotDocument.Num(st["offset"]) : PenpotDocument.Num(st["offset"]))).ToArray();
            var alphaKeys = stops.Select(st => new GradientAlphaKey(PenpotDocument.Num(st["opacity"], 1f),
                flip ? 1f - PenpotDocument.Num(st["offset"]) : PenpotDocument.Num(st["offset"]))).ToArray();
            var g = new UnityEngine.Gradient();
            g.SetKeys(colorKeys, alphaKeys);
            effect.GradientType = horizontal ? Gradient2.Type.Horizontal : Gradient2.Type.Vertical;
            effect.BlendMode = Gradient2.Blend.Multiply;
            effect.EffectGradient = g;
        }

        /// <summary>Palette colors become theme tokens; the rest stays a raw color.</summary>
        private void ApplyColor(Graphic g, string hex, float opacity)
        {
            if (g == null)
            {
                return;
            }

            hex = hex?.ToLowerInvariant();
            var color = PenpotDocument.ParseColor(hex, opacity);
            if (hex != null && _colorTokens.TryGetValue(hex, out var token))
            {
                var tc = GetOrAdd<ThemeColor>(g.gameObject);
                if (tc != null)
                {
                    if (tc.Token != token) tc.Token = token;
                    if (!Mathf.Approximately(tc.Alpha, opacity)) tc.Alpha = opacity;
                }
            }
            else
            {
                RemoveComponent<ThemeColor>(g.gameObject);
            }

            if (g.color != color)
            {
                g.color = color;
            }
        }

        private void ApplyOpacityGroup(JObject s, GameObject go)
        {
            var opacity = PenpotDocument.Opacity(s);
            var group = go.GetComponent<CanvasGroup>();
            if (opacity < 0.999f)
            {
                if (group == null) group = go.AddComponent<CanvasGroup>();
                if (!Mathf.Approximately(group.alpha, opacity)) group.alpha = opacity;
                group.interactable = true;
            }
            else if (group != null && !Mathf.Approximately(group.alpha, 1f))
            {
                group.alpha = 1f;
            }
        }

        private void ApplyVector(JObject s, GameObject go)
        {
            if (!_vectors.TryGetValue(PenpotDocument.Id(s), out var vec) || !_sprites.TryGetValue(vec.Hash, out var sprite))
            {
                return;
            }

            var bounds = PenpotDocument.SelRect(s);
            var padMin = new Vector2(bounds.xMin - vec.Bounds.xMin, vec.Bounds.yMax - bounds.yMax);
            var padMax = new Vector2(vec.Bounds.xMax - bounds.xMax, bounds.yMin - vec.Bounds.yMin);
            var target = go;
            if (padMin != Vector2.zero || padMax != Vector2.zero || go.transform.Find(ImageName) != null)
            {
                target = GetOrCreateChild(go, ImageName);
                Stretch(target, -padMin, padMax);
            }

            var image = GetOrAdd<Image>(target);
            if (image == null)
            {
                return;
            }

            if (image.sprite != sprite) image.sprite = sprite;
            if (image.preserveAspect) image.preserveAspect = false;
            image.raycastTarget = false;
            if (vec.TintHex != null)
            {
                ApplyColor(image, vec.TintHex, 1f);
            }
            else
            {
                RemoveComponent<ThemeColor>(target);
                if (image.color != Color.white) image.color = Color.white;
            }
        }

        // ---- text -------------------------------------------------------------------------------------

        private static string TextAlign(JObject s) => (string)FirstSpan(s)?["textAlign"] ?? "left";

        private static JObject FirstSpan(JObject s)
        {
            return Spans(s).FirstOrDefault();
        }

        private static IEnumerable<JObject> Spans(JObject s)
        {
            foreach (var paragraph in Paragraphs(s))
            {
                foreach (var span in paragraph["children"]?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
                {
                    yield return span;
                }
            }
        }

        private static IEnumerable<JObject> Paragraphs(JObject s)
        {
            var root = s["content"] as JObject;
            foreach (var set in root?["children"]?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
            {
                foreach (var p in set["children"]?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
                {
                    yield return p;
                }
            }
        }

        public static string FontToken(string family, string weight)
        {
            var w = weight switch
            {
                "100" => "Thin",
                "200" => "ExtraLight",
                "300" => "Light",
                "500" => "Medium",
                "600" => "SemiBold",
                "700" => "Bold",
                "800" => "ExtraBold",
                "900" => "Black",
                _ => "Regular"
            };
            return $"{(family ?? "Prompt").Replace(" ", "")}/{w}";
        }

        private void ApplyText(JObject s, GameObject go, JObject parent)
        {
            var existing = go.GetComponent<Graphic>();
            if (existing != null && existing is not TextMeshProUGUI)
            {
                return;
            }

            var tmp = GetOrAdd<TextMeshProUGUI>(go);
            var baseSpan = FirstSpan(s) ?? new JObject();
            var family = (string)baseSpan["fontFamily"];
            var weight = (string)baseSpan["fontWeight"] ?? "400";
            var size = PenpotDocument.Num(baseSpan["fontSize"], 14f);
            var token = FontToken(family, weight);

            var themeFont = GetOrAdd<ThemeFont>(go);
            if (themeFont.Token != token) themeFont.Token = token;
            if (_fonts.TryGetValue(token, out var font) && tmp.font != font)
            {
                tmp.font = font;
            }

            if (!Mathf.Approximately(tmp.fontSize, size)) tmp.fontSize = size;
            if (tmp.enableAutoSizing) tmp.enableAutoSizing = false;

            var text = BuildRichText(s, baseSpan);
            if (tmp.text != text) tmp.text = text;

            var fill = (baseSpan["fills"] as JArray)?.OfType<JObject>().FirstOrDefault();
            ApplyColor(tmp, (string)fill?["fillColor"] ?? "#000000", PenpotDocument.Num(fill?["fillOpacity"], 1f) * PenpotDocument.Opacity(s));

            var spacing = PenpotDocument.Num(baseSpan["letterSpacing"]) / size * 100f;
            SetIfDiff(tmp.characterSpacing, spacing, v => tmp.characterSpacing = v);

            if (font != null)
            {
                var natural = font.faceInfo.lineHeight / font.faceInfo.pointSize;
                var lineHeight = PenpotDocument.Num(baseSpan["lineHeight"], 1.2f);
                SetIfDiff(tmp.lineSpacing, Mathf.Round((lineHeight - natural) * 1000f) / 10f, v => tmp.lineSpacing = v);
            }

            var style = FontStyles.Normal;
            if ((string)baseSpan["textTransform"] == "uppercase") style |= FontStyles.UpperCase;
            if ((string)baseSpan["textTransform"] == "lowercase") style |= FontStyles.LowerCase;
            if ((string)baseSpan["textDecoration"] == "underline") style |= FontStyles.Underline;
            if ((string)baseSpan["textDecoration"] == "line-through") style |= FontStyles.Strikethrough;
            if ((string)baseSpan["fontStyle"] == "italic") style |= FontStyles.Italic;
            if (tmp.fontStyle != style) tmp.fontStyle = style;

            var vertical = (string)s["content"]?["verticalAlign"] ?? "top";
            var alignment = (TextAlign(s), vertical) switch
            {
                ("center", "center") => TextAlignmentOptions.Center,
                ("center", "bottom") => TextAlignmentOptions.Bottom,
                ("center", _) => TextAlignmentOptions.Top,
                ("right", "center") => TextAlignmentOptions.Right,
                ("right", "bottom") => TextAlignmentOptions.BottomRight,
                ("right", _) => TextAlignmentOptions.TopRight,
                ("justify", _) => TextAlignmentOptions.TopJustified,
                (_, "center") => TextAlignmentOptions.Left,
                (_, "bottom") => TextAlignmentOptions.BottomLeft,
                _ => TextAlignmentOptions.TopLeft
            };
            if (tmp.alignment != alignment) tmp.alignment = alignment;

            var grow = (string)s["growType"];
            var wrapping = grow == "auto-width" ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
            if (tmp.textWrappingMode != wrapping) tmp.textWrappingMode = wrapping;
            if (tmp.overflowMode != TextOverflowModes.Overflow) tmp.overflowMode = TextOverflowModes.Overflow;
            if (tmp.margin != Vector4.zero) tmp.margin = Vector4.zero;
            if (tmp.raycastTarget) tmp.raycastTarget = false;

            // Outside of flex layouts auto-sized texts are sized by a ContentSizeFitter.
            var inFlex = parent != null && PenpotDocument.IsFlex(parent) && !IsAbsolute(s);
            var fitter = go.GetComponent<ContentSizeFitter>();
            if (!inFlex && grow is "auto-width" or "auto-height")
            {
                fitter = GetOrAdd<ContentSizeFitter>(go);
                var h = grow == "auto-width" ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
                if (fitter.horizontalFit != h) fitter.horizontalFit = h;
                if (fitter.verticalFit != ContentSizeFitter.FitMode.PreferredSize) fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                RemoveComponent<ContentSizeFitter>(go);
            }
        }

        private string BuildRichText(JObject s, JObject baseSpan)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var paragraph in Paragraphs(s))
            {
                if (!first)
                {
                    sb.Append('\n');
                }

                first = false;
                foreach (var span in paragraph["children"]?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
                {
                    var text = (string)span["text"] ?? "";
                    if (text.Length == 0)
                    {
                        continue;
                    }

                    var open = new StringBuilder();
                    var close = new StringBuilder();
                    var fill = (span["fills"] as JArray)?.OfType<JObject>().FirstOrDefault();
                    var baseFill = (baseSpan["fills"] as JArray)?.OfType<JObject>().FirstOrDefault();
                    var hex = ((string)fill?["fillColor"])?.ToLowerInvariant();
                    if (hex != null && hex != ((string)baseFill?["fillColor"])?.ToLowerInvariant())
                    {
                        open.Append($"<color={hex}>");
                        close.Insert(0, "</color>");
                    }

                    var size = PenpotDocument.Num(span["fontSize"], 0f);
                    if (size > 0f && !Mathf.Approximately(size, PenpotDocument.Num(baseSpan["fontSize"], 0f)))
                    {
                        open.Append($"<size={size.ToString(CultureInfo.InvariantCulture)}>");
                        close.Insert(0, "</size>");
                    }

                    var family = (string)span["fontFamily"];
                    var weight = (string)span["fontWeight"];
                    if (family != (string)baseSpan["fontFamily"] || weight != (string)baseSpan["fontWeight"])
                    {
                        if (_fonts.TryGetValue(FontToken(family, weight), out var f))
                        {
                            open.Append($"<font=\"{f.name}\">");
                            close.Insert(0, "</font>");
                        }
                    }

                    sb.Append(open).Append(text.Replace("<", "<​")).Append(close);
                }
            }

            return sb.ToString();
        }

        // ---- interaction ------------------------------------------------------------------------------

        /// <summary>Button-like components get a Button so they react to touches out of the box.</summary>
        private static void AddInteraction(JObject s, GameObject go)
        {
            var name = PenpotDocument.Name(s);
            if (!name.Contains("/ Button /") && !name.Contains("/ IconButton /"))
            {
                return;
            }

            var button = GetOrAdd<Button>(go);
            var bg = go.transform.Find(BackgroundName);
            if (bg != null)
            {
                var graphic = bg.GetComponent<Graphic>();
                if (button.targetGraphic != graphic)
                {
                    button.targetGraphic = graphic;
                }
            }

            var interactable = !name.Contains("Disabled");
            if (button.interactable != interactable)
            {
                button.interactable = interactable;
            }
        }

        // ---- helpers ----------------------------------------------------------------------------------

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c != null)
            {
                if (c is Behaviour { enabled: false } behaviour)
                {
                    behaviour.enabled = true;
                }

                return c;
            }

            // Graphic types are exclusive.
            if (typeof(Graphic).IsAssignableFrom(typeof(T)) && go.GetComponent<Graphic>() != null)
            {
                return null;
            }

            return go.AddComponent<T>();
        }

        /// <summary>
        /// A board may change its direction or wrapping in Penpot, and an object holds only one LayoutGroup: the other kinds are removed.
        /// An inherited one can't be removed in a variant or an instance; it is switched off and goes away when its base prefab is
        /// imported (bases are imported first). Returns false while such a group is still in the way.
        /// </summary>
        private static bool RemoveOtherLayoutGroups(GameObject go, System.Type keep)
        {
            var free = true;
            foreach (var group in go.GetComponents<LayoutGroup>())
            {
                if (group.GetType() == keep)
                {
                    continue;
                }

                if (!PrefabUtility.IsPartOfPrefabInstance(group))
                {
                    Object.DestroyImmediate(group);
                    continue;
                }

                group.enabled = false;
                free = false;
                Debug.LogWarning($"[{nameof(PenpotUiBuilder)}] [{nameof(RemoveOtherLayoutGroups)}] '{go.name}' inherits a {group.GetType().Name}; re-import its base prefab first");
            }

            return free;
        }

        /// <summary>Components inherited from a prefab can't be removed in an instance or variant: they are switched off instead.</summary>
        private static void RemoveComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null)
            {
                return;
            }

            if (!PrefabUtility.IsPartOfPrefabInstance(c))
            {
                Object.DestroyImmediate(c);
                return;
            }

            if (c is ThemeColor themeColor && !string.IsNullOrEmpty(themeColor.Token))
            {
                themeColor.Token = "";
            }

            if (c is Behaviour behaviour && behaviour.enabled)
            {
                behaviour.enabled = false;
            }
        }

        /// <summary>Variants may lack a layer of their base (no shadow, no stroke): the inherited helper is switched off.</summary>
        private static void SetHelperActive(GameObject parent, string name, bool active)
        {
            var t = parent.transform.Find(name);
            if (t != null && t.gameObject.activeSelf != active)
            {
                t.gameObject.SetActive(active);
            }
        }

        private static GameObject GetOrCreateChild(GameObject parent, string name)
        {
            var t = parent.transform.Find(name);
            if (t != null)
            {
                return t.gameObject;
            }

            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            return go;
        }

        private static void Stretch(GameObject go, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = (RectTransform)go.transform;
            offsetMin = Round(offsetMin);
            offsetMax = Round(offsetMax);
            if (rt.anchorMin != Vector2.zero) rt.anchorMin = Vector2.zero;
            if (rt.anchorMax != Vector2.one) rt.anchorMax = Vector2.one;
            if (rt.pivot != new Vector2(0.5f, 0.5f)) rt.pivot = new Vector2(0.5f, 0.5f);
            if (rt.offsetMin != offsetMin) rt.offsetMin = offsetMin;
            if (rt.offsetMax != offsetMax) rt.offsetMax = offsetMax;
        }

        private static void SetIfDiff(float current, float value, System.Action<float> set)
        {
            if (!Mathf.Approximately(current, value))
            {
                set(value);
            }
        }

        private static void SetIfDiff(bool current, bool value, System.Action<bool> set)
        {
            if (current != value)
            {
                set(value);
            }
        }
    }
}

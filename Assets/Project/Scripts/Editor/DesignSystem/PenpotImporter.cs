using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Chang.UI.DesignSystem;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;
using Object = UnityEngine.Object;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Imports the Penpot redesign into uGUI prefabs.
    /// Penpot components become prefabs (same-structure siblings become prefab variants of one base),
    /// component instances inside other components and screens become nested prefab instances,
    /// library colors and fonts become <see cref="DesignTheme"/> tokens.
    /// Re-import updates the assets in place. See Docs/ui-design-system.md.
    /// </summary>
    public static class PenpotImporter
    {
        public const string UiRoot = "Assets/Project/UI";
        public const string VectorsRoot = UiRoot + "/Vectors";
        public const string ComponentsRoot = UiRoot + "/Components";
        public const string ScreensRoot = UiRoot + "/Screens";
        public const string MascotRoot = UiRoot + "/Mascot";
        public const string PreviewScene = UiRoot + "/DesignPreview.unity";
        public const string ThemePath = DesignTheme.AssetPath;
        public const string FontsRoot = "Assets/Project/Fonts";

        // TMP looks up <font="…"> rich text tags in Resources/Fonts & Materials.
        public const string FontAssetsRoot = FontsRoot + "/Resources/Fonts & Materials";

        private const string PagePrefix = "Redesign";
        private const string ComponentsPage = "Redesign · Components";
        private const string ScreensPage = "Redesign · Screens";
        private const string MascotPage = "Redesign · Mascot";
        private const string LastJsonKey = "Chang.PenpotImporter.LastJson";

        private static readonly (string family, string folder, string file)[] FontFiles =
        {
            ("Prompt", "Prompt/Static", "Prompt-{0}.ttf"),
            ("NotoSansThaiLooped", "NotoSansThaiLooped", "NotoSansThaiLooped-{0}.ttf"),
            ("Charmonman", "Charmonman", "Charmonman-{0}.ttf"),
        };

        private static readonly string[] Weights = { "Regular", "Medium", "SemiBold", "Bold" };

        [MenuItem("Chang/Design System/Import from Penpot JSON…", false, 0)]
        private static void ImportMenu()
        {
            var last = EditorPrefs.GetString(LastJsonKey, "");
            var path = EditorUtility.OpenFilePanel("Penpot file JSON", string.IsNullOrEmpty(last) ? "" : Path.GetDirectoryName(last), "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            EditorPrefs.SetString(LastJsonKey, path);
            Import(path);
        }

        [MenuItem("Chang/Design System/Re-import last Penpot JSON", false, 1)]
        private static void ReimportMenu()
        {
            var last = EditorPrefs.GetString(LastJsonKey, "");
            if (!File.Exists(last))
            {
                ImportMenu();
                return;
            }

            Import(last);
        }

        /// <summary>Runs the import on the next editor update (for calls from tools that can't block the main thread).</summary>
        public static void ImportDeferred(string jsonPath)
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    Import(jsonPath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            };
            // Wake the editor loop: delayCall doesn't run while the Editor window is in the background.
            EditorApplication.QueuePlayerLoopUpdate();
        }

        public static void Import(string jsonPath)
        {
            var doc = PenpotDocument.Load(jsonPath);
            var pages = doc.Pages.Where(p => p.Name.StartsWith(PagePrefix)).ToList();
            var svg = new PenpotSvg(doc);

            try
            {
                EditorUtility.DisplayProgressBar("Penpot import", "Fonts", 0.05f);
                EnsureFolders();
                var fonts = EnsureFonts();

                EditorUtility.DisplayProgressBar("Penpot import", "Theme", 0.1f);
                var theme = EnsureTheme(doc, fonts, out var colorTokens);

                var components = CollectComponents(doc);
                var screens = doc.FindPage(ScreensPage) is { } sp ? doc.TopLevel(sp).Where(s => PenpotDocument.Type(s) == "frame").ToList() : new List<JObject>();
                var friends = doc.FindPage(MascotPage) is { } mp
                    ? doc.TopLevel(mp).Where(s => PenpotDocument.Type(s) == "frame" && PenpotDocument.Name(s).StartsWith("Friend /")).ToList()
                    : new List<JObject>();

                EditorUtility.DisplayProgressBar("Penpot import", "Vector graphics", 0.15f);
                var roots = components.Select(c => c.Shape).Concat(screens).Concat(friends).ToList();
                var vectors = new Dictionary<string, PenpotSvg.Result>();
                foreach (var root in roots)
                {
                    CollectVectors(doc, svg, root, vectors);
                }

                var sprites = WriteSprites(doc, vectors);

                var prefabs = new Dictionary<string, GameObject>();
                var builder = new PenpotUiBuilder(doc, svg, theme, colorTokens, fonts, vectors, sprites, prefabs);

                for (var i = 0; i < components.Count; i++)
                {
                    var c = components[i];
                    EditorUtility.DisplayProgressBar("Penpot import", c.Component.FullName, 0.3f + 0.4f * i / components.Count);
                    prefabs[c.Component.Id] = BuildComponent(doc, builder, c, prefabs);
                }

                var screenPrefabs = new List<GameObject>();
                for (var i = 0; i < screens.Count; i++)
                {
                    var s = screens[i];
                    EditorUtility.DisplayProgressBar("Penpot import", PenpotDocument.Name(s), 0.7f + 0.2f * i / screens.Count);
                    screenPrefabs.Add(BuildScreen(builder, s, $"{ScreensRoot}/{FileName(PenpotDocument.Name(s))}.prefab"));
                }

                foreach (var f in friends)
                {
                    BuildScreen(builder, f, $"{MascotRoot}/{FileName(PenpotDocument.Name(f).Replace("Friend /", "Friend").Trim())}.prefab", isScreen: false);
                }

                EditorUtility.DisplayProgressBar("Penpot import", "Preview scene", 0.95f);
                BuildPreviewScene(screenPrefabs);

                AssetDatabase.SaveAssets();
                Debug.Log($"[{nameof(PenpotImporter)}] [{nameof(Import)}] Imported {components.Count} components, {screens.Count} screens, " +
                          $"{friends.Count} mascots, {sprites.Count} sprites from {pages.Count} pages.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ---- folders, fonts, theme --------------------------------------------------------------------

        private static void EnsureFolders()
        {
            foreach (var folder in new[]
                     {
                         UiRoot, VectorsRoot, VectorsRoot + "/Icons", VectorsRoot + "/Art", VectorsRoot + "/Mascot", VectorsRoot + "/Strokes",
                         ComponentsRoot, ScreensRoot, MascotRoot, FontAssetsRoot
                     })
            {
                CreateFolder(folder);
            }
        }

        private static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            CreateFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Creates a dynamic TMP font asset per family/weight. Returns token ("Prompt/SemiBold") → asset.</summary>
        private static Dictionary<string, TMP_FontAsset> EnsureFonts()
        {
            var result = new Dictionary<string, TMP_FontAsset>();
            foreach (var (family, folder, file) in FontFiles)
            {
                foreach (var weight in Weights)
                {
                    var ttfPath = $"{FontsRoot}/{folder}/{string.Format(file, weight)}";
                    var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                    if (font == null)
                    {
                        continue;
                    }

                    var assetPath = $"{FontAssetsRoot}/{family}-{weight} SDF.asset";
                    var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                    if (asset == null)
                    {
                        asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
                        asset.name = Path.GetFileNameWithoutExtension(assetPath);
                        AssetDatabase.CreateAsset(asset, assetPath);
                        asset.atlasTextures[0].name = asset.name + " Atlas";
                        AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
                        asset.material.name = asset.name + " Material";
                        AssetDatabase.AddObjectToAsset(asset.material, asset);
                        asset.isMultiAtlasTexturesEnabled = true;
                        asset.ClearFontAssetData(true);
                        EditorUtility.SetDirty(asset);
                    }

                    // Glyphs added while working in the Editor are not shipped; they are added again at runtime.
                    var so = new SerializedObject(asset);
                    var clear = so.FindProperty("m_ClearDynamicDataOnBuild");
                    if (clear != null && !clear.boolValue)
                    {
                        clear.boolValue = true;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }

                    result[$"{family}/{weight}"] = asset;
                }
            }

            // Looped Thai has no Latin glyphs: fall back to Prompt of the same weight.
            foreach (var weight in Weights)
            {
                if (result.TryGetValue($"NotoSansThaiLooped/{weight}", out var noto) && result.TryGetValue($"Prompt/{weight}", out var prompt))
                {
                    noto.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
                    if (!noto.fallbackFontAssetTable.Contains(prompt))
                    {
                        noto.fallbackFontAssetTable.Add(prompt);
                        EditorUtility.SetDirty(noto);
                    }
                }
            }

            return result;
        }

        private static DesignTheme EnsureTheme(PenpotDocument doc, Dictionary<string, TMP_FontAsset> fonts, out Dictionary<string, string> colorTokens)
        {
            var theme = AssetDatabase.LoadAssetAtPath<DesignTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<DesignTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            colorTokens = new Dictionary<string, string>();
            foreach (var (name, hex) in doc.Colors)
            {
                if (colorTokens.ContainsKey(hex))
                {
                    continue;
                }

                colorTokens[hex] = name;
                theme.SetColor(name, PenpotDocument.ParseColor(hex));
            }

            if (!colorTokens.ContainsKey("#ffffff"))
            {
                colorTokens["#ffffff"] = "White";
                theme.SetColor("White", Color.white);
            }

            // Tone colors come from the "tone / …" cards of the Foundations page.
            var foundations = doc.Pages.FirstOrDefault(p => p.Name.StartsWith(PagePrefix) && p.Name.Contains("Foundations"));
            if (foundations != null)
            {
                foreach (var tone in doc.TopLevel(foundations).Where(s => PenpotDocument.Name(s).StartsWith("tone /")))
                {
                    var label = doc.Children(tone).FirstOrDefault(c => PenpotDocument.Type(c) == "text" && PenpotDocument.Name(c) != null &&
                                                                       Regex.IsMatch(PenpotDocument.Name(c), "^[A-Za-z]"));
                    var span = label?["content"]?["children"]?[0]?["children"]?[0]?["children"]?[0];
                    var hex = ((string)span?["fills"]?[0]?["fillColor"])?.ToLowerInvariant();
                    if (hex == null || colorTokens.ContainsKey(hex))
                    {
                        continue;
                    }

                    var name = "Tone · " + PenpotDocument.Name(label);
                    colorTokens[hex] = name;
                    theme.SetColor(name, PenpotDocument.ParseColor(hex));
                }
            }

            foreach (var (token, font) in fonts)
            {
                theme.SetFont(token, font);
            }

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssetIfDirty(theme);
            return theme;
        }

        // ---- vectors ----------------------------------------------------------------------------------

        private static void CollectVectors(PenpotDocument doc, PenpotSvg svg, JObject s, Dictionary<string, PenpotSvg.Result> vectors)
        {
            if (PenpotDocument.Hidden(s))
            {
                return;
            }

            if (svg.IsVector(s))
            {
                vectors[PenpotDocument.Id(s)] = svg.Build(s);
                return;
            }

            var stroke = PenpotDocument.VisibleStrokes(s).FirstOrDefault();
            if (stroke != null && (string)stroke["strokeStyle"] is "dashed" or "dotted")
            {
                var size = PenpotDocument.SelRect(s).size;
                vectors[PenpotDocument.Id(s) + PenpotUiBuilder.StrokeName] = PenpotSvg.DashedRect(size, PenpotDocument.Radius(s),
                    (string)stroke["strokeColor"], PenpotDocument.Num(stroke["strokeOpacity"], 1f), PenpotDocument.Num(stroke["strokeWidth"], 1f),
                    (string)stroke["strokeAlignment"] ?? "inner");
            }

            foreach (var c in doc.Children(s))
            {
                CollectVectors(doc, svg, c, vectors);
            }
        }

        /// <summary>Writes one SVG per distinct graphic and imports it as a textured sprite. Returns hash → sprite.</summary>
        private static Dictionary<string, Sprite> WriteSprites(PenpotDocument doc, Dictionary<string, PenpotSvg.Result> vectors)
        {
            var files = new Dictionary<string, (string path, float maxSize)>();
            var names = new HashSet<string>();
            foreach (var (id, vec) in vectors)
            {
                var size = Mathf.Max(vec.Bounds.width, vec.Bounds.height);
                if (files.TryGetValue(vec.Hash, out var existing))
                {
                    files[vec.Hash] = (existing.path, Mathf.Max(existing.maxSize, size));
                    continue;
                }

                var shapeId = id.EndsWith(PenpotUiBuilder.StrokeName) ? null : id;
                var shape = doc.Get(shapeId);
                var name = shape != null ? PenpotDocument.Name(shape) : "dashed";
                string folder;
                string baseName;
                if (shape == null)
                {
                    folder = "Strokes";
                    baseName = "dashed";
                }
                else if (name.StartsWith("icon /"))
                {
                    folder = "Icons";
                    baseName = "icon_" + FileName(name.Substring("icon /".Length));
                }
                else if (doc.ShapePage.TryGetValue(shapeId, out var page) && page.Name == MascotPage || name.Contains("Mascot"))
                {
                    folder = "Mascot";
                    baseName = FileName(name);
                }
                else
                {
                    folder = "Art";
                    baseName = FileName(name);
                }

                // Icons keep a clean name for their first (canonical) shape; everything else is suffixed by content hash.
                var fileName = folder == "Icons" && names.Add(baseName) ? baseName : $"{baseName}_{vec.Hash}";
                files[vec.Hash] = ($"{VectorsRoot}/{folder}/{fileName}.svg", size);
            }

            var changed = new List<string>();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var (hash, (path, _)) in files)
                {
                    var vec = vectors.Values.First(v => v.Hash == hash);
                    var full = Path.GetFullPath(path);
                    if (File.Exists(full) && File.ReadAllText(full) == vec.Svg)
                    {
                        continue;
                    }

                    File.WriteAllText(full, vec.Svg, new UTF8Encoding(false));
                    changed.Add(path);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var (_, (path, maxSize)) in files)
                {
                    ConfigureSvgImporter(path, maxSize);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            var sprites = new Dictionary<string, Sprite>();
            foreach (var (hash, (path, _)) in files)
            {
                var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
                if (sprite != null)
                {
                    sprites[hash] = sprite;
                }
                else
                {
                    Debug.LogWarning($"[{nameof(PenpotImporter)}] [{nameof(WriteSprites)}] No sprite imported from {path}");
                }
            }

            return sprites;
        }

        /// <summary>SVGs are rasterized (TexturedSprite) at 3× of the largest use, so the regular Image component can show them.</summary>
        private static void ConfigureSvgImporter(string path, float maxSize)
        {
            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                return;
            }

            var so = new SerializedObject(importer);
            var size = Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.CeilToInt(maxSize * 3f)), 64, 2048);
            var dirty = false;

            void Set(string prop, int value)
            {
                var p = so.FindProperty(prop);
                if (p != null && p.intValue != value)
                {
                    p.intValue = value;
                    dirty = true;
                }
            }

            void SetBool(string prop, bool value)
            {
                var p = so.FindProperty(prop);
                if (p != null && p.boolValue != value)
                {
                    p.boolValue = value;
                    dirty = true;
                }
            }

            Set("m_SvgType", 1); // SVGType.TexturedSprite
            // Keep the viewBox: a sprite must cover the whole graphic frame, not only the drawn geometry.
            Set("m_ViewportOptions", 2); // ViewportOptions.PreserveViewport
            SetBool("m_PreserveViewport", true);
            SetBool("m_KeepTextureAspectRatio", true);
            Set("m_TextureSize", size);
            Set("m_SampleCount", 4);
            Set("m_FilterMode", (int)FilterMode.Bilinear);
            Set("m_WrapMode", (int)TextureWrapMode.Clamp);

            if (dirty)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                importer.SaveAndReimport();
            }
        }

        // ---- components -------------------------------------------------------------------------------

        private class ComponentEntry
        {
            public PenpotDocument.Component Component;
            public JObject Shape;
            public string PrefabPath;
            public ComponentEntry Base;
            public Dictionary<string, string> Alias = new();
            public List<string> Dependencies = new();
        }

        /// <summary>Main components of the redesign pages in dependency order (nested components and bases first).</summary>
        private static List<ComponentEntry> CollectComponents(PenpotDocument doc)
        {
            var entries = new List<ComponentEntry>();
            foreach (var c in doc.Components.Values)
            {
                if (!doc.ShapePage.TryGetValue(c.MainInstanceId ?? "", out var page) || !page.Name.StartsWith(PagePrefix))
                {
                    continue;
                }

                var shape = doc.Get(c.MainInstanceId);
                var full = PenpotDocument.Name(shape);
                var parts = full.Split('/').Select(p => p.Trim()).Where(p => p.Length > 0 && p != "Chang DS").ToList();
                var group = parts.Count > 1 ? parts[0] : parts.FirstOrDefault() ?? "Misc";
                var leaf = parts.LastOrDefault() ?? c.Name;
                entries.Add(new ComponentEntry
                {
                    Component = c,
                    Shape = shape,
                    PrefabPath = $"{ComponentsRoot}/{FileName(group)}/{FileName(leaf)}.prefab"
                });
            }

            // Penpot order on the canvas: top to bottom, left to right.
            entries = entries.OrderBy(e => PenpotDocument.SelRect(e.Shape).y).ThenBy(e => PenpotDocument.SelRect(e.Shape).x).ToList();

            foreach (var e in entries)
            {
                CollectDependencies(doc, e.Shape, e.Shape, e.Dependencies);
            }

            // Components of one group (Button, OptionCard, …) with the same root layout become variants of the first one:
            // the base defines the shared look, a variant overrides what differs (colors, texts, extra or hidden layers).
            foreach (var group in entries.GroupBy(e => Path.GetDirectoryName(e.PrefabPath)))
            {
                var bySignature = new Dictionary<string, ComponentEntry>();
                foreach (var e in group)
                {
                    var signature = RootKind(e.Shape);
                    if (bySignature.TryGetValue(signature, out var baseEntry))
                    {
                        e.Base = baseEntry;
                        e.Dependencies.Add(baseEntry.Component.Id);
                        MapAlias(doc, e.Shape, baseEntry.Shape, e.Alias);
                    }
                    else
                    {
                        bySignature[signature] = e;
                    }
                }
            }

            // Topological sort.
            var byId = entries.ToDictionary(e => e.Component.Id);
            var sorted = new List<ComponentEntry>();
            var visiting = new HashSet<string>();
            var done = new HashSet<string>();

            void Visit(ComponentEntry e)
            {
                if (done.Contains(e.Component.Id) || !visiting.Add(e.Component.Id))
                {
                    return;
                }

                foreach (var dep in e.Dependencies)
                {
                    if (byId.TryGetValue(dep, out var d))
                    {
                        Visit(d);
                    }
                }

                done.Add(e.Component.Id);
                sorted.Add(e);
            }

            foreach (var e in entries)
            {
                Visit(e);
            }

            return sorted;
        }

        private static void CollectDependencies(PenpotDocument doc, JObject root, JObject s, List<string> deps)
        {
            foreach (var c in doc.Children(s))
            {
                var compId = PenpotDocument.ComponentId(c);
                if (compId != null && c != root && !PenpotDocument.IsMainInstance(c))
                {
                    deps.Add(compId);
                }

                CollectDependencies(doc, root, c, deps);
            }
        }

        private static string RootKind(JObject s)
        {
            return PenpotDocument.Type(s) + (PenpotDocument.IsFlex(s) ? "|flex:" + (string)s["layoutFlexDir"] + (string)s["layoutWrapType"] : "");
        }

        private static void MapAlias(PenpotDocument doc, JObject variant, JObject baseShape, Dictionary<string, string> alias)
        {
            alias[PenpotDocument.Id(variant)] = PenpotDocument.Id(baseShape);

            // Layers are matched by name, type and nested component, so a variant can add or lack layers.
            var unused = doc.Children(baseShape).ToList();
            foreach (var v in doc.Children(variant))
            {
                var match = unused.FirstOrDefault(b => PenpotDocument.Name(b) == PenpotDocument.Name(v) &&
                                                       PenpotDocument.Type(b) == PenpotDocument.Type(v) &&
                                                       PenpotDocument.ComponentId(b) == PenpotDocument.ComponentId(v) &&
                                                       PenpotDocument.IsFlex(b) == PenpotDocument.IsFlex(v) &&
                                                       (string)b["layoutFlexDir"] == (string)v["layoutFlexDir"]);
                if (match != null)
                {
                    unused.Remove(match);
                    MapAlias(doc, v, match, alias);
                }
            }
        }

        private static GameObject BuildComponent(PenpotDocument doc, PenpotUiBuilder builder, ComponentEntry entry, Dictionary<string, GameObject> prefabs)
        {
            CreateFolder(Path.GetDirectoryName(entry.PrefabPath)!.Replace('\\', '/'));
            var options = new PenpotUiBuilder.Options { Root = entry.Shape, AssignIds = true, Alias = entry.Alias };

            if (entry.Base != null && prefabs.TryGetValue(entry.Base.Component.Id, out var basePrefab))
            {
                // A prefab that is not yet a variant of its base is rebuilt as one (same path, so the GUID stays).
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabPath);
                var isVariantOfBase = existing != null && PrefabUtility.GetPrefabAssetType(existing) == PrefabAssetType.Variant &&
                                      PrefabUtility.GetCorrespondingObjectFromSource(existing) == basePrefab;
                return SaveWith(entry.PrefabPath, isVariantOfBase, () =>
                {
                    var scene = EditorSceneManager.NewPreviewScene();
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab, scene);
                    return (go, () =>
                    {
                        Object.DestroyImmediate(go);
                        EditorSceneManager.ClosePreviewScene(scene);
                    });
                }, go => builder.BuildRoot(entry.Shape, go, options));
            }

            // A former variant that became a base is rebuilt as a plain prefab.
            var current = AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabPath);
            var plain = current == null || PrefabUtility.GetPrefabAssetType(current) != PrefabAssetType.Variant;
            return SaveWith(entry.PrefabPath, plain, NewRoot, go => builder.BuildRoot(entry.Shape, go, options));
        }

        private static GameObject BuildScreen(PenpotUiBuilder builder, JObject shape, string path, bool isScreen = true)
        {
            CreateFolder(Path.GetDirectoryName(path)!.Replace('\\', '/'));
            var options = new PenpotUiBuilder.Options { Root = shape, IsScreen = isScreen, AssignIds = !isScreen };
            return SaveWith(path, true, NewRoot, go => builder.BuildRoot(shape, go, options));
        }

        private static (GameObject, Action) NewRoot()
        {
            var go = new GameObject("Root", typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            return (go, () => Object.DestroyImmediate(go));
        }

        /// <summary>Loads the existing prefab (to update it in place) or creates a new root, builds, saves.</summary>
        private static GameObject SaveWith(string path, bool updateExisting, Func<(GameObject, Action)> create, Action<GameObject> build)
        {
            if (updateExisting && File.Exists(Path.GetFullPath(path)))
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

            var (root, dispose) = create();
            try
            {
                build(root);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                dispose();
            }
        }

        // ---- preview ----------------------------------------------------------------------------------

        /// <summary>A scene with every screen under a 540×1080 canvas, only the first one active.</summary>
        private static void BuildPreviewScene(List<GameObject> screens)
        {
            var activeScene = SceneManager.GetActiveScene().path;
            if (SceneManager.GetActiveScene().isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 |
                                              AdditionalCanvasShaderChannels.TexCoord3;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            for (var i = 0; i < screens.Count; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(screens[i], canvasGo.transform);
                go.SetActive(i == 0);
            }

            EditorSceneManager.SaveScene(scene, PreviewScene);
            if (!string.IsNullOrEmpty(activeScene) && activeScene != PreviewScene)
            {
                EditorSceneManager.OpenScene(activeScene);
            }
        }

        // ---- utils ------------------------------------------------------------------------------------

        public static string FileName(string name)
        {
            var s = name.Replace("·", "-").Replace("—", "-").Replace("%", "pct").Replace("→", "to").Replace("/", "-");
            s = Regex.Replace(s, @"[^A-Za-z0-9 _\-\(\)]", "");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            s = Regex.Replace(s, @"-{2,}", "-");
            return string.IsNullOrEmpty(s) ? "Unnamed" : s;
        }
    }
}

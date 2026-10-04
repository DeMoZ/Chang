using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Read-only view of a Penpot file exported as JSON (the response of the get-file RPC command).
    /// Indexes every shape of every page by id.
    /// </summary>
    public class PenpotDocument
    {
        public const string RootId = "00000000-0000-0000-0000-000000000000";

        public class Page
        {
            public string Id;
            public string Name;
            public Dictionary<string, JObject> Objects;
        }

        public class Component
        {
            public string Id;
            public string Name;
            public string Path;
            public string MainInstanceId;
            public string MainInstancePage;

            /// <summary>"Chang DS / Button / Primary" style full name.</summary>
            public string FullName => string.IsNullOrEmpty(Path) ? Name : $"{Path} / {Name}";
        }

        public readonly List<Page> Pages = new();
        public readonly Dictionary<string, JObject> Shapes = new();
        public readonly Dictionary<string, Page> ShapePage = new();
        public readonly Dictionary<string, Component> Components = new();

        /// <summary>Library colors: name → hex (lower case, "#rrggbb").</summary>
        public readonly List<(string name, string hex)> Colors = new();

        public static PenpotDocument Load(string path)
        {
            var json = JObject.Parse(File.ReadAllText(path));
            var data = (JObject)json["data"];
            var doc = new PenpotDocument();

            var pagesIndex = (JObject)data["pagesIndex"];
            foreach (var pageId in data["pages"].Values<string>())
            {
                var p = (JObject)pagesIndex[pageId];
                var page = new Page
                {
                    Id = pageId,
                    Name = (string)p["name"],
                    Objects = new Dictionary<string, JObject>()
                };

                foreach (var prop in ((JObject)p["objects"]).Properties())
                {
                    var o = (JObject)prop.Value;
                    page.Objects[prop.Name] = o;
                    doc.Shapes[prop.Name] = o;
                    doc.ShapePage[prop.Name] = page;
                }

                doc.Pages.Add(page);
            }

            if (data["components"] is JObject comps)
            {
                foreach (var prop in comps.Properties())
                {
                    var c = (JObject)prop.Value;
                    doc.Components[prop.Name] = new Component
                    {
                        Id = prop.Name,
                        Name = (string)c["name"],
                        Path = (string)c["path"],
                        MainInstanceId = (string)c["mainInstanceId"],
                        MainInstancePage = (string)c["mainInstancePage"]
                    };
                }
            }

            if (data["colors"] is JObject colors)
            {
                foreach (var prop in colors.Properties())
                {
                    var name = (string)prop.Value["name"];
                    var hex = ((string)prop.Value["color"])?.ToLowerInvariant();
                    if (hex != null && doc.Colors.All(c => c.name != name))
                    {
                        doc.Colors.Add((name, hex));
                    }
                }
            }

            return doc;
        }

        public Page FindPage(string name) => Pages.FirstOrDefault(p => p.Name == name);

        public IEnumerable<JObject> TopLevel(Page page)
        {
            return Children(page.Objects[RootId]);
        }

        public IEnumerable<JObject> Children(JObject shape)
        {
            if (shape["shapes"] is not JArray ids)
            {
                yield break;
            }

            foreach (var id in ids.Values<string>())
            {
                if (Shapes.TryGetValue(id, out var child))
                {
                    yield return child;
                }
            }
        }

        public JObject Get(string id) => id != null && Shapes.TryGetValue(id, out var s) ? s : null;

        /// <summary>The shape id followed by the ids of the shapes it was copied from (shapeRef chain).</summary>
        public List<string> RefChain(JObject shape)
        {
            var chain = new List<string>();
            var cur = shape;
            while (cur != null && chain.Count < 16)
            {
                var id = Id(cur);
                if (chain.Contains(id))
                {
                    break;
                }

                chain.Add(id);
                cur = Get((string)cur["shapeRef"]);
            }

            return chain;
        }

        // ---- shape accessors -------------------------------------------------------------------------

        public static string Id(JObject s) => (string)s["id"];
        public static string Name(JObject s) => (string)s["name"] ?? "";
        public static string Type(JObject s) => (string)s["type"];
        public static bool Hidden(JObject s) => (bool?)s["hidden"] ?? false;
        public static bool IsFlex(JObject s) => (string)s["layout"] == "flex";
        public static string ComponentId(JObject s) => (string)s["componentId"];
        public static bool IsMainInstance(JObject s) => (bool?)s["mainInstance"] ?? false;

        public static float Num(JToken t, float fallback = 0f)
        {
            if (t == null || t.Type == JTokenType.Null)
            {
                return fallback;
            }

            if (t.Type == JTokenType.String)
            {
                return float.TryParse((string)t, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;
            }

            return t.Value<float>();
        }

        /// <summary>Axis aligned bounds in page coordinates (y down).</summary>
        public static Rect SelRect(JObject s)
        {
            var r = s["selrect"];
            return new Rect(Num(r["x"]), Num(r["y"]), Num(r["width"]), Num(r["height"]));
        }

        /// <summary>Center of the (possibly rotated) shape in page coordinates.</summary>
        public static Vector2 Center(JObject s)
        {
            if (s["points"] is JArray pts && pts.Count == 4)
            {
                var c = Vector2.zero;
                foreach (var p in pts)
                {
                    c += new Vector2(Num(p["x"]), Num(p["y"]));
                }

                return c / 4f;
            }

            return SelRect(s).center;
        }

        public static float Rotation(JObject s) => Num(s["rotation"]);

        public static float Opacity(JObject s) => Num(s["opacity"], 1f);

        public static Vector4 Radius(JObject s)
        {
            return new Vector4(Num(s["r1"]), Num(s["r2"]), Num(s["r3"]), Num(s["r4"]));
        }

        public static JObject FirstVisibleFill(JObject s)
        {
            if (s["fills"] is not JArray fills)
            {
                return null;
            }

            return fills.OfType<JObject>().FirstOrDefault(f => !((bool?)f["hidden"] ?? false));
        }

        public static IEnumerable<JObject> VisibleStrokes(JObject s)
        {
            if (s["strokes"] is not JArray strokes)
            {
                return Enumerable.Empty<JObject>();
            }

            return strokes.OfType<JObject>().Where(st => !((bool?)st["hidden"] ?? false) && Num(st["strokeWidth"], 1f) > 0f);
        }

        public static IEnumerable<JObject> VisibleShadows(JObject s)
        {
            if (s["shadow"] is not JArray shadows)
            {
                return Enumerable.Empty<JObject>();
            }

            return shadows.OfType<JObject>().Where(sh => !((bool?)sh["hidden"] ?? false));
        }

        public static Color ParseColor(string hex, float opacity = 1f)
        {
            if (string.IsNullOrEmpty(hex) || !ColorUtility.TryParseHtmlString(hex, out var c))
            {
                c = Color.magenta;
            }

            c.a = opacity;
            return c;
        }
    }
}

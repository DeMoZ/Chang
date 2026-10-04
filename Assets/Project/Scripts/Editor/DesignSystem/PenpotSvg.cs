using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Converts Penpot vector shapes (paths, ellipses, rects and groups of them) to standalone SVG documents.
    /// Coordinates are moved to the graphic's own origin, so equal graphics produce equal SVG text
    /// and can share one sprite.
    /// </summary>
    public class PenpotSvg
    {
        public class Result
        {
            public string Svg;
            public string Hash;

            /// <summary>Bounds of the SVG in page coordinates (shape bounds plus stroke overflow).</summary>
            public Rect Bounds;

            /// <summary>Set when the graphic uses a single color: the SVG is white and should be tinted with it.</summary>
            public string TintHex;
        }

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly Regex PathToken = new(@"[A-Za-z]|-?\d*\.?\d+(?:[eE][-+]?\d+)?", RegexOptions.Compiled);

        private readonly PenpotDocument _doc;

        public PenpotSvg(PenpotDocument doc)
        {
            _doc = doc;
        }

        /// <summary>A shape that is drawn as a sprite rather than as UI hierarchy.</summary>
        public bool IsVector(JObject s)
        {
            var type = PenpotDocument.Type(s);
            switch (type)
            {
                case "path":
                case "bool":
                    return true;
                case "group":
                    return _doc.Children(s).All(c => PenpotDocument.Hidden(c) || IsVectorPart(c));
                default:
                    return false;
            }
        }

        private bool IsVectorPart(JObject s)
        {
            var type = PenpotDocument.Type(s);
            return type is "path" or "bool" or "circle" or "rect" ||
                   (type == "group" && _doc.Children(s).All(c => PenpotDocument.Hidden(c) || IsVectorPart(c)));
        }

        public Result Build(JObject root)
        {
            var bounds = PenpotDocument.SelRect(root);
            var pad = Mathf.Ceil(MaxStroke(root) * 0.5f);
            bounds.xMin -= pad;
            bounds.yMin -= pad;
            bounds.xMax += pad;
            bounds.yMax += pad;

            var colors = new HashSet<string>();
            var hasGradient = false;
            CollectPaints(root, colors, ref hasGradient);
            var mono = !hasGradient && colors.Count == 1;

            var defs = new StringBuilder();
            var body = new StringBuilder();
            var origin = bounds.position;
            Emit(root, body, defs, origin, mono, isRoot: true);

            var sb = new StringBuilder();
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" ");
            sb.Append($"width=\"{F(bounds.width)}\" height=\"{F(bounds.height)}\" viewBox=\"0 0 {F(bounds.width)} {F(bounds.height)}\">\n");
            if (defs.Length > 0)
            {
                sb.Append("<defs>\n").Append(defs).Append("</defs>\n");
            }

            sb.Append(body);
            sb.Append("</svg>\n");

            var svg = sb.ToString();
            return new Result
            {
                Svg = svg,
                Hash = Hash(svg),
                Bounds = bounds,
                TintHex = mono ? colors.First() : null
            };
        }

        /// <summary>Rounded-rect outline with a dash pattern, which ProceduralImage can't draw.</summary>
        public static Result DashedRect(Vector2 size, Vector4 radius, string hex, float opacity, float width, string alignment)
        {
            var inset = alignment switch
            {
                "outer" => -width * 0.5f,
                "center" => 0f,
                _ => width * 0.5f
            };

            var pad = Mathf.Max(0f, -inset + width * 0.5f);
            var w = size.x + pad * 2f;
            var h = size.y + pad * 2f;
            var r = Mathf.Max(0f, radius.x - inset);
            var dash = $"{F(width * 3f)} {F(width * 2f)}";
            var svg =
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(w)}\" height=\"{F(h)}\" viewBox=\"0 0 {F(w)} {F(h)}\">\n" +
                $"<rect x=\"{F(pad + inset)}\" y=\"{F(pad + inset)}\" width=\"{F(size.x - inset * 2f)}\" height=\"{F(size.y - inset * 2f)}\" rx=\"{F(r)}\" " +
                $"fill=\"none\" stroke=\"#ffffff\" stroke-opacity=\"{F(opacity)}\" stroke-width=\"{F(width)}\" stroke-dasharray=\"{dash}\"/>\n</svg>\n";
            return new Result
            {
                Svg = svg,
                Hash = Hash(svg),
                Bounds = new Rect(-pad, -pad, w, h),
                TintHex = hex
            };
        }

        private float MaxStroke(JObject s)
        {
            if (PenpotDocument.Hidden(s))
            {
                return 0f;
            }

            var max = PenpotDocument.VisibleStrokes(s)
                .Select(st => PenpotDocument.Num(st["strokeWidth"], 1f) * ((string)st["strokeAlignment"] == "outer" ? 2f : 1f))
                .DefaultIfEmpty(0f).Max();
            foreach (var c in _doc.Children(s))
            {
                max = Mathf.Max(max, MaxStroke(c));
            }

            return max;
        }

        private void CollectPaints(JObject s, HashSet<string> colors, ref bool hasGradient)
        {
            if (PenpotDocument.Hidden(s))
            {
                return;
            }

            var type = PenpotDocument.Type(s);
            if (type != "group")
            {
                var fill = PenpotDocument.FirstVisibleFill(s);
                if (fill != null)
                {
                    if (fill["fillColorGradient"] != null)
                    {
                        hasGradient = true;
                    }
                    else if (fill["fillColor"] != null)
                    {
                        colors.Add(((string)fill["fillColor"]).ToLowerInvariant());
                    }
                }

                foreach (var st in PenpotDocument.VisibleStrokes(s))
                {
                    if (st["strokeColorGradient"] != null)
                    {
                        hasGradient = true;
                    }
                    else if (st["strokeColor"] != null)
                    {
                        colors.Add(((string)st["strokeColor"]).ToLowerInvariant());
                    }
                }
            }

            foreach (var c in _doc.Children(s))
            {
                CollectPaints(c, colors, ref hasGradient);
            }
        }

        private void Emit(JObject s, StringBuilder body, StringBuilder defs, Vector2 origin, bool mono, bool isRoot = false)
        {
            if (PenpotDocument.Hidden(s))
            {
                return;
            }

            var type = PenpotDocument.Type(s);
            var opacity = PenpotDocument.Opacity(s);
            var opacityAttr = opacity < 0.999f ? $" opacity=\"{F(opacity)}\"" : "";

            if (type == "group" || (type == "bool" && s["content"] == null))
            {
                body.Append($"<g{opacityAttr}>\n");
                foreach (var c in _doc.Children(s))
                {
                    Emit(c, body, defs, origin, mono);
                }

                body.Append("</g>\n");
                return;
            }

            var paint = Paint(s, defs, mono) + opacityAttr;
            var r = PenpotDocument.SelRect(s);
            switch (type)
            {
                case "path":
                case "bool":
                    var d = OffsetPath(s["content"]?.Type == JTokenType.String ? (string)s["content"] : "", origin);
                    var rule = (string)s["svgAttrs"]?["fillRule"];
                    var ruleAttr = string.IsNullOrEmpty(rule) ? "" : $" fill-rule=\"{rule}\"";
                    body.Append($"<path d=\"{d}\"{ruleAttr}{paint}/>\n");
                    break;
                case "circle":
                    body.Append($"<ellipse cx=\"{F(r.center.x - origin.x)}\" cy=\"{F(r.center.y - origin.y)}\" rx=\"{F(r.width * 0.5f)}\" ry=\"{F(r.height * 0.5f)}\"{Transform(s, origin)}{paint}/>\n");
                    break;
                case "rect":
                    var rad = PenpotDocument.Radius(s).x;
                    var rx = rad > 0f ? $" rx=\"{F(rad)}\"" : "";
                    body.Append($"<rect x=\"{F(r.x - origin.x)}\" y=\"{F(r.y - origin.y)}\" width=\"{F(r.width)}\" height=\"{F(r.height)}\"{rx}{Transform(s, origin)}{paint}/>\n");
                    break;
            }
        }

        private static string Transform(JObject s, Vector2 origin)
        {
            var t = s["transform"];
            if (t == null)
            {
                return "";
            }

            float a = PenpotDocument.Num(t["a"], 1f), b = PenpotDocument.Num(t["b"]), c = PenpotDocument.Num(t["c"]), d = PenpotDocument.Num(t["d"], 1f);
            if (Mathf.Approximately(a, 1f) && Mathf.Approximately(d, 1f) && Mathf.Approximately(b, 0f) && Mathf.Approximately(c, 0f))
            {
                return "";
            }

            // Penpot keeps the matrix relative to the shape center.
            var center = PenpotDocument.SelRect(s).center - origin;
            var e = center.x - a * center.x - c * center.y;
            var f = center.y - b * center.x - d * center.y;
            return $" transform=\"matrix({F(a)} {F(b)} {F(c)} {F(d)} {F(e)} {F(f)})\"";
        }

        private static string Paint(JObject s, StringBuilder defs, bool mono)
        {
            var sb = new StringBuilder();
            var fill = PenpotDocument.FirstVisibleFill(s);
            if (fill == null)
            {
                sb.Append(" fill=\"none\"");
            }
            else if (fill["fillColorGradient"] is JObject grad)
            {
                var id = "g" + defs.Length.ToString(Inv);
                defs.Append(Gradient(id, grad));
                sb.Append($" fill=\"url(#{id})\"");
                AppendOpacity(sb, "fill-opacity", PenpotDocument.Num(fill["fillOpacity"], 1f));
            }
            else
            {
                sb.Append($" fill=\"{(mono ? "#ffffff" : (string)fill["fillColor"])}\"");
                AppendOpacity(sb, "fill-opacity", PenpotDocument.Num(fill["fillOpacity"], 1f));
            }

            var stroke = PenpotDocument.VisibleStrokes(s).FirstOrDefault();
            if (stroke != null)
            {
                sb.Append($" stroke=\"{(mono ? "#ffffff" : (string)stroke["strokeColor"] ?? "#000000")}\"");
                AppendOpacity(sb, "stroke-opacity", PenpotDocument.Num(stroke["strokeOpacity"], 1f));
                sb.Append($" stroke-width=\"{F(PenpotDocument.Num(stroke["strokeWidth"], 1f))}\"");

                var cap = (string)stroke["strokeCapStart"] ?? (string)s["svgAttrs"]?["strokeLinecap"];
                if (cap is "round" or "square")
                {
                    sb.Append($" stroke-linecap=\"{cap}\"");
                }

                var join = (string)s["svgAttrs"]?["strokeLinejoin"];
                if (!string.IsNullOrEmpty(join))
                {
                    sb.Append($" stroke-linejoin=\"{join}\"");
                }

                var dash = (string)s["svgAttrs"]?["strokeDasharray"];
                if (!string.IsNullOrEmpty(dash))
                {
                    sb.Append($" stroke-dasharray=\"{dash}\"");
                }
                else if ((string)stroke["strokeStyle"] is "dashed" or "dotted")
                {
                    var w = PenpotDocument.Num(stroke["strokeWidth"], 1f);
                    sb.Append($" stroke-dasharray=\"{F(w * 3f)} {F(w * 2f)}\"");
                }
            }

            return sb.ToString();
        }

        private static void AppendOpacity(StringBuilder sb, string attr, float value)
        {
            if (value < 0.999f)
            {
                sb.Append($" {attr}=\"{F(value)}\"");
            }
        }

        private static string Gradient(string id, JObject g)
        {
            var sb = new StringBuilder();
            var radial = (string)g["type"] == "radial";
            if (radial)
            {
                sb.Append($"<radialGradient id=\"{id}\" cx=\"{F(PenpotDocument.Num(g["startX"]))}\" cy=\"{F(PenpotDocument.Num(g["startY"]))}\" r=\"0.5\">\n");
            }
            else
            {
                sb.Append($"<linearGradient id=\"{id}\" x1=\"{F(PenpotDocument.Num(g["startX"]))}\" y1=\"{F(PenpotDocument.Num(g["startY"]))}\" " +
                          $"x2=\"{F(PenpotDocument.Num(g["endX"], 1f))}\" y2=\"{F(PenpotDocument.Num(g["endY"], 1f))}\">\n");
            }

            foreach (var stop in g["stops"] ?? new JArray())
            {
                var op = PenpotDocument.Num(stop["opacity"], 1f);
                var opAttr = op < 0.999f ? $" stop-opacity=\"{F(op)}\"" : "";
                sb.Append($"<stop offset=\"{F(PenpotDocument.Num(stop["offset"]))}\" stop-color=\"{(string)stop["color"]}\"{opAttr}/>\n");
            }

            sb.Append(radial ? "</radialGradient>\n" : "</linearGradient>\n");
            return sb.ToString();
        }

        /// <summary>Penpot path data uses absolute M/L/C/Z commands: shift every point by -origin.</summary>
        private static string OffsetPath(string d, Vector2 origin)
        {
            var sb = new StringBuilder();
            var isX = true;
            foreach (Match m in PathToken.Matches(d))
            {
                var tok = m.Value;
                if (char.IsLetter(tok[0]))
                {
                    sb.Append(tok);
                    isX = true;
                    continue;
                }

                var v = float.Parse(tok, Inv) - (isX ? origin.x : origin.y);
                if (sb.Length > 0 && !char.IsLetter(sb[sb.Length - 1]))
                {
                    sb.Append(' ');
                }

                sb.Append(F(v));
                isX = !isX;
            }

            return sb.ToString();
        }

        private static string F(float v) => (Mathf.Round(v * 100f) / 100f).ToString("0.##", Inv);

        private static string Hash(string text)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            return string.Concat(bytes.Take(4).Select(b => b.ToString("x2")));
        }
    }
}
